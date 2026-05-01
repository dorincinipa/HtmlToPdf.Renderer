using HtmlToPdf.Renderer.Adapters;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Security;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;

namespace HtmlToPdf.Renderer;

public static class PdfGenerator
{
    public static PdfBuilder Create() => new();

    public static void LoadFontsFromFolder(string folderPath)
        => FontResolver.Instance.LoadFontsFromFolder(folderPath);

    public static PdfDocument GeneratePdf(string html, PdfOptions options)
    {
        var document = new PdfDocument();
        ApplySecurity(document, options.Security);
        AddPdfPages(document, html, options);
        return document;
    }

    public static async Task<byte[]> GeneratePdfAsync(string html, PdfOptions options)
    {
        using var document = GeneratePdf(html, options);
        using var stream = new MemoryStream();
        await document.SaveAsync(stream, false);
        return stream.ToArray();
    }

    public static void AddPdfPages(PdfDocument document, string html, PdfOptions options)
    {
        var adapter = RenderAdapter.Instance;
        var effectivePageSize = options.GetEffectivePageSize();

        double pageWidth = effectivePageSize.Width;
        double pageHeight = effectivePageSize.Height;
        double contentWidth = pageWidth - options.MarginLeft - options.MarginRight;
        double contentHeight = pageHeight - options.MarginTop - options.MarginBottom;

        using var container = new HtmlContainer(adapter);
        container.SetHtml(html);

        container.PageSize = new RSize(pageWidth, pageHeight);
        container.MarginTop = (int)Math.Round(options.MarginTop);
        container.MarginBottom = (int)Math.Round(options.MarginBottom);
        container.MarginLeft = (int)Math.Round(options.MarginLeft);
        container.MarginRight = (int)Math.Round(options.MarginRight);
        container.MaxSize = new RSize(contentWidth, 0);
        container.Location = new RPoint(options.MarginLeft, options.MarginTop);

        using var measureContext = XGraphics.CreateMeasureContext(new XSize(pageWidth, pageHeight), XGraphicsUnit.Point, XPageDirection.Downwards);
        using var measureGraphics = new GraphicsAdapter(measureContext, adapter, new RRect(0, 0, pageWidth, pageHeight));
        container.PerformLayout(measureGraphics);

        double totalHeight = container.ActualSize.Height;
        int pageCount = Math.Max(1, (int)Math.Ceiling(totalHeight / contentHeight));

        container.MaxSize = new RSize(contentWidth, contentHeight);

        for (int i = 0; i < pageCount; i++)
        {
            var page = document.AddPage();
            page.Width = XUnit.FromPoint(pageWidth);
            page.Height = XUnit.FromPoint(pageHeight);

            using var xGraphics = XGraphics.FromPdfPage(page);
            using var pageGraphics = new GraphicsAdapter(
                xGraphics, adapter, new RRect(0, 0, pageWidth, pageHeight));

            container.Location = new RPoint(options.MarginLeft, options.MarginTop - i * contentHeight);
            container.PerformLayout(pageGraphics);
            container.PerformPaint(pageGraphics);

            if (!string.IsNullOrEmpty(options.Header))
                RenderHeaderFooter(pageGraphics, adapter,
                    Substitute(options.Header, i + 1, pageCount),
                    options.MarginLeft, 0, contentWidth, pageWidth);

            if (!string.IsNullOrEmpty(options.Footer))
                RenderHeaderFooter(pageGraphics, adapter,
                    Substitute(options.Footer, i + 1, pageCount),
                    options.MarginLeft, pageHeight - options.MarginBottom, contentWidth, pageWidth);

            if (options.Stamp != null)
                DrawStamp(xGraphics, options.Stamp, pageWidth, pageHeight);
        }
    }

    private static string Substitute(string html, int pageNumber, int totalPages) =>
        html.Replace("{{PageNumber}}", pageNumber.ToString())
            .Replace("{{TotalPages}}", totalPages.ToString());

    private static void RenderHeaderFooter(
        GraphicsAdapter gfx, RenderAdapter adapter,
        string html, double x, double y, double width, double pageWidth)
    {
        using var container = new HtmlContainer(adapter);
        container.SetHtml(html);
        container.PageSize = new RSize(pageWidth, 100_000);
        container.MaxSize = new RSize(width, 0);
        container.Location = new RPoint(x, y);
        container.PerformLayout(gfx);
        container.PerformPaint(gfx);
    }

    private static void DrawStamp(XGraphics g, StampOptions stamp, double pageWidth, double pageHeight)
    {
        var state = g.Save();
        try
        {
            if (stamp.Text != null)
                DrawTextStamp(g, stamp, pageWidth, pageHeight);
            else if (stamp.ImageData != null)
                DrawImageStamp(g, stamp, pageWidth, pageHeight);
        }
        finally
        {
            g.Restore(state);
        }
    }

    private static void DrawTextStamp(XGraphics g, StampOptions stamp, double pageWidth, double pageHeight)
    {
        var font = new XFont("Arial", stamp.FontSize, XFontStyleEx.Bold);
        var base_ = stamp.Color;
        var color = XColor.FromArgb(
            (int)(stamp.Opacity * 255),
            (int)(base_.R * 255),
            (int)(base_.G * 255),
            (int)(base_.B * 255));
        var brush = new XSolidBrush(color);

        g.TranslateTransform(pageWidth / 2, pageHeight / 2);
        g.RotateTransform(stamp.Angle);

        var size = g.MeasureString(stamp.Text!, font);
        g.DrawString(stamp.Text!, font, brush,
            new XRect(-size.Width / 2, -size.Height / 2, size.Width, size.Height),
            XStringFormats.Center);
    }

    private const double StampEdgeMargin = 20;

    private static void DrawImageStamp(XGraphics g, StampOptions stamp, double pageWidth, double pageHeight)
    {
        using var xImage = XImage.FromStream(new MemoryStream(stamp.ImageData!));

        double w = stamp.ImageWidth  > 0 ? stamp.ImageWidth  : xImage.PointWidth;
        double h = stamp.ImageHeight > 0 ? stamp.ImageHeight : xImage.PointHeight;

        var (x, y) = stamp.Position switch
        {
            StampPosition.TopLeft     => (StampEdgeMargin, StampEdgeMargin),
            StampPosition.TopRight    => (pageWidth  - w - StampEdgeMargin, StampEdgeMargin),
            StampPosition.BottomLeft  => (StampEdgeMargin, pageHeight - h - StampEdgeMargin),
            StampPosition.BottomRight => (pageWidth  - w - StampEdgeMargin, pageHeight - h - StampEdgeMargin),
            _                         => ((pageWidth - w) / 2, (pageHeight - h) / 2),
        };

        if (stamp.Angle != 0)
        {
            g.TranslateTransform(x + w / 2, y + h / 2);
            g.RotateTransform(stamp.Angle);
            g.DrawImage(xImage, new XRect(-w / 2, -h / 2, w, h));
        }
        else
        {
            g.DrawImage(xImage, new XRect(x, y, w, h));
        }
    }

    private static void ApplySecurity(PdfDocument document, PdfSecurityOptions? security)
    {
        if (security is null) return;

        if (string.IsNullOrEmpty(security.UserPassword) &&
            string.IsNullOrEmpty(security.OwnerPassword))
        {
            throw new InvalidOperationException(
                "PdfSecurityOptions requires at least UserPassword or OwnerPassword.");
        }

        var s = document.SecuritySettings;
        if (!string.IsNullOrEmpty(security.UserPassword))  s.UserPassword  = security.UserPassword;
        if (!string.IsNullOrEmpty(security.OwnerPassword)) s.OwnerPassword = security.OwnerPassword;

        var p = security.Permissions;
        s.PermitPrint            = p.HasFlag(PdfPermissions.Print);
        s.PermitFullQualityPrint = p.HasFlag(PdfPermissions.HighQualityPrint);
        s.PermitModifyDocument   = p.HasFlag(PdfPermissions.ModifyContent);
        s.PermitExtractContent   = p.HasFlag(PdfPermissions.CopyContent);
        s.PermitAnnotations      = p.HasFlag(PdfPermissions.Annotate);
        s.PermitFormsFill        = p.HasFlag(PdfPermissions.FillForms);
        s.PermitAssembleDocument = p.HasFlag(PdfPermissions.AssembleDocument);
    }
}

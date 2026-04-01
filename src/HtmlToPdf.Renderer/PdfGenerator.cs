using HtmlToPdf.Renderer.Adapters;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlToPdf.Renderer;

public static class PdfGenerator
{
    public static PdfBuilder Create() => new();

    public static PdfDocument GeneratePdf(string html, PdfOptions options)
    {
        var document = new PdfDocument();
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
        }
    }
}

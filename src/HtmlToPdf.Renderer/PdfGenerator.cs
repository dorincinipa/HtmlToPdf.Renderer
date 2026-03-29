using HtmlToPdf.Renderer.Adapters;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlToPdf.Renderer;

public static class PdfGenerator
{
    public static PdfDocument GeneratePdf(string html, PdfGenerateConfig config)
    {
        var document = new PdfDocument();
        var adapter = PdfSharpAdapter.Instance;
        var effectivePageSize = config.GetEffectivePageSize();

        double pageWidth = effectivePageSize.Width;
        double pageHeight = effectivePageSize.Height;
        double contentWidth = pageWidth - config.MarginLeft - config.MarginRight;
        double contentHeight = pageHeight - config.MarginTop - config.MarginBottom;

        using var container = new HtmlContainer(adapter);
        container.SetHtml(html);

        // Configure pagination properties
        container.PageSize = new RSize(pageWidth, pageHeight);
        container.MarginTop = (int)Math.Round(config.MarginTop);
        container.MarginBottom = (int)Math.Round(config.MarginBottom);
        container.MarginLeft = (int)Math.Round(config.MarginLeft);
        container.MarginRight = (int)Math.Round(config.MarginRight);
        container.MaxSize = new RSize(contentWidth, 0); // unlimited height for measure
        container.Location = new RPoint(config.MarginLeft, config.MarginTop);

        // Measure pass
        using var measureContext = XGraphics.CreateMeasureContext(new XSize(pageWidth, pageHeight), XGraphicsUnit.Point, XPageDirection.Downwards);
        using var measureGraphics = new GraphicsAdapter(measureContext, adapter, new RRect(0, 0, pageWidth, pageHeight));
        container.PerformLayout(measureGraphics);

        double totalHeight = container.ActualSize.Height;
        int pageCount = Math.Max(1, (int)Math.Ceiling(totalHeight / contentHeight));

        // Enable height-based clipping for page rendering
        container.MaxSize = new RSize(contentWidth, contentHeight);

        // Page loop
        for (int i = 0; i < pageCount; i++)
        {
            var page = document.AddPage();
            page.Width = XUnit.FromPoint(pageWidth);
            page.Height = XUnit.FromPoint(pageHeight);

            using var xGraphics = XGraphics.FromPdfPage(page);
            using var pageGraphics = new GraphicsAdapter(
                xGraphics, adapter, new RRect(0, 0, pageWidth, pageHeight));

            // Offset content upward for subsequent pages
            container.Location = new RPoint(config.MarginLeft, config.MarginTop - i * contentHeight);

            // PerformLayout propagates Location to the root element
            container.PerformLayout(pageGraphics);
            // PerformPaint alone only uses it for the clip rect
            container.PerformPaint(pageGraphics);
        }

        return document;
    }
}

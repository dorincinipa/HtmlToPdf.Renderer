using HtmlToPdf.Renderer;
using PdfSharp.Pdf;

namespace HtmlToPdf.Api;

internal sealed class HtmlToPdfGenerator : IPdfGenerator
{
    private readonly PdfOptions _defaults;

    internal HtmlToPdfGenerator(PdfOptions defaults)
    {
        _defaults = defaults;
    }

    public PdfDocument GeneratePdf(string html)
        => PdfGenerator.GeneratePdf(html, _defaults);

    public PdfDocument GeneratePdf(string html, PdfOptions options)
        => PdfGenerator.GeneratePdf(html, options);

    public Task<byte[]> GeneratePdfAsync(string html)
        => PdfGenerator.GeneratePdfAsync(html, _defaults);

    public Task<byte[]> GeneratePdfAsync(string html, PdfOptions options)
        => PdfGenerator.GeneratePdfAsync(html, options);

    public void AddPdfPages(PdfDocument document, string html)
        => PdfGenerator.AddPdfPages(document, html, _defaults);

    public void AddPdfPages(PdfDocument document, string html, PdfOptions options)
        => PdfGenerator.AddPdfPages(document, html, options);
}

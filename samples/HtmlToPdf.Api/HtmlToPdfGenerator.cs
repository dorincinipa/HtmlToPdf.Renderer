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

    public PdfDocument GeneratePdf(string html, IRenderingOptions options)
        => PdfGenerator.GeneratePdf(html, options);

    public PdfDocument GeneratePdf(IEnumerable<string> htmlPages)
        => PdfGenerator.GeneratePdf(htmlPages, _defaults);

    public PdfDocument GeneratePdf(IEnumerable<string> htmlPages, IRenderingOptions options)
        => PdfGenerator.GeneratePdf(htmlPages, options);

    public Task<byte[]> GeneratePdfAsync(string html, CancellationToken ct = default)
        => PdfGenerator.GeneratePdfAsync(html, _defaults, ct);

    public Task<byte[]> GeneratePdfAsync(string html, IRenderingOptions options, CancellationToken ct = default)
        => PdfGenerator.GeneratePdfAsync(html, options, ct);

    public Task<byte[]> GeneratePdfAsync(IEnumerable<string> htmlPages, CancellationToken ct = default)
        => PdfGenerator.GeneratePdfAsync(htmlPages, _defaults, ct);

    public Task<byte[]> GeneratePdfAsync(IEnumerable<string> htmlPages, IRenderingOptions options, CancellationToken ct = default)
        => PdfGenerator.GeneratePdfAsync(htmlPages, options, ct);

    public PdfGenerateResult GeneratePdfWithResult(string html)
        => PdfGenerator.GeneratePdfWithResult(html, _defaults);

    public PdfGenerateResult GeneratePdfWithResult(string html, IRenderingOptions options)
        => PdfGenerator.GeneratePdfWithResult(html, options);

    public void AddPdfPages(PdfDocument document, string html)
        => PdfGenerator.AddPdfPages(document, html, _defaults);

    public void AddPdfPages(PdfDocument document, string html, IRenderingOptions options)
        => PdfGenerator.AddPdfPages(document, html, options);

    public PdfDocument AppendHtml(byte[] existingPdf, string html)
        => PdfGenerator.AppendHtml(existingPdf, html, _defaults);

    public PdfDocument AppendHtml(byte[] existingPdf, string html, IRenderingOptions options)
        => PdfGenerator.AppendHtml(existingPdf, html, options);

    public Task<byte[]> AppendHtmlAsync(byte[] existingPdf, string html, CancellationToken ct = default)
        => PdfGenerator.AppendHtmlAsync(existingPdf, html, _defaults, ct);

    public Task<byte[]> AppendHtmlAsync(byte[] existingPdf, string html, IRenderingOptions options, CancellationToken ct = default)
        => PdfGenerator.AppendHtmlAsync(existingPdf, html, options, ct);

    public PdfDocument GeneratePdfFromFile(string filePath)
        => PdfGenerator.GeneratePdfFromFile(filePath, _defaults);

    public PdfDocument GeneratePdfFromFile(string filePath, PdfOptions options)
        => PdfGenerator.GeneratePdfFromFile(filePath, options);

    public Task<byte[]> GeneratePdfFromFileAsync(string filePath, CancellationToken ct = default)
        => PdfGenerator.GeneratePdfFromFileAsync(filePath, _defaults, ct);

    public Task<byte[]> GeneratePdfFromFileAsync(string filePath, PdfOptions options, CancellationToken ct = default)
        => PdfGenerator.GeneratePdfFromFileAsync(filePath, options, ct);
}

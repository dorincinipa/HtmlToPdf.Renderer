using PdfSharp.Pdf;

namespace HtmlToPdf.Renderer;

public interface IPdfGenerator
{
    PdfDocument GeneratePdf(string html);
    PdfDocument GeneratePdf(string html, IRenderingOptions options);
    PdfDocument GeneratePdf(IEnumerable<string> htmlPages);
    PdfDocument GeneratePdf(IEnumerable<string> htmlPages, IRenderingOptions options);
    Task<byte[]> GeneratePdfAsync(string html, CancellationToken ct = default);
    Task<byte[]> GeneratePdfAsync(string html, IRenderingOptions options, CancellationToken ct = default);
    Task<byte[]> GeneratePdfAsync(IEnumerable<string> htmlPages, CancellationToken ct = default);
    Task<byte[]> GeneratePdfAsync(IEnumerable<string> htmlPages, IRenderingOptions options, CancellationToken ct = default);
    PdfGenerateResult GeneratePdfWithResult(string html);
    PdfGenerateResult GeneratePdfWithResult(string html, IRenderingOptions options);
    void AddPdfPages(PdfDocument document, string html);
    void AddPdfPages(PdfDocument document, string html, IRenderingOptions options);
    PdfDocument AppendHtml(byte[] existingPdf, string html);
    PdfDocument AppendHtml(byte[] existingPdf, string html, IRenderingOptions options);
    Task<byte[]> AppendHtmlAsync(byte[] existingPdf, string html, CancellationToken ct = default);
    Task<byte[]> AppendHtmlAsync(byte[] existingPdf, string html, IRenderingOptions options, CancellationToken ct = default);
    PdfDocument GeneratePdfFromFile(string filePath);
    PdfDocument GeneratePdfFromFile(string filePath, PdfOptions options);
    Task<byte[]> GeneratePdfFromFileAsync(string filePath, CancellationToken ct = default);
    Task<byte[]> GeneratePdfFromFileAsync(string filePath, PdfOptions options, CancellationToken ct = default);
}

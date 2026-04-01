using PdfSharp.Pdf;

namespace HtmlToPdf.Renderer;

public interface IPdfGenerator
{
    PdfDocument GeneratePdf(string html);
    PdfDocument GeneratePdf(string html, PdfOptions options);
    Task<byte[]> GeneratePdfAsync(string html);
    Task<byte[]> GeneratePdfAsync(string html, PdfOptions options);
    void AddPdfPages(PdfDocument document, string html);
    void AddPdfPages(PdfDocument document, string html, PdfOptions options);
}

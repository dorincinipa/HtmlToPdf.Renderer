using PdfSharp.Pdf;

namespace HtmlToPdf.Renderer;

public sealed class PdfGenerateResult : IDisposable
{
    private bool _disposed;

    public PdfDocument Document { get; }
    public int PageCount { get; }
    public TimeSpan RenderTime { get; }
    public IReadOnlyList<PdfRenderError> Warnings { get; }

    internal PdfGenerateResult(PdfDocument document, int pageCount, TimeSpan renderTime, IReadOnlyList<PdfRenderError> warnings)
    {
        Document = document;
        PageCount = pageCount;
        RenderTime = renderTime;
        Warnings = warnings;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Document.Dispose();
            _disposed = true;
        }
    }
}

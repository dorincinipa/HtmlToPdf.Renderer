using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer;

public interface IRenderingOptions
{
    XSize PageSize { get; }
    double MarginTop { get; }
    double MarginBottom { get; }
    double MarginLeft { get; }
    double MarginRight { get; }
    PageOrientation PageOrientation { get; }
    PdfSecurityOptions? Security { get; }
    string? Title { get; }
    string? Author { get; }
    string? Subject { get; }
    string? Keywords { get; }
    string? Header { get; }
    string? Footer { get; }
    StampOptions? Stamp { get; }
    Dictionary<string, string>? StyleSheets { get; }
    ILogger? Logger { get; }
    string? BaseDirectory { get; }
    Action<int, int>? OnProgress { get; }
    Action<PdfRenderError>? OnRenderError { get; }
    bool ContinueOnError { get; }
    PdfCompliance Compliance { get; }
    byte[]? IccProfileBytes { get; }
    XSize GetEffectivePageSize();
}

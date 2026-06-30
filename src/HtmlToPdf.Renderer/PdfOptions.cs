using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer;

public enum PageOrientation
{
    Portrait,
    Landscape
}

public class PdfOptions : IRenderingOptions
{
    public XSize PageSize { get; set; } = Renderer.PageSize.A4;
    public double MarginTop { get; set; }
    public double MarginBottom { get; set; }
    public double MarginLeft { get; set; }
    public double MarginRight { get; set; }
    public PageOrientation PageOrientation { get; set; } = PageOrientation.Portrait;
    public double DefaultMargin { get; set; }
    public string? FontFolder { get; set; }
    public PdfSecurityOptions? Security { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Subject { get; set; }
    public string? Keywords { get; set; }
    public string? Header { get; set; }
    public string? Footer { get; set; }
    public StampOptions? Stamp { get; set; }
    public Dictionary<string, string>? StyleSheets { get; set; }
    public ILogger? Logger { get; set; }
    public string? BaseDirectory { get; set; }
    public Action<int, int>? OnProgress { get; set; }
    public Action<PdfRenderError>? OnRenderError { get; set; }
    public bool ContinueOnError { get; set; }
    public PdfCompliance Compliance { get; set; }
    public byte[]? IccProfileBytes { get; set; }

    internal PdfOptions CloneWithBaseDirectory(string baseDirectory)
    {
        var clone = (PdfOptions)MemberwiseClone();
        clone.BaseDirectory = baseDirectory;
        return clone;
    }

    public void SetMargins(double all)
    {
        MarginTop = MarginBottom = MarginLeft = MarginRight = all;
    }

    public void SetMargins(double vertical, double horizontal)
    {
        MarginTop = MarginBottom = vertical;
        MarginLeft = MarginRight = horizontal;
    }

    public XSize GetEffectivePageSize()
    {
        return PageOrientation == PageOrientation.Landscape
            ? new XSize(PageSize.Height, PageSize.Width)
            : PageSize;
    }
}

using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer;

public enum PageOrientation
{
    Portrait,
    Landscape
}

public class PdfGenerateConfig
{
    public XSize PageSize { get; set; } = PageSizes.A4;
    public double MarginTop { get; set; }
    public double MarginBottom { get; set; }
    public double MarginLeft { get; set; }
    public double MarginRight { get; set; }
    public PageOrientation PageOrientation { get; set; } = PageOrientation.Portrait;

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

    public static class PageSizes
    {
        public static readonly XSize A4 = new(595.276, 841.890);
        public static readonly XSize Letter = new(612, 792);
        public static readonly XSize Legal = new(612, 1008);
        public static readonly XSize A3 = new(841.890, 1190.551);
    }
}

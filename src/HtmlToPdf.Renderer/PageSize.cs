using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer;

public static class PageSize
{
    public static readonly XSize A4 = new(595.276, 841.890);
    public static readonly XSize Letter = new(612, 792);
    public static readonly XSize Legal = new(612, 1008);
    public static readonly XSize A3 = new(841.890, 1190.551);
}

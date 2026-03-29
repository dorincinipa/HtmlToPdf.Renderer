using PdfSharp.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace HtmlToPdf.Renderer.Adapters;

public sealed class FontFamilyAdapter : RFontFamily
{
    internal XFontFamily FontFamily { get; }

    public FontFamilyAdapter(XFontFamily fontFamily)
    {
        FontFamily = fontFamily;
    }

    public override string Name => FontFamily.Name;
}

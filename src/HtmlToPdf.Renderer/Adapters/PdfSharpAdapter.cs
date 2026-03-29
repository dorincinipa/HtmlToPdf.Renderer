using PdfSharp.Drawing;
using PdfSharp.Fonts;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlToPdf.Renderer.Adapters;

public sealed class PdfSharpAdapter : RAdapter
{
    private static readonly Lazy<PdfSharpAdapter> _instance = new(() => new PdfSharpAdapter());

    public static PdfSharpAdapter Instance => _instance.Value;

    static PdfSharpAdapter()
    {
        // PdfSharp 6.2.x cross-platform build requires explicit font resolver config.
        // Enable system font resolution on Windows; other platforms need a custom IFontResolver.
        // Only effective for Core build on Windows; no-op otherwise.
        GlobalFontSettings.UseWindowsFontsUnderWindows = true;
    }

    private PdfSharpAdapter() { }

    protected override RColor GetColorInt(string colorName)
    {
        if (ColorMap.TryGetColor(colorName, out var color))
            return color;

        return RColor.FromArgb(0, 0, 0, 0);
    }

    protected override RPen CreatePen(RColor color)
    {
        return new PenAdapter(new XPen(ToXColor(color)));
    }

    protected override RBrush CreateSolidBrush(RColor color)
    {
        return new PdfSharpBrushAdapter(new XSolidBrush(ToXColor(color)));
    }

    protected override RBrush CreateLinearGradientBrush(RRect rect,
        RColor color1, RColor color2, double angle)
    {
        var xBrush = new XLinearGradientBrush(
            new XRect(rect.X, rect.Y, rect.Width, rect.Height),
            ToXColor(color1), ToXColor(color2), XLinearGradientMode.Horizontal);
        return new PdfSharpBrushAdapter(xBrush);
    }

    protected override RImage ConvertImageInt(object image)
    {
        if (image is XImage xImage)
            return new ImageAdapter(xImage);
        return null!;
    }

    protected override RImage ImageFromStreamInt(Stream memoryStream)
    {
        return new ImageAdapter(XImage.FromStream(memoryStream));
    }

    // Fonts known to be available via PdfSharp on Windows used as fallback candidates.
    private static readonly string[] _fallbackFonts = { "Arial", "Verdana", "Courier New", "Times New Roman" };

    protected override RFont CreateFontInt(string family, double size, RFontStyle style)
    {
        // Only map Bold/Italic to XFont — HtmlRenderer.Core draws underline/strikeout separately
        var xStyle = (XFontStyleEx)(int)(style & (RFontStyle.Bold | RFontStyle.Italic));
        try
        {
            return new FontAdapter(new XFont(family, size, xStyle));
        }
        catch
        {
            // Requested font not available — try fallback fonts
            foreach (var fallback in _fallbackFonts)
            {
                try
                {
                    return new FontAdapter(new XFont(fallback, size, xStyle));
                }
                catch { }
            }
            throw;
        }
    }

    protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
    {
        return CreateFontInt(family.Name, size, style);
    }

    internal static XColor ToXColor(RColor c) => XColor.FromArgb(c.A, c.R, c.G, c.B);
}

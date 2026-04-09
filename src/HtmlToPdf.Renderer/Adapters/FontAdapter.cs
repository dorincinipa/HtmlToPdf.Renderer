using PdfSharp.Drawing;
using HtmlToPdf.Renderer.HtmlEngine.Adapters;

namespace HtmlToPdf.Renderer.Adapters;

public sealed class FontAdapter : RFont
{
    internal XFont XFont { get; }

    private double? _height;
    private double? _underlineOffset;
    private double? _whitespaceWidth;

    public FontAdapter(XFont font)
    {
        XFont = font;
    }

    public override double Size => XFont.Size;

    public override double Height
    {
        get
        {
            if (_height == null)
            {
                var unitsPerEm = (double)XFont.Metrics.UnitsPerEm;
                _height = (XFont.CellAscent + XFont.CellDescent) / unitsPerEm * XFont.Size;
            }
            return _height.Value;
        }
    }

    public override double UnderlineOffset
    {
        get
        {
            if (_underlineOffset == null)
            {
                var metrics = XFont.Metrics;
                var unitsPerEm = (double)metrics.UnitsPerEm;
                // UnderlinePosition is below baseline — we want positive offset from top
                _underlineOffset = XFont.CellAscent / unitsPerEm * XFont.Size
                                   + Math.Abs(metrics.UnderlinePosition) / unitsPerEm * XFont.Size;
            }
            return _underlineOffset.Value;
        }
    }

    public override double LeftPadding => 0;

    public override double GetWhitespaceWidth(RGraphics graphics)
    {
        if (_whitespaceWidth == null)
        {
            var g = ((GraphicsAdapter)graphics).XGraphics;
            _whitespaceWidth = g.MeasureString(" ", XFont).Width;
        }
        return _whitespaceWidth.Value;
    }
}

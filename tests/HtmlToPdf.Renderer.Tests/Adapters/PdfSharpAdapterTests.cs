using HtmlToPdf.Renderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlToPdf.Renderer.Tests.Adapters;

public class PdfSharpAdapterTests
{
    [Fact]
    public void Instance_ReturnsSingleton()
    {
        var a = PdfSharpAdapter.Instance;
        var b = PdfSharpAdapter.Instance;
        Assert.Same(a, b);
    }

    [Fact]
    public void GetPen_ReturnsPdfSharpPenAdapter()
    {
        var pen = PdfSharpAdapter.Instance.GetPen(RColor.FromArgb(255, 0, 0, 0));
        Assert.IsType<PenAdapter>(pen);
    }

    [Fact]
    public void GetPen_CachesSameColor()
    {
        var color = RColor.FromArgb(255, 128, 64, 32);
        var pen1 = PdfSharpAdapter.Instance.GetPen(color);
        var pen2 = PdfSharpAdapter.Instance.GetPen(color);
        Assert.Same(pen1, pen2);
    }

    [Fact]
    public void GetSolidBrush_ReturnsPdfSharpBrushAdapter()
    {
        var brush = PdfSharpAdapter.Instance.GetSolidBrush(RColor.FromArgb(255, 255, 0, 0));
        Assert.IsType<PdfSharpBrushAdapter>(brush);
    }

    [Fact]
    public void GetColor_ParsesNamedColor()
    {
        var color = PdfSharpAdapter.Instance.GetColor("red");
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void GetFont_ReturnsPdfSharpFontAdapter()
    {
        var font = PdfSharpAdapter.Instance.GetFont("Arial", 12, RFontStyle.Regular);
        Assert.IsType<FontAdapter>(font);
    }

    [Fact]
    public void GetFont_BoldItalic_MapsStyles()
    {
        var font = PdfSharpAdapter.Instance.GetFont("Arial", 14,
            RFontStyle.Bold | RFontStyle.Italic);
        Assert.IsType<FontAdapter>(font);
        Assert.Equal(14, font.Size);
    }
}

using HtmlToPdf.Renderer.Adapters;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;

namespace HtmlToPdf.Renderer.Tests.Adapters;

public class RenderAdapterTests
{
    [Fact]
    public void Instance_ReturnsSingleton()
    {
        var a = RenderAdapter.Instance;
        var b = RenderAdapter.Instance;
        Assert.Same(a, b);
    }

    [Fact]
    public void GetPen_ReturnsPenAdapter()
    {
        var pen = RenderAdapter.Instance.GetPen(RColor.FromArgb(255, 0, 0, 0));
        Assert.IsType<PenAdapter>(pen);
    }

    [Fact]
    public void GetPen_CachesSameColor()
    {
        var color = RColor.FromArgb(255, 128, 64, 32);
        var pen1 = RenderAdapter.Instance.GetPen(color);
        var pen2 = RenderAdapter.Instance.GetPen(color);
        Assert.Same(pen1, pen2);
    }

    [Fact]
    public void GetSolidBrush_ReturnsBrushAdapter()
    {
        var brush = RenderAdapter.Instance.GetSolidBrush(RColor.FromArgb(255, 255, 0, 0));
        Assert.IsType<BrushAdapter>(brush);
    }

    [Fact]
    public void GetColor_ParsesNamedColor()
    {
        var color = RenderAdapter.Instance.GetColor("red");
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void GetFont_ReturnsFontAdapter()
    {
        var font = RenderAdapter.Instance.GetFont("Arial", 12, RFontStyle.Regular);
        Assert.IsType<FontAdapter>(font);
    }

    [Fact]
    public void GetFont_BoldItalic_MapsStyles()
    {
        var font = RenderAdapter.Instance.GetFont("Arial", 14,
            RFontStyle.Bold | RFontStyle.Italic);
        Assert.IsType<FontAdapter>(font);
        Assert.Equal(14, font.Size);
    }
}

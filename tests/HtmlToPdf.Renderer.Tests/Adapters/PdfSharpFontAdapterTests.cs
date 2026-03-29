using HtmlToPdf.Renderer.Adapters;
using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer.Tests.Adapters;

public class PdfSharpFontAdapterTests
{
    [Fact]
    public void Size_ReturnsXFontSize()
    {
        var xFont = new XFont("Arial", 12, XFontStyleEx.Regular);
        var adapter = new FontAdapter(xFont);
        Assert.Equal(12, adapter.Size);
    }

    [Fact]
    public void Height_IsPositive()
    {
        var xFont = new XFont("Arial", 12, XFontStyleEx.Regular);
        var adapter = new FontAdapter(xFont);
        Assert.True(adapter.Height > 0);
    }

    [Fact]
    public void UnderlineOffset_IsPositive()
    {
        var xFont = new XFont("Arial", 12, XFontStyleEx.Regular);
        var adapter = new FontAdapter(xFont);
        Assert.True(adapter.UnderlineOffset > 0);
    }

    [Fact]
    public void LeftPadding_IsNonNegative()
    {
        var xFont = new XFont("Arial", 12, XFontStyleEx.Regular);
        var adapter = new FontAdapter(xFont);
        Assert.True(adapter.LeftPadding >= 0);
    }
}

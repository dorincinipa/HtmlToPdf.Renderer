using HtmlToPdf.Renderer.Adapters;
using PdfSharp.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlToPdf.Renderer.Tests.Adapters;

public class PdfSharpPenAdapterTests
{
    [Fact]
    public void Constructor_WrapsXPen()
    {
        var xPen = new XPen(XColors.Black);
        var adapter = new PenAdapter(xPen);

        Assert.Same(xPen, adapter.XPen);
    }

    [Fact]
    public void Width_DelegatesToXPen()
    {
        var xPen = new XPen(XColors.Black, 2.5);
        var adapter = new PenAdapter(xPen);

        Assert.Equal(2.5, adapter.Width);

        adapter.Width = 5.0;
        Assert.Equal(5.0, xPen.Width);
    }

    [Fact]
    public void DashStyle_DelegatesToXPen()
    {
        var xPen = new XPen(XColors.Black);
        var adapter = new PenAdapter(xPen);

        adapter.DashStyle = RDashStyle.Dash;
        Assert.Equal(XDashStyle.Dash, xPen.DashStyle);
    }
}

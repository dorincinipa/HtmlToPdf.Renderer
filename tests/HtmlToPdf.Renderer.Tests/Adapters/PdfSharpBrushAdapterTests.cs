using HtmlToPdf.Renderer.Adapters;
using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer.Tests.Adapters;

public class PdfSharpBrushAdapterTests
{
    [Fact]
    public void Constructor_WrapsXBrush()
    {
        var xBrush = new XSolidBrush(XColors.Red);
        var adapter = new PdfSharpBrushAdapter(xBrush);

        Assert.Same(xBrush, adapter.XBrush);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var adapter = new PdfSharpBrushAdapter(new XSolidBrush(XColors.Blue));
        var ex = Record.Exception(() => adapter.Dispose());
        Assert.Null(ex);
    }
}

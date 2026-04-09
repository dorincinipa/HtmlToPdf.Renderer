using HtmlToPdf.Renderer.Adapters;
using PdfSharp.Drawing;
using HtmlToPdf.Renderer.HtmlEngine.Adapters;

namespace HtmlToPdf.Renderer.Tests.Adapters;

public class PathAdapterTests
{
    [Fact]
    public void Start_LineTo_CreatesPath()
    {
        using var adapter = new PathAdapter();
        adapter.Start(0, 0);
        adapter.LineTo(100, 0);
        adapter.LineTo(100, 100);

        Assert.NotNull(adapter.XGraphicsPath);
    }

    [Fact]
    public void ArcTo_AllCorners_DoesNotThrow()
    {
        using var adapter = new PathAdapter();
        adapter.Start(10, 0);

        var ex = Record.Exception(() =>
        {
            adapter.ArcTo(100, 0, 10, RGraphicsPath.Corner.TopRight);
            adapter.LineTo(100, 90);
            adapter.ArcTo(100, 100, 10, RGraphicsPath.Corner.BottomRight);
            adapter.LineTo(10, 100);
            adapter.ArcTo(0, 100, 10, RGraphicsPath.Corner.BottomLeft);
            adapter.LineTo(0, 10);
            adapter.ArcTo(0, 0, 10, RGraphicsPath.Corner.TopLeft);
        });

        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var adapter = new PathAdapter();
        var ex = Record.Exception(() => adapter.Dispose());
        Assert.Null(ex);
    }
}

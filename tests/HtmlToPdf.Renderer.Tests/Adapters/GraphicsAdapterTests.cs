using HtmlToPdf.Renderer.Adapters;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;

namespace HtmlToPdf.Renderer.Tests.Adapters;

public class GraphicsAdapterTests
{
    private GraphicsAdapter CreateAdapter(out XGraphics xGraphics)
    {
        var doc = new PdfDocument();
        var page = doc.AddPage();
        xGraphics = XGraphics.FromPdfPage(page);
        var adapter = RenderAdapter.Instance;
        return new GraphicsAdapter(xGraphics, adapter,
            new RRect(0, 0, page.Width.Point, page.Height.Point));
    }

    [Fact]
    public void MeasureString_ReturnsPositiveSize()
    {
        using var ga = CreateAdapter(out var xg);
        var font = new FontAdapter(new XFont("Arial", 12));
        var size = ga.MeasureString("Hello", font);
        Assert.True(size.Width > 0);
        Assert.True(size.Height > 0);
    }

    [Fact]
    public void MeasureString_CharFit_FitsCharsInWidth()
    {
        using var ga = CreateAdapter(out var xg);
        var font = new FontAdapter(new XFont("Arial", 12));
        ga.MeasureString("Hello World Test", font, 50, out int charFit, out double charFitWidth);
        Assert.True(charFit > 0);
        Assert.True(charFit <= 16);
        Assert.True(charFitWidth > 0);
        Assert.True(charFitWidth <= 50);
    }

    [Fact]
    public void PushClip_PopClip_DoesNotThrow()
    {
        using var ga = CreateAdapter(out _);
        var ex = Record.Exception(() =>
        {
            ga.PushClip(new RRect(10, 10, 100, 100));
            ga.PopClip();
        });
        Assert.Null(ex);
    }

    [Fact]
    public void DrawLine_DoesNotThrow()
    {
        using var ga = CreateAdapter(out _);
        var pen = new PenAdapter(new XPen(XColors.Black));
        var ex = Record.Exception(() => ga.DrawLine(pen, 0, 0, 100, 100));
        Assert.Null(ex);
    }

    [Fact]
    public void DrawRectangle_WithPen_DoesNotThrow()
    {
        using var ga = CreateAdapter(out _);
        var pen = new PenAdapter(new XPen(XColors.Black));
        var ex = Record.Exception(() => ga.DrawRectangle(pen, 10, 10, 50, 50));
        Assert.Null(ex);
    }

    [Fact]
    public void DrawRectangle_WithBrush_DoesNotThrow()
    {
        using var ga = CreateAdapter(out _);
        var brush = new BrushAdapter(new XSolidBrush(XColors.Red));
        var ex = Record.Exception(() => ga.DrawRectangle(brush, 10, 10, 50, 50));
        Assert.Null(ex);
    }

    [Fact]
    public void DrawString_DoesNotThrow()
    {
        using var ga = CreateAdapter(out _);
        var font = new FontAdapter(new XFont("Arial", 12));
        var ex = Record.Exception(() =>
            ga.DrawString("Hello", font, RColor.FromArgb(255, 0, 0, 0),
                new RPoint(10, 10), new RSize(100, 20), false));
        Assert.Null(ex);
    }

    [Fact]
    public void GetGraphicsPath_ReturnsPathAdapter()
    {
        using var ga = CreateAdapter(out _);
        var path = ga.GetGraphicsPath();
        Assert.IsType<PathAdapter>(path);
    }

    [Fact]
    public void SetAntiAlias_ReturnsPreviousMode()
    {
        using var ga = CreateAdapter(out _);
        var prev = ga.SetAntiAliasSmoothingMode();
        Assert.NotNull(prev);
        var ex = Record.Exception(() => ga.ReturnPreviousSmoothingMode(prev));
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_DoesNotDisposeUnderlyingXGraphics()
    {
        var doc = new PdfDocument();
        var page = doc.AddPage();
        var xg = XGraphics.FromPdfPage(page);
        var ga = new GraphicsAdapter(xg, RenderAdapter.Instance,
            new RRect(0, 0, page.Width.Point, page.Height.Point));
        ga.Dispose();
        // XGraphics should still be usable after adapter disposal
        var ex = Record.Exception(() => xg.DrawLine(XPens.Black, 0, 0, 10, 10));
        Assert.Null(ex);
    }

    [Fact]
    public void GetWhitespaceWidth_ReturnsPositiveWidth()
    {
        using var ga = CreateAdapter(out _);
        var font = new FontAdapter(new XFont("Arial", 12));
        var width = font.GetWhitespaceWidth(ga);
        Assert.True(width > 0);
    }
}

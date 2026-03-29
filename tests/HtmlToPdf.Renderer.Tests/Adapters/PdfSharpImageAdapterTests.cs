using HtmlToPdf.Renderer.Adapters;
using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer.Tests.Adapters;

public class PdfSharpImageAdapterTests
{
    [Fact]
    public void WidthAndHeight_DelegateToXImage()
    {
        // Known valid 1x1 red pixel PNG (base64-encoded)
        var pngBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==");
        using var stream = new MemoryStream(pngBytes);
        using var xImage = XImage.FromStream(stream);
        var adapter = new ImageAdapter(xImage);

        Assert.Equal(xImage.PixelWidth, adapter.Width);
        Assert.Equal(xImage.PixelHeight, adapter.Height);
    }
}

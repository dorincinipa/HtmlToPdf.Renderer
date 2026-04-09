using PdfSharp.Drawing;
using HtmlToPdf.Renderer.HtmlEngine.Adapters;

namespace HtmlToPdf.Renderer.Adapters;

public sealed class ImageAdapter : RImage
{
    internal XImage XImage { get; }

    public ImageAdapter(XImage image)
    {
        XImage = image;
    }

    public override double Width => XImage.PixelWidth;
    public override double Height => XImage.PixelHeight;

    public override void Dispose()
    {
        XImage.Dispose();
    }
}

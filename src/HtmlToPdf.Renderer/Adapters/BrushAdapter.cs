using PdfSharp.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace HtmlToPdf.Renderer.Adapters;

public sealed class PdfSharpBrushAdapter : RBrush
{
    internal XBrush XBrush { get; }

    public PdfSharpBrushAdapter(XBrush brush)
    {
        XBrush = brush;
    }

    public override void Dispose()
    {
        // No-op: brushes are cached by RAdapter and shared
    }
}

using PdfSharp.Drawing;
using HtmlToPdf.Renderer.HtmlEngine.Adapters;

namespace HtmlToPdf.Renderer.Adapters;

public sealed class BrushAdapter : RBrush
{
    internal XBrush XBrush { get; }

    public BrushAdapter(XBrush brush)
    {
        XBrush = brush;
    }

    public override void Dispose() { }
}

using PdfSharp.Drawing;
using HtmlToPdf.Renderer.HtmlEngine.Adapters;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;

namespace HtmlToPdf.Renderer.Adapters;

public sealed class PenAdapter : RPen
{
    internal XPen XPen { get; }

    public PenAdapter(XPen pen)
    {
        XPen = pen;
    }

    public override double Width
    {
        get => XPen.Width;
        set => XPen.Width = value;
    }

    public override RDashStyle DashStyle
    {
        set => XPen.DashStyle = (XDashStyle)(int)value;
    }
}

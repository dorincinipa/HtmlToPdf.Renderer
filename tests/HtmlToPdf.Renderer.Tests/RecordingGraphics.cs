using HtmlToPdf.Renderer.Adapters;
using HtmlToPdf.Renderer.HtmlEngine.Adapters;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;
using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer.Tests;

/// <summary>
/// An RGraphics that records the text drawn and at which Y, honouring the current clip's vertical
/// range. Used to assert which content lands on which page when the document is painted in slices.
/// </summary>
internal sealed class RecordingGraphics : RGraphics
{
    private readonly XGraphics _measure;
    public readonly List<(string Text, double Y)> Drawn = new();

    public RecordingGraphics(RAdapter adapter, XGraphics measure, RRect initialClip)
        : base(adapter, initialClip)
    {
        _measure = measure;
    }

    private bool InClip(double y)
    {
        var c = _clipStack.Peek();
        return y >= c.Y - 0.5 && y <= c.Y + c.Height + 0.5;
    }

    public override void PushClip(RRect rect)
    {
        var cur = _clipStack.Peek();
        double top = Math.Max(cur.Y, rect.Y);
        double bottom = Math.Min(cur.Y + cur.Height, rect.Y + rect.Height);
        _clipStack.Push(new RRect(rect.X, top, rect.Width, Math.Max(0, bottom - top)));
    }

    public override void PushClipExclude(RRect rect) => _clipStack.Push(_clipStack.Peek());
    public override void PopClip() => _clipStack.Pop();

    public override void DrawString(string str, RFont font, RColor color, RPoint point, RSize size, bool rtl)
    {
        if (!string.IsNullOrWhiteSpace(str) && InClip(point.Y))
            Drawn.Add((str, point.Y));
    }

    public override RSize MeasureString(string str, RFont font)
    {
        var s = _measure.MeasureString(str, ((FontAdapter)font).XFont);
        return new RSize(s.Width, s.Height);
    }

    public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
    {
        charFit = str?.Length ?? 0;
        charFitWidth = string.IsNullOrEmpty(str) ? 0 : _measure.MeasureString(str, ((FontAdapter)font).XFont).Width;
    }

    public override object SetAntiAliasSmoothingMode() => null!;
    public override void ReturnPreviousSmoothingMode(object prevMode) { }
    public override RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation) => new BrushAdapter(new XSolidBrush(XColors.Transparent));
    public override RGraphicsPath GetGraphicsPath() => new PathAdapter();
    public override void DrawLine(RPen pen, double x1, double y1, double x2, double y2) { }
    public override void DrawRectangle(RPen pen, double x, double y, double width, double height) { }
    public override void DrawRectangle(RBrush brush, double x, double y, double width, double height) { }
    public override void DrawImage(RImage image, RRect destRect, RRect srcRect) { }
    public override void DrawImage(RImage image, RRect destRect) { }
    public override void DrawPath(RPen pen, RGraphicsPath path) { }
    public override void DrawPath(RBrush brush, RGraphicsPath path) { }
    public override void DrawPolygon(RBrush brush, RPoint[] points) { }
    public override void Dispose() { }
}

using PdfSharp.Drawing;
using HtmlToPdf.Renderer.HtmlEngine.Adapters;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;

namespace HtmlToPdf.Renderer.Adapters;

internal sealed class GraphicsAdapter : RGraphics
{
    private static readonly XStringFormat _ltrFormat = new() { Alignment = XStringAlignment.Near, LineAlignment = XLineAlignment.Near };
    private static readonly XStringFormat _rtlFormat = new() { Alignment = XStringAlignment.Far, LineAlignment = XLineAlignment.Near };

    private readonly XGraphics _xGraphics;
    private readonly Stack<XGraphicsState> _stateStack = new();
    private XSmoothingMode _previousSmoothingMode;

    internal XGraphics XGraphics => _xGraphics;

    public GraphicsAdapter(XGraphics xGraphics, RAdapter adapter, RRect initialClip)
        : base(adapter, initialClip)
    {
        _xGraphics = xGraphics;
    }

    public override void PopClip()
    {
        _clipStack.Pop();
        if (_stateStack.Count > 0)
            _xGraphics.Restore(_stateStack.Pop());
    }

    public override void PushClip(RRect rect)
    {
        _clipStack.Push(rect);
        _stateStack.Push(_xGraphics.Save());
        _xGraphics.IntersectClip(new XRect(rect.X, rect.Y, rect.Width, rect.Height));
    }

    public override void PushClipExclude(RRect rect)
    {
        // No-op for PDF — text selection exclusion is not relevant.
        // Push current clip unchanged to maintain stack balance.
        _clipStack.Push(_clipStack.Peek());
        _stateStack.Push(_xGraphics.Save());
    }

    public override Object SetAntiAliasSmoothingMode()
    {
        _previousSmoothingMode = _xGraphics.SmoothingMode;
        _xGraphics.SmoothingMode = XSmoothingMode.AntiAlias;
        return _previousSmoothingMode;
    }

    public override void ReturnPreviousSmoothingMode(Object prevMode)
    {
        if (prevMode is XSmoothingMode mode)
            _xGraphics.SmoothingMode = mode;
    }

    public override RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint offset)
    {
        // PdfSharp 6.x has no XTextureBrush — return transparent brush.
        return new BrushAdapter(new XSolidBrush(XColors.Transparent));
    }

    public override RGraphicsPath GetGraphicsPath()
    {
        return new PathAdapter();
    }

    public override RSize MeasureString(string str, RFont font)
    {
        var xFont = ((FontAdapter)font).XFont;
        var size = _xGraphics.MeasureString(str, xFont);
        return new RSize(size.Width, size.Height);
    }

    public override void MeasureString(string str, RFont font, double maxWidth,
        out int charFit, out double charFitWidth)
    {
        var xFont = ((FontAdapter)font).XFont;

        charFit = 0;
        charFitWidth = 0;

        if (string.IsNullOrEmpty(str))
            return;

        var fullWidth = _xGraphics.MeasureString(str, xFont).Width;
        if (fullWidth <= maxWidth)
        {
            charFit = str.Length;
            charFitWidth = fullWidth;
            return;
        }

        int lo = 0, hi = str.Length;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            var w = _xGraphics.MeasureString(str.Substring(0, mid), xFont).Width;
            if (w <= maxWidth)
            {
                charFit = mid;
                charFitWidth = w;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }
    }

    public override void DrawString(string str, RFont font, RColor color,
        RPoint point, RSize size, bool rtl)
    {
        var xFont = ((FontAdapter)font).XFont;
        var xBrush = new XSolidBrush(RenderAdapter.ToXColor(color));
        _xGraphics.DrawString(str, xFont, xBrush,
            new XRect(point.X, point.Y, size.Width, size.Height), rtl ? _rtlFormat : _ltrFormat);
    }

    public override void DrawLine(RPen pen, double x1, double y1, double x2, double y2)
    {
        _xGraphics.DrawLine(((PenAdapter)pen).XPen, x1, y1, x2, y2);
    }

    public override void DrawRectangle(RPen pen, double x, double y, double width, double height)
    {
        _xGraphics.DrawRectangle(((PenAdapter)pen).XPen, x, y, width, height);
    }

    public override void DrawRectangle(RBrush brush, double x, double y,
        double width, double height)
    {
        _xGraphics.DrawRectangle(((BrushAdapter)brush).XBrush, x, y, width, height);
    }

    public override void DrawImage(RImage image, RRect destRect)
    {
        var xImage = ((ImageAdapter)image).XImage;
        _xGraphics.DrawImage(xImage,
            new XRect(destRect.X, destRect.Y, destRect.Width, destRect.Height));
    }

    public override void DrawImage(RImage image, RRect destRect, RRect srcRect)
    {
        var xImage = ((ImageAdapter)image).XImage;
        _xGraphics.Save();
        _xGraphics.IntersectClip(
            new XRect(destRect.X, destRect.Y, destRect.Width, destRect.Height));

        double scaleX = destRect.Width / srcRect.Width;
        double scaleY = destRect.Height / srcRect.Height;
        double drawX = destRect.X - srcRect.X * scaleX;
        double drawY = destRect.Y - srcRect.Y * scaleY;
        double drawW = xImage.PixelWidth * scaleX;
        double drawH = xImage.PixelHeight * scaleY;

        _xGraphics.DrawImage(xImage, new XRect(drawX, drawY, drawW, drawH));
        _xGraphics.Restore();
    }

    public override void DrawPath(RPen pen, RGraphicsPath path)
    {
        _xGraphics.DrawPath(((PenAdapter)pen).XPen,
            ((PathAdapter)path).XGraphicsPath);
    }

    public override void DrawPath(RBrush brush, RGraphicsPath path)
    {
        _xGraphics.DrawPath(((BrushAdapter)brush).XBrush,
            ((PathAdapter)path).XGraphicsPath);
    }

    public override void DrawPolygon(RBrush brush, RPoint[] points)
    {
        var xPoints = new XPoint[points.Length];
        for (int i = 0; i < points.Length; i++)
            xPoints[i] = new XPoint(points[i].X, points[i].Y);
        _xGraphics.DrawPolygon(((BrushAdapter)brush).XBrush,
            xPoints, XFillMode.Winding);
    }

    public override void Dispose()
    {
        // Do NOT dispose the underlying XGraphics — the caller (PdfGenerator) owns it.
    }
}

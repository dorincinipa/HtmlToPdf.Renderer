using PdfSharp.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace HtmlToPdf.Renderer.Adapters;

public sealed class PathAdapter : RGraphicsPath
{
    internal XGraphicsPath XGraphicsPath { get; } = new();

    private double _lastX;
    private double _lastY;

    public override void Start(double x, double y)
    {
        _lastX = x;
        _lastY = y;
    }

    public override void LineTo(double x, double y)
    {
        XGraphicsPath.AddLine(_lastX, _lastY, x, y);
        _lastX = x;
        _lastY = y;
    }

    public override void ArcTo(double x, double y, double size, Corner corner)
    {
        double arcX, arcY;
        double startAngle;

        switch (corner)
        {
            case Corner.TopLeft:
                arcX = x;
                arcY = y;
                startAngle = 180;
                break;
            case Corner.TopRight:
                arcX = x - size;
                arcY = y;
                startAngle = 270;
                break;
            case Corner.BottomRight:
                arcX = x - size;
                arcY = y - size;
                startAngle = 0;
                break;
            case Corner.BottomLeft:
                arcX = x;
                arcY = y - size;
                startAngle = 90;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(corner));
        }

        XGraphicsPath.AddArc(arcX, arcY, size, size, startAngle, 90);
        _lastX = x;
        _lastY = y;
    }

    public override void Dispose()
    {
        // XGraphicsPath has no Dispose — no-op
    }
}

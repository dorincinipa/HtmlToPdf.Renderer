using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer;

public enum StampPosition { Center, TopLeft, TopRight, BottomLeft, BottomRight }

public sealed class StampOptions
{
    public string? Text { get; }
    public byte[]? ImageData { get; }
    public float Opacity { get; }
    public double Angle { get; }
    public double FontSize { get; init; } = 60;
    public XColor Color { get; init; } = XColors.Gray;
    public StampPosition Position { get; init; } = StampPosition.Center;
    public double ImageWidth { get; }
    public double ImageHeight { get; }

    /// <summary>Text watermark - e.g. new StampOptions("CONFIDENTIAL", opacity: 0.2f, angle: 45)</summary>
    public StampOptions(string text, float opacity = 0.3f, double angle = 45)
    {
        Text = text; Opacity = opacity; Angle = angle;
    }

    /// <summary>Image watermark / logo. Opacity is applied via the image's own alpha channel (use transparent PNG).</summary>
    public StampOptions(byte[] imageData, float opacity = 1f, double angle = 0,
        StampPosition position = StampPosition.Center, double width = 0, double height = 0)
    {
        ImageData = imageData; Opacity = opacity; Angle = angle;
        Position = position; ImageWidth = width; ImageHeight = height;
    }
}

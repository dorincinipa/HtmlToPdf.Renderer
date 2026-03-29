using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlToPdf.Renderer.Adapters;

internal static class ColorMap
{
    private static readonly Dictionary<string, RColor> _colors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["black"] = RColor.FromArgb(255, 0, 0, 0),
            ["white"] = RColor.FromArgb(255, 255, 255, 255),
            ["red"] = RColor.FromArgb(255, 255, 0, 0),
            ["green"] = RColor.FromArgb(255, 0, 128, 0),
            ["blue"] = RColor.FromArgb(255, 0, 0, 255),
            ["yellow"] = RColor.FromArgb(255, 255, 255, 0),
            ["orange"] = RColor.FromArgb(255, 255, 165, 0),
            ["purple"] = RColor.FromArgb(255, 128, 0, 128),
            ["gray"] = RColor.FromArgb(255, 128, 128, 128),
            ["grey"] = RColor.FromArgb(255, 128, 128, 128),
            ["silver"] = RColor.FromArgb(255, 192, 192, 192),
            ["maroon"] = RColor.FromArgb(255, 128, 0, 0),
            ["navy"] = RColor.FromArgb(255, 0, 0, 128),
            ["teal"] = RColor.FromArgb(255, 0, 128, 128),
            ["aqua"] = RColor.FromArgb(255, 0, 255, 255),
            ["cyan"] = RColor.FromArgb(255, 0, 255, 255),
            ["fuchsia"] = RColor.FromArgb(255, 255, 0, 255),
            ["magenta"] = RColor.FromArgb(255, 255, 0, 255),
            ["lime"] = RColor.FromArgb(255, 0, 255, 0),
            ["olive"] = RColor.FromArgb(255, 128, 128, 0),
            ["lightgray"] = RColor.FromArgb(255, 211, 211, 211),
            ["lightgrey"] = RColor.FromArgb(255, 211, 211, 211),
            ["darkgray"] = RColor.FromArgb(255, 169, 169, 169),
            ["darkgrey"] = RColor.FromArgb(255, 169, 169, 169),
            ["whitesmoke"] = RColor.FromArgb(255, 245, 245, 245),
            ["gainsboro"] = RColor.FromArgb(255, 220, 220, 220),
            ["dimgray"] = RColor.FromArgb(255, 105, 105, 105),
            ["dimgrey"] = RColor.FromArgb(255, 105, 105, 105),
            ["brown"] = RColor.FromArgb(255, 165, 42, 42),
            ["pink"] = RColor.FromArgb(255, 255, 192, 203),
            ["coral"] = RColor.FromArgb(255, 255, 127, 80),
            ["tomato"] = RColor.FromArgb(255, 255, 99, 71),
            ["gold"] = RColor.FromArgb(255, 255, 215, 0),
            ["khaki"] = RColor.FromArgb(255, 240, 230, 140),
            ["violet"] = RColor.FromArgb(255, 238, 130, 238),
            ["indigo"] = RColor.FromArgb(255, 75, 0, 130),
            ["darkblue"] = RColor.FromArgb(255, 0, 0, 139),
            ["darkred"] = RColor.FromArgb(255, 139, 0, 0),
            ["darkgreen"] = RColor.FromArgb(255, 0, 100, 0),
            ["lightblue"] = RColor.FromArgb(255, 173, 216, 230),
            ["lightgreen"] = RColor.FromArgb(255, 144, 238, 144),
            ["lightyellow"] = RColor.FromArgb(255, 255, 255, 224),
            ["ivory"] = RColor.FromArgb(255, 255, 255, 240),
            ["beige"] = RColor.FromArgb(255, 245, 245, 220),
        };

    public static bool TryGetColor(string colorName, out RColor color) =>
        _colors.TryGetValue(colorName, out color);
}

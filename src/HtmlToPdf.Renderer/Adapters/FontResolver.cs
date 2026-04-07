using System.Collections.Concurrent;
using PdfSharp.Fonts;

namespace HtmlToPdf.Renderer.Adapters;

internal sealed class FontResolver : IFontResolver
{
    internal static FontResolver Instance { get; } = new();

    private readonly ConcurrentDictionary<string, byte[]> _customFonts = new(StringComparer.OrdinalIgnoreCase);
    private volatile bool _registered;

    private FontResolver() { }

    private void EnsureRegistered()
    {
        if (_registered)
            return;

        _registered = true;
        try
        {
            GlobalFontSettings.FontResolver = this;
        }
        catch (InvalidOperationException)
        {
            // PdfSharp may throw if a font resolver was already set or fonts were already used.
        }
    }

    internal void RegisterFont(string familyName, byte[] fontData)
    {
        ArgumentNullException.ThrowIfNull(familyName);
        ArgumentNullException.ThrowIfNull(fontData);
        _customFonts[familyName] = fontData;
        EnsureRegistered();
    }

    internal void LoadFontsFromFolder(string folderPath)
    {
        ArgumentNullException.ThrowIfNull(folderPath);
        if (!Directory.Exists(folderPath))
            return;

        foreach (var file in Directory.EnumerateFiles(folderPath, "*.*"))
        {
            var ext = Path.GetExtension(file);
            if (!ext.Equals(".ttf", StringComparison.OrdinalIgnoreCase) &&
                !ext.Equals(".otf", StringComparison.OrdinalIgnoreCase))
                continue;

            var familyName = Path.GetFileNameWithoutExtension(file);
            _customFonts[familyName] = File.ReadAllBytes(file);
        }

        if (_customFonts.Count > 0)
            EnsureRegistered();
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
    {
        if (_customFonts.ContainsKey(familyName))
            return new FontResolverInfo(familyName);

        return null;
    }

    public byte[] GetFont(string faceName)
    {
        if (_customFonts.TryGetValue(faceName, out var data))
            return data;

        return [];
    }
}

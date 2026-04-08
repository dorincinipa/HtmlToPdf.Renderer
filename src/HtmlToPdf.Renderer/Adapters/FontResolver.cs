using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using PdfSharp.Fonts;

namespace HtmlToPdf.Renderer.Adapters;

internal sealed class FontResolver : IFontResolver
{
    internal static FontResolver Instance { get; } = new();

    private readonly ConcurrentDictionary<string, byte[]> _customFonts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lazy<ConcurrentDictionary<string, string>> _systemFonts = new(ScanSystemFonts);
    private int _registered;

    private FontResolver() { }

    internal void EnsureRegistered()
    {
        if (Interlocked.CompareExchange(ref _registered, 1, 0) != 0)
            return;

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

        if (_systemFonts.Value.ContainsKey(familyName))
            return new FontResolverInfo(familyName);

        return null;
    }

    public byte[] GetFont(string faceName)
    {
        if (_customFonts.TryGetValue(faceName, out var data))
            return data;

        if (_systemFonts.Value.TryGetValue(faceName, out var path))
            return File.ReadAllBytes(path);

        return [];
    }

    private static ConcurrentDictionary<string, string> ScanSystemFonts()
    {
        var fonts = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return fonts; // Windows uses built-in PdfSharp resolution

        string[] dirs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? ["/Library/Fonts", "/System/Library/Fonts", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts")]
            : ["/usr/share/fonts", "/usr/local/share/fonts", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts")];

        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir))
                continue;

            foreach (var file in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file);
                if (!ext.Equals(".ttf", StringComparison.OrdinalIgnoreCase) &&
                    !ext.Equals(".otf", StringComparison.OrdinalIgnoreCase))
                    continue;

                var name = Path.GetFileNameWithoutExtension(file);
                fonts.TryAdd(name, file);
            }
        }

        return fonts;
    }
}

using PdfSharp.Fonts;

namespace HtmlToPdf.Renderer.Adapters;

internal sealed class FontResolver : IFontResolver
{
    internal static FontResolver Instance { get; } = new();

    private FontResolver() { }

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic) {
        throw new NotImplementedException();
    }

    public byte[]? GetFont(string faceName) {
        throw new NotImplementedException();
    }
}

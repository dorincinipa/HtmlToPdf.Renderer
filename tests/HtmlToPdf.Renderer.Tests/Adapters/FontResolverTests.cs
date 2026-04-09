using HtmlToPdf.Renderer.Adapters;

namespace HtmlToPdf.Renderer.Tests.Adapters;

public class FontResolverTests
{
    [Fact]
    public void ResolveTypeface_UnknownFamily_ReturnsNull()
    {
        var resolver = FontResolver.Instance;
        var result = resolver.ResolveTypeface("NonExistentFont_abc123", false, false);
        Assert.Null(result);
    }

    [Fact]
    public void GetFont_UnknownFace_ReturnsEmptyArray()
    {
        var resolver = FontResolver.Instance;
        var result = resolver.GetFont("NonExistentFont_abc123");
        Assert.Empty(result);
    }
}

using HtmlToPdf.Renderer.Adapters;

namespace HtmlToPdf.Renderer.Tests.Adapters;

public class FontResolverTests
{
    [Fact]
    public void EnsureRegistered_CalledMultipleTimes_DoesNotThrow()
    {
        var resolver = FontResolver.Instance;

        var exception = Record.Exception(() =>
        {
            resolver.EnsureRegistered();
            resolver.EnsureRegistered();
            resolver.EnsureRegistered();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void RegisterFont_WithLiberationName_ResolvesViaCssAlias()
    {
        var resolver = FontResolver.Instance;
        var fakeFontData = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        // Register a font under the Liberation filename key
        resolver.RegisterFont("LiberationSans-Regular", fakeFontData);

        // The CSS alias "Arial" should resolve because the alias table
        // maps "Arial" → "LiberationSans-Regular"
        var info = resolver.ResolveTypeface("Arial", false, false);

        Assert.NotNull(info);
    }

    [Fact]
    public void GetFont_WithCssAlias_ReturnsFontData()
    {
        var resolver = FontResolver.Instance;
        var fakeFontData = new byte[] { 0xDE, 0xAD };

        resolver.RegisterFont("LiberationSans-Regular", fakeFontData);

        var data = resolver.GetFont("Arial");

        Assert.Equal(fakeFontData, data);
    }

    [Fact]
    public void GetFont_UnknownFont_ReturnsEmptyArray()
    {
        var resolver = FontResolver.Instance;

        var data = resolver.GetFont("NonExistentFont_" + Guid.NewGuid());

        Assert.Empty(data);
    }
}

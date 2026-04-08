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
}

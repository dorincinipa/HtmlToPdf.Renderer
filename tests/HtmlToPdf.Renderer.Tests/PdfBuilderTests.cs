namespace HtmlToPdf.Renderer.Tests;

public class PdfBuilderTests
{
    [Fact]
    public void Create_ReturnsBuilder()
    {
        var builder = PdfGenerator.Create();
        Assert.NotNull(builder);
    }

    [Fact]
    public void WithPageSize_GeneratePdf_UsesConfiguredSize()
    {
        var doc = PdfGenerator.Create()
            .WithPageSize(PageSize.Letter)
            .GeneratePdf("<p>Test</p>");

        Assert.Equal(612, doc.Pages[0].Width.Point, 1);
        Assert.Equal(792, doc.Pages[0].Height.Point, 1);
    }

    [Fact]
    public void WithMargin_GeneratePdf_ProducesValidDocument()
    {
        var doc = PdfGenerator.Create()
            .WithPageSize(PageSize.A4)
            .WithMargin(20)
            .GeneratePdf("<p>Margin test</p>");

        Assert.NotNull(doc);
        Assert.True(doc.PageCount >= 1);
    }

    [Fact]
    public void WithOrientation_Landscape_SwapsDimensions()
    {
        var doc = PdfGenerator.Create()
            .WithPageSize(PageSize.A4)
            .WithOrientation(PageOrientation.Landscape)
            .GeneratePdf("<p>Landscape</p>");

        Assert.True(doc.Pages[0].Width.Point > doc.Pages[0].Height.Point);
    }

    [Fact]
    public void FluentChain_AllOptions_ProducesValidPdf()
    {
        var doc = PdfGenerator.Create()
            .WithPageSize(PageSize.A4)
            .WithOrientation(PageOrientation.Portrait)
            .WithMargin(10, 20)
            .GeneratePdf("<p>Full chain</p>");

        Assert.NotNull(doc);
        Assert.True(doc.PageCount >= 1);
    }

    [Fact]
    public async Task GeneratePdfAsync_ReturnsValidBytes()
    {
        var bytes = await PdfGenerator.Create()
            .WithPageSize(PageSize.A4)
            .WithMargin(20)
            .GeneratePdfAsync("<p>Async test</p>");

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
        Assert.Equal("%PDF-"u8.ToArray(), bytes[..5]);
    }
}

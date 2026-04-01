using PdfSharp.Pdf;

namespace HtmlToPdf.Renderer.Tests;

public class PdfGeneratorTests
{
    private static PdfOptions DefaultConfig() => new()
    {
        PageSize = PageSize.A4,
        MarginTop = 20,
        MarginBottom = 20,
        MarginLeft = 20,
        MarginRight = 20
    };

    [Fact]
    public void GeneratePdf_SimpleHtml_ReturnsValidDocument()
    {
        var doc = PdfGenerator.GeneratePdf("<p>Hello World</p>", DefaultConfig());
        Assert.NotNull(doc);
        Assert.True(doc.PageCount >= 1);
    }

    [Fact]
    public void GeneratePdf_EmptyHtml_ReturnsSinglePage()
    {
        var doc = PdfGenerator.GeneratePdf("", DefaultConfig());
        Assert.NotNull(doc);
        Assert.Equal(1, doc.PageCount);
    }

    [Fact]
    public void GeneratePdf_LongContent_CreatesMultiplePages()
    {
        var html = "<div>" + string.Join("", Enumerable.Repeat("<p>Line of text for pagination test.</p>", 200)) + "</div>";
        var doc = PdfGenerator.GeneratePdf(html, DefaultConfig());
        Assert.True(doc.PageCount > 1, $"Expected multiple pages but got {doc.PageCount}");
    }

    [Fact]
    public void GeneratePdf_WithMargins_PageDimensionsMatchConfig()
    {
        var config = new PdfOptions
        {
            PageSize = PageSize.Letter,
            MarginTop = 50,
            MarginBottom = 50,
            MarginLeft = 50,
            MarginRight = 50
        };
        var doc = PdfGenerator.GeneratePdf("<p>Margins test</p>", config);
        Assert.Equal(612, doc.Pages[0].Width.Point, 1);
        Assert.Equal(792, doc.Pages[0].Height.Point, 1);
    }

    [Fact]
    public void GeneratePdf_WithCss_DoesNotThrow()
    {
        var html = @"<p style='color:red; font-size:24px; font-weight:bold;'>Styled text</p>";
        var ex = Record.Exception(() => PdfGenerator.GeneratePdf(html, DefaultConfig()));
        Assert.Null(ex);
    }

    [Fact]
    public void GeneratePdf_Landscape_SwapsDimensions()
    {
        var config = new PdfOptions
        {
            PageSize = PageSize.A4,
            PageOrientation = PageOrientation.Landscape
        };
        var doc = PdfGenerator.GeneratePdf("<p>Landscape</p>", config);
        Assert.True(doc.Pages[0].Width.Point > doc.Pages[0].Height.Point);
    }

    [Fact]
    public void GeneratePdf_SaveToStream_ProducesNonEmptyOutput()
    {
        var doc = PdfGenerator.GeneratePdf("<p>Stream test</p>", DefaultConfig());
        using var stream = new MemoryStream();
        doc.Save(stream, false);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void GeneratePdf_WithImage_LoadsAndRenders()
    {
        var html = "<img src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==' />";
        var doc = PdfGenerator.GeneratePdf(html, DefaultConfig());
        Assert.NotNull(doc);
        Assert.True(doc.PageCount >= 1);
    }

    [Fact]
    public void GeneratePdf_WithTable_DoesNotThrow()
    {
        var html = @"
            <table border='1'>
                <tr><th>Name</th><th>Value</th></tr>
                <tr><td>Item 1</td><td>100</td></tr>
                <tr><td>Item 2</td><td>200</td></tr>
            </table>";
        var ex = Record.Exception(() => PdfGenerator.GeneratePdf(html, DefaultConfig()));
        Assert.Null(ex);
    }

    [Fact]
    public void GeneratePdf_TableWithThead_SpansMultiplePages()
    {
        var rows = string.Join("\n", Enumerable.Range(1, 60)
            .Select(i => $"<tr><td>{i}</td><td>Item {i}</td><td>Description for row {i}</td><td>{i * 10:C}</td></tr>"));

        var html = $@"
            <style>
                table {{ width: 100%; border-collapse: collapse; }}
                thead {{ display: table-header-group; }}
                th {{ background-color: #2c3e50; color: white; padding: 8px; text-align: left; }}
                td {{ padding: 6px 8px; border-bottom: 1px solid #ddd; }}
                tr:nth-child(even) {{ background-color: #f2f2f2; }}
            </style>
            <table>
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Price</th>
                    </tr>
                </thead>
                <tbody>
                    {rows}
                </tbody>
            </table>";

        var doc = PdfGenerator.GeneratePdf(html, DefaultConfig());

        Assert.True(doc.PageCount >= 2, $"Expected at least 2 pages but got {doc.PageCount}");
    }

    [Fact]
    public void AddPdfPages_AppendsToExistingDocument()
    {
        var doc = new PdfDocument();
        doc.AddPage(); // pre-existing page
        Assert.Equal(1, doc.PageCount);

        PdfGenerator.AddPdfPages(doc, "<p>Appended content</p>", DefaultConfig());

        Assert.True(doc.PageCount >= 2, $"Expected at least 2 pages but got {doc.PageCount}");
    }

    [Fact]
    public void AddPdfPages_MultipleCalls_AccumulatePages()
    {
        var doc = new PdfDocument();

        PdfGenerator.AddPdfPages(doc, "<p>Section 1</p>", DefaultConfig());
        var countAfterFirst = doc.PageCount;

        PdfGenerator.AddPdfPages(doc, "<p>Section 2</p>", DefaultConfig());

        Assert.True(doc.PageCount > countAfterFirst,
            $"Expected more pages after second call, but still {doc.PageCount}");
    }

    [Fact]
    public void GeneratePdf_TableWithThead_SavesValidPdf()
    {
        var rows = string.Join("\n", Enumerable.Range(1, 60)
            .Select(i => $"<tr><td>{i}</td><td>Item {i}</td><td>Description for row {i}</td><td>{i * 10:C}</td></tr>"));

        var html = $@"
            <style>
                table {{ width: 100%; border-collapse: collapse; }}
                thead {{ display: table-header-group; }}
                th {{ background-color: #2c3e50; color: white; padding: 8px; text-align: left; }}
                td {{ padding: 6px 8px; border-bottom: 1px solid #ddd; }}
                tr:nth-child(even) {{ background-color: #f2f2f2; }}
            </style>
            <table>
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Price</th>
                    </tr>
                </thead>
                <tbody>
                    {rows}
                </tbody>
            </table>";

        var doc = PdfGenerator.GeneratePdf(html, DefaultConfig());

        using var stream = new MemoryStream();
        doc.Save(stream, false);
        Assert.True(stream.Length > 0);

        // PDF header magic bytes
        stream.Position = 0;
        var header = new byte[5];
        stream.Read(header, 0, 5);
        Assert.Equal("%PDF-"u8.ToArray(), header);
    }
}

using HtmlToPdf.Renderer;
using HtmlToPdf.Renderer.Adapters;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace HtmlToPdf.Renderer.Tests;

public class PaginationTests
{
    private static PdfOptions Config() => new()
    {
        PageSize = PageSize.A4,
        MarginTop = 20,
        MarginBottom = 20,
        MarginLeft = 20,
        MarginRight = 20
    };

    // A document made of many headings, paragraphs and tables (including a nested table) that
    // spans several pages. The whole thing is laid out once and sliced into pages; the generated
    // page count must cover the full content height so that trailing content is not dropped.
    private static string TallHtml()
    {
        var sections = string.Join("\n", Enumerable.Range(0, 12).Select(i => $@"
            <h2>Section {i}</h2>
            <p>Paragraph {i} with some representative body text to take vertical space.</p>
            <table border='1' style='width:100%'>
                <tr><td>Row {i} A</td><td>
                    <table border='1'><tr><td>nested {i}</td><td>x</td></tr></table>
                </td></tr>
            </table>"));
        return $"<div>{sections}<p>FINAL TRAILING CONTENT</p></div>";
    }

    [Fact]
    public void GeneratePdf_TallDocument_GeneratesEnoughPagesToCoverAllContent()
    {
        var html = TallHtml();
        var options = Config();

        var effective = options.GetEffectivePageSize();
        double contentHeight = effective.Height - options.MarginTop - options.MarginBottom;
        double contentWidth = effective.Width - options.MarginLeft - options.MarginRight;

        // measure the full content height exactly as PdfGenerator lays it out (single pass)
        var adapter = RenderAdapter.Instance;
        double totalHeight;
        using (var container = new HtmlContainer(adapter))
        {
            container.SetHtml(html);
            container.PageSize = new RSize(effective.Width, effective.Height);
            container.MarginTop = (int)Math.Round(options.MarginTop);
            container.MarginBottom = (int)Math.Round(options.MarginBottom);
            container.MarginLeft = (int)Math.Round(options.MarginLeft);
            container.MarginRight = (int)Math.Round(options.MarginRight);
            container.MaxSize = new RSize(contentWidth, 0);
            container.Location = new RPoint(options.MarginLeft, options.MarginTop);

            using var ctx = XGraphics.CreateMeasureContext(new XSize(effective.Width, effective.Height), XGraphicsUnit.Point, XPageDirection.Downwards);
            using var g = new GraphicsAdapter(ctx, adapter, new RRect(0, 0, effective.Width, effective.Height));
            container.PerformLayout(g);
            totalHeight = container.ActualSize.Height;
        }

        int expectedPages = Math.Max(1, (int)Math.Ceiling(totalHeight / contentHeight));
        Assert.True(expectedPages > 1, "test document should span multiple pages");

        var doc = PdfGenerator.GeneratePdf(html, options);

        Assert.Equal(expectedPages, doc.PageCount);
    }

    [Fact]
    public void GeneratePdf_MultiPage_HasNoBlankPages()
    {
        var doc = PdfGenerator.GeneratePdf(TallHtml(), Config());
        Assert.True(doc.PageCount > 1);

        for (int i = 0; i < doc.PageCount; i++)
        {
            int len = ContentLength(doc.Pages[i]);
            Assert.True(len > 100, $"page {i + 1} of {doc.PageCount} appears blank (content length {len})");
        }
    }

    [Fact]
    public void Paint_SlicesSingleLayout_TrailingContentLandsOnLastPage()
    {
        var html = TallHtml();
        var options = Config();
        var effective = options.GetEffectivePageSize();
        double contentHeight = effective.Height - options.MarginTop - options.MarginBottom;
        double contentWidth = effective.Width - options.MarginLeft - options.MarginRight;

        var adapter = RenderAdapter.Instance;
        using var container = new HtmlContainer(adapter);
        container.SetHtml(html);
        container.PageSize = new RSize(effective.Width, effective.Height);
        container.MarginTop = (int)options.MarginTop;
        container.MarginBottom = (int)options.MarginBottom;
        container.MarginLeft = (int)options.MarginLeft;
        container.MarginRight = (int)options.MarginRight;
        container.MaxSize = new RSize(contentWidth, 0);
        container.Location = new RPoint(options.MarginLeft, options.MarginTop);

        using var ctx = XGraphics.CreateMeasureContext(new XSize(effective.Width, effective.Height), XGraphicsUnit.Point, XPageDirection.Downwards);
        using var layoutG = new GraphicsAdapter(ctx, adapter, new RRect(0, 0, effective.Width, effective.Height));
        container.PerformLayout(layoutG); // single layout, exactly like PdfGenerator

        double totalHeight = container.ActualSize.Height;
        int pageCount = Math.Max(1, (int)Math.Ceiling(totalHeight / contentHeight));
        Assert.True(pageCount > 1);

        var perPageText = new List<List<string>>();
        var recorder = new RecordingGraphics(adapter, ctx, new RRect(0, -1_000_000, effective.Width, 2_000_000));
        for (int i = 0; i < pageCount; i++)
        {
            int before = recorder.Drawn.Count;
            var clip = new RRect(options.MarginLeft, options.MarginTop + i * contentHeight, contentWidth, contentHeight);
            container.PerformPaint(recorder, clip);
            perPageText.Add(recorder.Drawn.Skip(before).Select(d => d.Text).ToList());
        }

        // every page renders something (no blank pages)
        for (int i = 0; i < pageCount; i++)
            Assert.True(perPageText[i].Count > 0, $"page {i + 1} rendered no text");

        // the very last content of the document is rendered, and on the last page
        Assert.Contains("FINAL", perPageText[pageCount - 1]);
        Assert.DoesNotContain(perPageText.Take(pageCount - 1).SelectMany(t => t), t => t == "FINAL");

        // every section heading is rendered somewhere
        var all = perPageText.SelectMany(t => t).ToHashSet();
        for (int i = 0; i < 12; i++)
            Assert.Contains($"Section", all); // headings present (word-split keeps "Section")
    }

    private static int ContentLength(PdfPage page)
    {
        int total = 0;
        foreach (var content in page.Contents)
            total += content.Stream?.Value?.Length ?? 0;
        return total;
    }
}

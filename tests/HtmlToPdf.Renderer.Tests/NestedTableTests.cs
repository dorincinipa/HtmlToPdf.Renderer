using System.Reflection;
using HtmlToPdf.Renderer;
using HtmlToPdf.Renderer.Adapters;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;
using HtmlToPdf.Renderer.HtmlEngine.Core.Dom;
using HtmlToPdf.Renderer.HtmlEngine.Core.Utils;
using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer.Tests;

public class NestedTableTests
{
    [Fact]
    public void GeneratePdf_NestedTable_DoesNotThrow()
    {
        var html = @"
            <table border='1'>
                <tr>
                    <td>Outer</td>
                    <td>
                        <table border='1'>
                            <tr><td>Inner A</td><td>Inner B</td></tr>
                        </table>
                    </td>
                </tr>
            </table>";

        var ex = Record.Exception(() => PdfGenerator.GeneratePdf(html, new PdfOptions { PageSize = PageSize.A4 }));
        Assert.Null(ex);
    }

    [Fact]
    public void NestedTable_DoesNotOverflowContainingCell()
    {
        // Three levels of auto-width tables: each inner table needs more width than a naive
        // (longest-word) estimate, so without min-width propagation the inner table overflows
        // its parent cell to the right.
        var html = @"
            <table border='1'>
                <tr>
                    <td>Outer cell 1</td>
                    <td>
                        <table border='1'>
                            <tr><td>Inner A</td><td>Inner B</td></tr>
                            <tr><td>Inner C</td>
                                <td>
                                    <table border='1'>
                                        <tr><td>Deep 1</td><td>Deep 2</td></tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>";

        var root = LayoutTree(html);

        foreach (var table in Descendants(root))
        {
            if (table.Display != CssConstants.Table && table.Display != CssConstants.InlineTable)
                continue;

            var cell = ContainingCell(table);
            if (cell == null)
                continue; // top-level table

            Assert.True(table.ActualRight <= cell.ActualRight + 0.5,
                $"Nested table (right={table.ActualRight:F1}) overflows its containing cell (right={cell.ActualRight:F1}).");
        }
    }

    private static CssBox? ContainingCell(CssBox box)
    {
        for (var parent = box.ParentBox; parent != null; parent = parent.ParentBox)
            if (parent.Display == CssConstants.TableCell)
                return parent;
        return null;
    }

    private static IEnumerable<CssBox> Descendants(CssBox box)
    {
        foreach (var child in box.Boxes)
        {
            yield return child;
            foreach (var d in Descendants(child))
                yield return d;
        }
    }

    private static CssBox LayoutTree(string html)
    {
        var adapter = RenderAdapter.Instance;
        var container = new HtmlContainer(adapter);
        container.SetHtml(html);
        container.PageSize = new RSize(595, 842);
        container.MaxSize = new RSize(515, 0);
        container.Location = new RPoint(40, 40);

        using var ctx = XGraphics.CreateMeasureContext(new XSize(595, 842), XGraphicsUnit.Point, XPageDirection.Downwards);
        using var g = new GraphicsAdapter(ctx, adapter, new RRect(0, 0, 595, 842));
        container.PerformLayout(g);

        var ci = typeof(HtmlContainer)
            .GetField("_containerInt", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(container);
        return (CssBox)ci!.GetType()
            .GetProperty("Root", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(ci)!;
    }
}

using System.Reflection;
using HtmlToPdf.Renderer;
using HtmlToPdf.Renderer.Adapters;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;
using HtmlToPdf.Renderer.HtmlEngine.Core.Dom;
using HtmlToPdf.Renderer.HtmlEngine.Core.Utils;
using PdfSharp.Drawing;

namespace HtmlToPdf.Renderer.Tests;

public class TableRowBackgroundTests
{
    // A background-color on a <tr> (e.g. a styled thead row) must actually paint. The cells are
    // positioned directly by the table layout, so the row needs real geometry or its background
    // has nothing to draw into and the row appears blank (white text on no background).
    [Fact]
    public void TheadRow_WithBackground_GetsPaintableGeometry()
    {
        const string html = @"
            <table style='width:100%; border-collapse:collapse;'>
                <thead>
                    <tr style='background-color:#3498db; color:white;'>
                        <th style='padding:8px;'>Item</th>
                        <th style='padding:8px;'>Price</th>
                    </tr>
                </thead>
                <tbody>
                    <tr><td style='padding:8px;'>Widget</td><td style='padding:8px;'>$9.99</td></tr>
                </tbody>
            </table>";

        var root = LayoutTree(html);

        var headerRow = FindHeaderRow(root);
        Assert.NotNull(headerRow);
        Assert.True(headerRow!.Size.Width > 0, "header row has no width to paint a background into");
        Assert.True(headerRow.Size.Height > 0, "header row has no height to paint a background into");
        Assert.True(RenderUtils.IsColorVisible(headerRow.ActualBackgroundColor), "header row background color is not visible");
    }

    private static CssBox? FindHeaderRow(CssBox box)
    {
        if (box.Display == CssConstants.TableRow && RenderUtils.IsColorVisible(box.ActualBackgroundColor))
            return box;
        foreach (var c in box.Boxes)
        {
            var found = FindHeaderRow(c);
            if (found != null)
                return found;
        }
        return null;
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

        var ci = typeof(HtmlContainer).GetField("_containerInt", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(container);
        return (CssBox)ci!.GetType().GetProperty("Root", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(ci)!;
    }
}

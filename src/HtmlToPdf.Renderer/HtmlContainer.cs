using HtmlToPdf.Renderer.Adapters;
using HtmlToPdf.Renderer.HtmlEngine.Adapters;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;
using HtmlToPdf.Renderer.HtmlEngine.Core;
using HtmlToPdf.Renderer.HtmlEngine.Core.Entities;

namespace HtmlToPdf.Renderer;

public sealed class HtmlContainer : IDisposable
{
    private readonly HtmlContainerInt _containerInt;

    public HtmlContainer(RenderAdapter adapter)
    {
        _containerInt = new HtmlContainerInt(adapter);
    }

    public void SetHtml(string html, CssData? cssData = null)
    {
        _containerInt.SetHtml(html, cssData!);
    }

    public RSize MaxSize
    {
        get => _containerInt.MaxSize;
        set => _containerInt.MaxSize = value;
    }

    public RSize ActualSize => _containerInt.ActualSize;

    public RPoint Location
    {
        get => _containerInt.Location;
        set => _containerInt.Location = value;
    }

    public RSize PageSize
    {
        get => _containerInt.PageSize;
        set => _containerInt.PageSize = value;
    }

    public int MarginTop
    {
        get => _containerInt.MarginTop;
        set => _containerInt.MarginTop = value;
    }

    public int MarginBottom
    {
        get => _containerInt.MarginBottom;
        set => _containerInt.MarginBottom = value;
    }

    public int MarginLeft
    {
        get => _containerInt.MarginLeft;
        set => _containerInt.MarginLeft = value;
    }

    public int MarginRight
    {
        get => _containerInt.MarginRight;
        set => _containerInt.MarginRight = value;
    }

    public void PerformLayout(RGraphics g)
    {
        _containerInt.PerformLayout(g);
    }

    public void PerformPaint(RGraphics g)
    {
        _containerInt.PerformPaint(g);
    }

    public event EventHandler<HtmlImageLoadEventArgs>? ImageLoad
    {
        add => _containerInt.ImageLoad += value;
        remove => _containerInt.ImageLoad -= value;
    }

    public event EventHandler<HtmlStylesheetLoadEventArgs>? StylesheetLoad
    {
        add => _containerInt.StylesheetLoad += value;
        remove => _containerInt.StylesheetLoad -= value;
    }

    public event EventHandler<HtmlRenderErrorEventArgs>? RenderError
    {
        add => _containerInt.RenderError += value;
        remove => _containerInt.RenderError -= value;
    }

    public void Dispose()
    {
        _containerInt.Dispose();
    }
}

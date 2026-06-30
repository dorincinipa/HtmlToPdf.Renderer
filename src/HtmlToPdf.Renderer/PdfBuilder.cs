using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using HtmlToPdf.Renderer.Adapters;

namespace HtmlToPdf.Renderer;

public sealed class PdfBuilder
{
    private readonly PdfOptions _options = new();

    internal PdfBuilder() { }

    public PdfBuilder WithPageSize(XSize pageSize)
    {
        _options.PageSize = pageSize;
        return this;
    }

    public PdfBuilder WithOrientation(PageOrientation orientation)
    {
        _options.PageOrientation = orientation;
        return this;
    }

    public PdfBuilder WithMargin(double all)
    {
        _options.SetMargins(all);
        return this;
    }

    public PdfBuilder WithMargin(double vertical, double horizontal)
    {
        _options.SetMargins(vertical, horizontal);
        return this;
    }

    public PdfBuilder WithMargin(double top, double right, double bottom, double left)
    {
        _options.MarginTop = top;
        _options.MarginRight = right;
        _options.MarginBottom = bottom;
        _options.MarginLeft = left;
        return this;
    }

    public PdfBuilder WithFont(string familyName, byte[] fontData)
    {
        FontResolver.Instance.RegisterFont(familyName, fontData);
        return this;
    }

    public PdfBuilder WithPassword(string userPassword)
    {
        (_options.Security ??= new PdfSecurityOptions()).UserPassword = userPassword;
        return this;
    }

    public PdfBuilder WithOwnerPassword(string ownerPassword)
    {
        (_options.Security ??= new PdfSecurityOptions()).OwnerPassword = ownerPassword;
        return this;
    }

    public PdfBuilder WithHeader(string html)
    {
        _options.Header = html;
        return this;
    }

    public PdfBuilder WithFooter(string html)
    {
        _options.Footer = html;
        return this;
    }

    public PdfBuilder WithStyleSheet(string href, string cssContent)
    {
        (_options.StyleSheets ??= new Dictionary<string, string>())[href] = cssContent;
        return this;
    }

    public PdfBuilder WithLogger(ILogger logger)
    {
        _options.Logger = logger;
        return this;
    }

    public PdfBuilder WithOnProgress(Action<int, int> onProgress)
    {
        _options.OnProgress = onProgress;
        return this;
    }

    public PdfBuilder WithOnRenderError(Action<PdfRenderError> onRenderError)
    {
        _options.OnRenderError = onRenderError;
        return this;
    }

    public PdfBuilder WithContinueOnError(bool continueOnError = true)
    {
        _options.ContinueOnError = continueOnError;
        return this;
    }

    public PdfBuilder WithCompliance(PdfCompliance compliance)
    {
        _options.Compliance = compliance;
        return this;
    }

    public PdfBuilder WithIccProfile(byte[] iccProfileBytes)
    {
        _options.IccProfileBytes = iccProfileBytes;
        return this;
    }

    public PdfBuilder WithStamp(StampOptions stamp)
    {
        _options.Stamp = stamp;
        return this;
    }

    public PdfBuilder WithPermissions(PdfPermissions permissions)
    {
        (_options.Security ??= new PdfSecurityOptions()).Permissions = permissions;
        return this;
    }

    public PdfDocument GeneratePdf(string html)
        => PdfGenerator.GeneratePdf(html, _options);

    public PdfDocument GeneratePdf(IEnumerable<string> htmlPages)
        => PdfGenerator.GeneratePdf(htmlPages, _options);

    public Task<byte[]> GeneratePdfAsync(string html, CancellationToken ct = default)
        => PdfGenerator.GeneratePdfAsync(html, _options, ct);

    public Task<byte[]> GeneratePdfAsync(IEnumerable<string> htmlPages, CancellationToken ct = default)
        => PdfGenerator.GeneratePdfAsync(htmlPages, _options, ct);

    public PdfGenerateResult GeneratePdfWithResult(string html)
        => PdfGenerator.GeneratePdfWithResult(html, _options);

    public PdfDocument AppendTo(byte[] existingPdf, string html)
        => PdfGenerator.AppendHtml(existingPdf, html, _options);

    public Task<byte[]> AppendToAsync(byte[] existingPdf, string html, CancellationToken ct = default)
        => PdfGenerator.AppendHtmlAsync(existingPdf, html, _options, ct);

    public PdfDocument GeneratePdfFromFile(string filePath)
        => PdfGenerator.GeneratePdfFromFile(filePath, _options);

    public Task<byte[]> GeneratePdfFromFileAsync(string filePath, CancellationToken ct = default)
        => PdfGenerator.GeneratePdfFromFileAsync(filePath, _options, ct);
}

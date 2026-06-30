using System.Diagnostics;
using HtmlToPdf.Renderer.Adapters;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using HtmlToPdf.Renderer.HtmlEngine.Adapters.Entities;

namespace HtmlToPdf.Renderer;

public static class PdfGenerator
{
    public static PdfBuilder Create() => new();

    public static void LoadFontsFromFolder(string folderPath)
        => FontResolver.Instance.LoadFontsFromFolder(folderPath);

    public static PdfDocument GeneratePdf(string html, IRenderingOptions options, CancellationToken ct = default)
    {
        var document = new PdfDocument();
        ApplySecurity(document, options.Security);
        ApplyMetadata(document, options);
        ApplyPdfACompliance(document, options);
        AddPdfPages(document, html, options, ct);
        return document;
    }

    // Merge multiple HTML fragments into one PDF
    public static PdfDocument GeneratePdf(IEnumerable<string> htmlPages, IRenderingOptions options, CancellationToken ct = default)
    {
        var document = new PdfDocument();
        ApplySecurity(document, options.Security);
        ApplyMetadata(document, options);
        ApplyPdfACompliance(document, options);
        foreach (var html in htmlPages)
            AddPdfPages(document, html, options, ct);
        return document;
    }

    // CancellationToken support on async generation
    public static async Task<byte[]> GeneratePdfAsync(string html, IRenderingOptions options, CancellationToken ct = default)
    {
        using var document = GeneratePdf(html, options, ct);
        using var stream = new MemoryStream();
        await document.SaveAsync(stream, false);
        return stream.ToArray();
    }

    public static async Task<byte[]> GeneratePdfAsync(IEnumerable<string> htmlPages, IRenderingOptions options, CancellationToken ct = default)
    {
        using var document = GeneratePdf(htmlPages, options, ct);
        using var stream = new MemoryStream();
        await document.SaveAsync(stream, false);
        return stream.ToArray();
    }

    // Append HTML pages to an existing PDF
    public static PdfDocument AppendHtml(byte[] existingPdf, string html, IRenderingOptions options)
    {
        using var ms = new MemoryStream(existingPdf);
        var document = PdfReader.Open(ms, PdfDocumentOpenMode.Modify);
        AddPdfPages(document, html, options);
        return document;
    }

    public static async Task<byte[]> AppendHtmlAsync(byte[] existingPdf, string html, IRenderingOptions options, CancellationToken ct = default)
    {
        using var ms = new MemoryStream(existingPdf);
        using var document = PdfReader.Open(ms, PdfDocumentOpenMode.Modify);
        AddPdfPages(document, html, options, ct);
        using var stream = new MemoryStream();
        await document.SaveAsync(stream, false);
        return stream.ToArray();
    }

    // Extract a subset of pages (0-based indices) into a new document
    public static PdfDocument ExtractPages(byte[] pdfBytes, IEnumerable<int> pageIndices)
    {
        using var ms = new MemoryStream(pdfBytes);
        using var source = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        var result = new PdfDocument();
        foreach (var i in pageIndices)
            result.AddPage(source.Pages[i]);
        return result;
    }

    // Split every page into its own document
    public static PdfDocument[] SplitPages(byte[] pdfBytes)
    {
        using var ms = new MemoryStream(pdfBytes);
        using var source = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        var docs = new PdfDocument[source.PageCount];
        for (int i = 0; i < source.PageCount; i++)
        {
            docs[i] = new PdfDocument();
            docs[i].AddPage(source.Pages[i]);
        }
        return docs;
    }

    // Generate PDF from an HTML file, resolving relative asset paths
    public static PdfDocument GeneratePdfFromFile(string filePath, PdfOptions options, CancellationToken ct = default)
    {
        var html = File.ReadAllText(filePath);
        var baseDir = Path.GetDirectoryName(Path.GetFullPath(filePath))!;
        return GeneratePdf(html, options.CloneWithBaseDirectory(baseDir), ct);
    }

    public static async Task<byte[]> GeneratePdfFromFileAsync(string filePath, PdfOptions options, CancellationToken ct = default)
    {
        var html = await File.ReadAllTextAsync(filePath, ct);
        var baseDir = Path.GetDirectoryName(Path.GetFullPath(filePath))!;
        return await GeneratePdfAsync(html, options.CloneWithBaseDirectory(baseDir), ct);
    }

    // Generate PDF and return result metadata (page count, render time, warnings)
    public static PdfGenerateResult GeneratePdfWithResult(string html, IRenderingOptions options)
    {
        var warnings = new List<PdfRenderError>();
        var capturing = new CapturingOptions(options, err =>
        {
            warnings.Add(err);
            options.OnRenderError?.Invoke(err);
        });
        var sw = Stopwatch.StartNew();
        var document = GeneratePdf(html, capturing);
        sw.Stop();
        return new PdfGenerateResult(document, document.PageCount, sw.Elapsed, warnings);
    }

    public static void AddPdfPages(PdfDocument document, string html, IRenderingOptions options, CancellationToken ct = default)
    {
        try
        {
            AddPdfPagesCore(document, html, options, ct);
        }
        catch (Exception ex) when (options.ContinueOnError && ex is not OperationCanceledException)
        {
            var error = new PdfRenderError("General", ex.Message, ex);
            options.OnRenderError?.Invoke(error);
            options.Logger?.LogError(ex, "HtmlToPdf [General] {Message}", ex.Message);
        }
    }

    private static void AddPdfPagesCore(PdfDocument document, string html, IRenderingOptions options, CancellationToken ct = default)
    {
        var adapter = RenderAdapter.Instance;
        var effectivePageSize = options.GetEffectivePageSize();

        double pageWidth = effectivePageSize.Width;
        double pageHeight = effectivePageSize.Height;
        double contentWidth = pageWidth - options.MarginLeft - options.MarginRight;
        double contentHeight = pageHeight - options.MarginTop - options.MarginBottom;

        using var container = new HtmlContainer(adapter);

        if (!string.IsNullOrEmpty(options.BaseDirectory))
        {
            var baseDir = options.BaseDirectory;
            container.StylesheetLoad += (_, args) =>
            {
                if (IsRelativePath(args.Src))
                {
                    var fullPath = Path.GetFullPath(Path.Combine(baseDir, args.Src));
                    if (File.Exists(fullPath))
                        args.SetStyleSheet = File.ReadAllText(fullPath);
                }
            };
            container.ImageLoad += (_, args) =>
            {
                if (IsRelativePath(args.Src))
                {
                    var fullPath = Path.GetFullPath(Path.Combine(baseDir, args.Src));
                    if (File.Exists(fullPath))
                        args.Callback(fullPath);
                }
            };
        }

        if (options.StyleSheets?.Count > 0)
        {
            container.StylesheetLoad += (_, args) =>
            {
                if (options.StyleSheets.TryGetValue(args.Src, out var css))
                    args.SetStyleSheet = css;
            };
        }

        if (options.Logger != null || options.OnRenderError != null)
        {
            var logger = options.Logger;
            var onError = options.OnRenderError;
            container.RenderError += (_, args) =>
            {
                onError?.Invoke(new PdfRenderError(args.Type.ToString(), args.Message, args.Exception));
                if (args.Exception != null)
                    logger?.LogError(args.Exception, "HtmlToPdf [{ErrorType}] {Message}", args.Type, args.Message);
                else
                    logger?.LogWarning("HtmlToPdf [{ErrorType}] {Message}", args.Type, args.Message);
            };
        }

        container.SetHtml(html);

        container.PageSize = new RSize(pageWidth, contentHeight);
        container.MarginTop = (int)Math.Round(options.MarginTop);
        container.MarginBottom = (int)Math.Round(options.MarginBottom);
        container.MarginLeft = (int)Math.Round(options.MarginLeft);
        container.MarginRight = (int)Math.Round(options.MarginRight);
        container.MaxSize = new RSize(contentWidth, 0);
        container.Location = new RPoint(options.MarginLeft, options.MarginTop);

        using var measureContext = XGraphics.CreateMeasureContext(new XSize(pageWidth, pageHeight), XGraphicsUnit.Point, XPageDirection.Downwards);
        using var measureGraphics = new GraphicsAdapter(measureContext, adapter, new RRect(0, 0, pageWidth, pageHeight));

        // Lay out the whole document once at its natural position. Each page is then rendered by
        // translating the painter to bring that page's vertical slice into view and clipping to it.
        // Re-laying out per page (at a negative offset) is not safe: the layout engine assumes a
        // non-negative origin, so it would corrupt the layout and drop trailing content.
        container.PerformLayout(measureGraphics);

        double totalHeight = container.ActualSize.Height;
        int pageCount = Math.Max(1, (int)Math.Ceiling(totalHeight / contentHeight));

        var bodyBg = container.BodyBackgroundColor;
        XSolidBrush? bodyBgBrush = null;
        if (!bodyBg.IsEmpty && bodyBg.A > 0)
            bodyBgBrush = new XSolidBrush(XColor.FromArgb(bodyBg.A, bodyBg.R, bodyBg.G, bodyBg.B));

        for (int i = 0; i < pageCount; i++)
        {
            ct.ThrowIfCancellationRequested();

            var page = document.AddPage();
            page.Width = XUnit.FromPoint(pageWidth);
            page.Height = XUnit.FromPoint(pageHeight);

            using var xGraphics = XGraphics.FromPdfPage(page);
            using var pageGraphics = new GraphicsAdapter(
                xGraphics, adapter, new RRect(0, 0, pageWidth, pageHeight));

            if (bodyBgBrush != null)
                xGraphics.DrawRectangle(bodyBgBrush, 0, 0, pageWidth, pageHeight);

            var state = xGraphics.Save();
            xGraphics.TranslateTransform(0, -i * contentHeight);
            var clip = new RRect(options.MarginLeft, options.MarginTop + i * contentHeight, contentWidth, contentHeight);
            container.PerformPaint(pageGraphics, clip);
            xGraphics.Restore(state);

            if (!string.IsNullOrEmpty(options.Header))
                RenderHeaderFooter(pageGraphics, adapter,
                    Substitute(options.Header, i + 1, pageCount),
                    options.MarginLeft, 0, contentWidth, pageWidth);

            if (!string.IsNullOrEmpty(options.Footer))
                RenderHeaderFooter(pageGraphics, adapter,
                    Substitute(options.Footer, i + 1, pageCount),
                    options.MarginLeft, pageHeight - options.MarginBottom, contentWidth, pageWidth);

            if (options.Stamp != null)
                DrawStamp(xGraphics, options.Stamp, pageWidth, pageHeight);

            options.OnProgress?.Invoke(i + 1, pageCount);
        }
    }

    // Wraps IRenderingOptions to capture OnRenderError calls into a warnings list
    private sealed class CapturingOptions(IRenderingOptions inner, Action<PdfRenderError> onRenderError) : IRenderingOptions
    {
        public XSize PageSize => inner.PageSize;
        public double MarginTop => inner.MarginTop;
        public double MarginBottom => inner.MarginBottom;
        public double MarginLeft => inner.MarginLeft;
        public double MarginRight => inner.MarginRight;
        public PageOrientation PageOrientation => inner.PageOrientation;
        public PdfSecurityOptions? Security => inner.Security;
        public string? Title => inner.Title;
        public string? Author => inner.Author;
        public string? Subject => inner.Subject;
        public string? Keywords => inner.Keywords;
        public string? Header => inner.Header;
        public string? Footer => inner.Footer;
        public StampOptions? Stamp => inner.Stamp;
        public Dictionary<string, string>? StyleSheets => inner.StyleSheets;
        public ILogger? Logger => inner.Logger;
        public string? BaseDirectory => inner.BaseDirectory;
        public Action<int, int>? OnProgress => inner.OnProgress;
        public Action<PdfRenderError>? OnRenderError => onRenderError;
        public bool ContinueOnError => inner.ContinueOnError;
        public PdfCompliance Compliance => inner.Compliance;
        public byte[]? IccProfileBytes => inner.IccProfileBytes;
        public XSize GetEffectivePageSize() => inner.GetEffectivePageSize();
    }

    private static bool IsRelativePath(string src) =>
        !string.IsNullOrEmpty(src) &&
        !Uri.TryCreate(src, UriKind.Absolute, out _) &&
        !Path.IsPathRooted(src);

    private static string Substitute(string html, int pageNumber, int totalPages) =>
        html.Replace("{{PageNumber}}", pageNumber.ToString())
            .Replace("{{TotalPages}}", totalPages.ToString());

    private static void RenderHeaderFooter(
        GraphicsAdapter gfx, RenderAdapter adapter,
        string html, double x, double y, double width, double pageWidth)
    {
        using var container = new HtmlContainer(adapter);
        container.SetHtml(html);
        container.PageSize = new RSize(pageWidth, 100_000);
        container.MaxSize = new RSize(width, 0);
        container.Location = new RPoint(x, y);
        container.PerformLayout(gfx);
        container.PerformPaint(gfx);
    }

    private static void DrawStamp(XGraphics g, StampOptions stamp, double pageWidth, double pageHeight)
    {
        var state = g.Save();
        try
        {
            if (stamp.Text != null)
                DrawTextStamp(g, stamp, pageWidth, pageHeight);
            else if (stamp.ImageData != null)
                DrawImageStamp(g, stamp, pageWidth, pageHeight);
        }
        finally
        {
            g.Restore(state);
        }
    }

    private static void DrawTextStamp(XGraphics g, StampOptions stamp, double pageWidth, double pageHeight)
    {
        var font = new XFont("Arial", stamp.FontSize, XFontStyleEx.Bold);
        var base_ = stamp.Color;
        var color = XColor.FromArgb(
            (int)(stamp.Opacity * 255),
            (int)(base_.R * 255),
            (int)(base_.G * 255),
            (int)(base_.B * 255));
        var brush = new XSolidBrush(color);

        g.TranslateTransform(pageWidth / 2, pageHeight / 2);
        g.RotateTransform(stamp.Angle);

        var size = g.MeasureString(stamp.Text!, font);
        g.DrawString(stamp.Text!, font, brush,
            new XRect(-size.Width / 2, -size.Height / 2, size.Width, size.Height),
            XStringFormats.Center);
    }

    private const double StampEdgeMargin = 20;

    private static void DrawImageStamp(XGraphics g, StampOptions stamp, double pageWidth, double pageHeight)
    {
        using var xImage = XImage.FromStream(new MemoryStream(stamp.ImageData!));

        double w = stamp.ImageWidth  > 0 ? stamp.ImageWidth  : xImage.PointWidth;
        double h = stamp.ImageHeight > 0 ? stamp.ImageHeight : xImage.PointHeight;

        var (x, y) = stamp.Position switch
        {
            StampPosition.TopLeft     => (StampEdgeMargin, StampEdgeMargin),
            StampPosition.TopRight    => (pageWidth  - w - StampEdgeMargin, StampEdgeMargin),
            StampPosition.BottomLeft  => (StampEdgeMargin, pageHeight - h - StampEdgeMargin),
            StampPosition.BottomRight => (pageWidth  - w - StampEdgeMargin, pageHeight - h - StampEdgeMargin),
            _                         => ((pageWidth - w) / 2, (pageHeight - h) / 2),
        };

        if (stamp.Angle != 0)
        {
            g.TranslateTransform(x + w / 2, y + h / 2);
            g.RotateTransform(stamp.Angle);
            g.DrawImage(xImage, new XRect(-w / 2, -h / 2, w, h));
        }
        else
        {
            g.DrawImage(xImage, new XRect(x, y, w, h));
        }
    }

    private static void ApplyPdfACompliance(PdfDocument document, IRenderingOptions options)
    {
        if (options.Compliance == PdfCompliance.None) return;

        if (options.Security != null)
            throw new InvalidOperationException(
                "PDF/A compliance cannot be combined with PDF security — encryption is not permitted by the PDF/A standard.");

        var catalog = document.Internals.Catalog;

        // XMP metadata stream with pdfaid markers
        var xmpBytes = System.Text.Encoding.UTF8.GetBytes(BuildXmpMetadata());
        var metaDict = new PdfDictionary(document);
        metaDict.Elements["/Type"] = new PdfName("/Metadata");
        metaDict.Elements["/Subtype"] = new PdfName("/XML");
        metaDict.CreateStream(xmpBytes);
        document.Internals.AddObject(metaDict);
        catalog.Elements["/Metadata"] = metaDict.Reference;

        // OutputIntents — required for PDF/A-1b to declare the intended output color space
        var outputIntentDict = new PdfDictionary(document);
        outputIntentDict.Elements["/Type"] = new PdfName("/OutputIntent");
        outputIntentDict.Elements["/S"] = new PdfName("/GTS_PDFA1");
        outputIntentDict.Elements["/OutputConditionIdentifier"] = new PdfString("sRGB IEC61966-2.1");

        if (options.IccProfileBytes?.Length > 0)
        {
            var iccDict = new PdfDictionary(document);
            iccDict.Elements["/N"] = new PdfInteger(3); // 3 = RGB channels
            iccDict.CreateStream(options.IccProfileBytes);
            document.Internals.AddObject(iccDict);
            outputIntentDict.Elements["/DestOutputProfile"] = iccDict.Reference;
        }

        document.Internals.AddObject(outputIntentDict);

        var outputIntents = new PdfArray(document, outputIntentDict.Reference);
        catalog.Elements["/OutputIntents"] = outputIntents;
    }

    private static string BuildXmpMetadata() => """
        <?xpacket begin="﻿" id="W5M0MpCehiHzreSzNTczkc9d"?>
        <x:xmpmeta xmlns:x="adobe:ns:meta/">
          <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
            <rdf:Description rdf:about="" xmlns:pdfaid="http://www.aiim.org/pdfa/ns/id/">
              <pdfaid:part>1</pdfaid:part>
              <pdfaid:conformance>B</pdfaid:conformance>
            </rdf:Description>
          </rdf:RDF>
        </x:xmpmeta>
        <?xpacket end="w"?>
        """;

    private static void ApplyMetadata(PdfDocument document, IRenderingOptions options)
    {
        if (options.Title    != null) document.Info.Title    = options.Title;
        if (options.Author   != null) document.Info.Author   = options.Author;
        if (options.Subject  != null) document.Info.Subject  = options.Subject;
        if (options.Keywords != null) document.Info.Keywords = options.Keywords;
    }

    private static void ApplySecurity(PdfDocument document, PdfSecurityOptions? security)
    {
        if (security is null) return;

        if (string.IsNullOrEmpty(security.UserPassword) &&
            string.IsNullOrEmpty(security.OwnerPassword))
        {
            throw new InvalidOperationException(
                "PdfSecurityOptions requires at least UserPassword or OwnerPassword.");
        }

        var s = document.SecuritySettings;
        if (!string.IsNullOrEmpty(security.UserPassword))  s.UserPassword  = security.UserPassword;
        if (!string.IsNullOrEmpty(security.OwnerPassword)) s.OwnerPassword = security.OwnerPassword;

        var p = security.Permissions;
        s.PermitPrint            = p.HasFlag(PdfPermissions.Print);
        s.PermitFullQualityPrint = p.HasFlag(PdfPermissions.HighQualityPrint);
        s.PermitModifyDocument   = p.HasFlag(PdfPermissions.ModifyContent);
        s.PermitExtractContent   = p.HasFlag(PdfPermissions.CopyContent);
        s.PermitAnnotations      = p.HasFlag(PdfPermissions.Annotate);
        s.PermitFormsFill        = p.HasFlag(PdfPermissions.FillForms);
        s.PermitAssembleDocument = p.HasFlag(PdfPermissions.AssembleDocument);
    }
}

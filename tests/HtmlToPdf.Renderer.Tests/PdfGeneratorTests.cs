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

    // #27 - Non-ASCII character encoding
    [Fact]
    public void GeneratePdf_NonAsciiCharacters_DoesNotThrow()
    {
        var html = "<p>Romanian: ăîşţâ | Chinese: 中文 | Arabic: عربي | Emoji: \U0001F600</p>";
        var ex = Record.Exception(() => PdfGenerator.GeneratePdf(html, DefaultConfig()));
        Assert.Null(ex);
    }

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
    public void GeneratePdf_LeadingHeading_DoesNotForceBlankFirstPage()
    {
        // A short document that starts with an h1 must fit on a single page. The default
        // stylesheet must not force a page break before headings, which would leave page 1 blank.
        var html = "<div style='padding:10px;'><h1>Title</h1><p>Body content.</p></div>";
        var doc = PdfGenerator.GeneratePdf(html, DefaultConfig());
        Assert.Equal(1, doc.PageCount);
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

    // #38 - Merge multiple HTML fragments
    [Fact]
    public void GeneratePdf_MultipleHtml_MergesAllPages()
    {
        var cfg = DefaultConfig();
        var pages1 = PdfGenerator.GeneratePdf("<p>Part 1</p>", cfg).PageCount;
        var pages2 = PdfGenerator.GeneratePdf("<p>Part 2</p>", cfg).PageCount;

        using var merged = PdfGenerator.GeneratePdf(["<p>Part 1</p>", "<p>Part 2</p>"], cfg);

        Assert.Equal(pages1 + pages2, merged.PageCount);
    }

    [Fact]
    public async Task GeneratePdfAsync_MultipleHtml_ReturnsMergedBytes()
    {
        var bytes = await PdfGenerator.GeneratePdfAsync(["<p>A</p>", "<p>B</p>"], DefaultConfig());

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
        Assert.Equal((byte)'%', bytes[0]);
    }

    // #39 - Append HTML to existing PDF
    [Fact]
    public async Task AppendHtml_AddsPagesToBeyondOriginal()
    {
        var cfg = DefaultConfig();
        var originalBytes = await PdfGenerator.GeneratePdfAsync("<p>Original</p>", cfg);
        var originalPageCount = PdfGenerator.GeneratePdf("<p>Original</p>", cfg).PageCount;
        var appendPageCount  = PdfGenerator.GeneratePdf("<p>Appended</p>", cfg).PageCount;

        using var result = PdfGenerator.AppendHtml(originalBytes, "<p>Appended</p>", cfg);

        Assert.Equal(originalPageCount + appendPageCount, result.PageCount);
    }

    [Fact]
    public async Task AppendHtmlAsync_ReturnsBytesWithExtraPages()
    {
        var cfg = DefaultConfig();
        var originalBytes = await PdfGenerator.GeneratePdfAsync("<p>Original</p>", cfg);

        var resultBytes = await PdfGenerator.AppendHtmlAsync(originalBytes, "<p>Appended</p>", cfg);

        Assert.True(resultBytes.Length > originalBytes.Length);
    }

    // #40 - Extract pages
    [Fact]
    public void ExtractPages_ReturnsOnlyRequestedPages()
    {
        var cfg = DefaultConfig();
        var manyLines = string.Concat(Enumerable.Repeat("<p>Line</p>", 200));
        byte[] sourceBytes;
        using (var doc = PdfGenerator.GeneratePdf($"<div>{manyLines}</div>", cfg))
        using (var ms = new MemoryStream())
        {
            doc.Save(ms, false);
            sourceBytes = ms.ToArray();
        }

        using var extracted = PdfGenerator.ExtractPages(sourceBytes, [0, 1]);

        Assert.Equal(2, extracted.PageCount);
    }

    [Fact]
    public void SplitPages_EachDocumentHasExactlyOnePage()
    {
        var cfg = DefaultConfig();
        var manyLines = string.Concat(Enumerable.Repeat("<p>Line</p>", 200));
        using var multiPage = PdfGenerator.GeneratePdf($"<div>{manyLines}</div>", cfg);
        var sourcePageCount = multiPage.PageCount;

        using var ms = new MemoryStream();
        multiPage.Save(ms, false);
        var sourceBytes = ms.ToArray();

        var split = PdfGenerator.SplitPages(sourceBytes);
        try
        {
            Assert.Equal(sourcePageCount, split.Length);
            Assert.All(split, doc => Assert.Equal(1, doc.PageCount));
        }
        finally
        {
            foreach (var doc in split) doc.Dispose();
        }
    }

    // #45 - GeneratePdfFromFile
    [Fact]
    public void GeneratePdfFromFile_ReturnsValidPdf()
    {
        var dir = Directory.CreateTempSubdirectory("HtmlToPdfTest").FullName;
        try
        {
            var htmlPath = Path.Combine(dir, "test.html");
            File.WriteAllText(htmlPath, "<p>Hello from file</p>");

            using var doc = PdfGenerator.GeneratePdfFromFile(htmlPath, DefaultConfig());

            Assert.NotNull(doc);
            Assert.True(doc.PageCount >= 1);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void GeneratePdfFromFile_ResolvesRelativeStylesheet()
    {
        var dir = Directory.CreateTempSubdirectory("HtmlToPdfTest").FullName;
        try
        {
            File.WriteAllText(Path.Combine(dir, "styles.css"),
                "body { color: red; font-size: 14pt; }");
            File.WriteAllText(Path.Combine(dir, "index.html"),
                "<html><head><link rel='stylesheet' href='styles.css'/></head><body><p>Styled</p></body></html>");

            var ex = Record.Exception(() =>
                PdfGenerator.GeneratePdfFromFile(Path.Combine(dir, "index.html"), DefaultConfig()));

            Assert.Null(ex);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void GeneratePdfFromFile_ResolvesRelativeImage()
    {
        // Minimal 1x1 transparent PNG
        var pngBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

        var dir = Directory.CreateTempSubdirectory("HtmlToPdfTest").FullName;
        try
        {
            File.WriteAllBytes(Path.Combine(dir, "img.png"), pngBytes);
            File.WriteAllText(Path.Combine(dir, "index.html"),
                "<html><body><img src='img.png' width='10' height='10'/></body></html>");

            var ex = Record.Exception(() =>
                PdfGenerator.GeneratePdfFromFile(Path.Combine(dir, "index.html"), DefaultConfig()));

            Assert.Null(ex);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public async Task GeneratePdfFromFileAsync_ReturnsPdfBytes()
    {
        var dir = Directory.CreateTempSubdirectory("HtmlToPdfTest").FullName;
        try
        {
            var htmlPath = Path.Combine(dir, "test.html");
            await File.WriteAllTextAsync(htmlPath, "<p>Async file test</p>");

            var bytes = await PdfGenerator.GeneratePdfFromFileAsync(htmlPath, DefaultConfig());

            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
            Assert.Equal((byte)'%', bytes[0]);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // #65 - OnProgress callback
    [Fact]
    public void OnProgress_InvokedOncePerPage()
    {
        var manyLines = string.Concat(Enumerable.Repeat("<p>Line</p>", 200));
        var cfg = DefaultConfig();
        using var reference = PdfGenerator.GeneratePdf($"<div>{manyLines}</div>", cfg);
        int expectedPages = reference.PageCount;

        var calls = new List<(int current, int total)>();
        cfg.OnProgress = (current, total) => calls.Add((current, total));

        PdfGenerator.GeneratePdf($"<div>{manyLines}</div>", cfg);

        Assert.Equal(expectedPages, calls.Count);
        Assert.All(calls, c => Assert.Equal(expectedPages, c.total));
        for (int i = 0; i < calls.Count; i++)
            Assert.Equal(i + 1, calls[i].current);
    }

    [Fact]
    public void OnProgress_SinglePage_InvokedOnce()
    {
        var calls = new List<(int current, int total)>();
        var cfg = DefaultConfig();
        cfg.OnProgress = (c, t) => calls.Add((c, t));

        PdfGenerator.GeneratePdf("<p>Single page</p>", cfg);

        Assert.Single(calls);
        Assert.Equal((1, 1), calls[0]);
    }

    // #66 - OnRenderError / ContinueOnError
    [Fact]
    public void OnRenderError_InvokedForMissingStylesheet()
    {
        var errors = new List<PdfRenderError>();
        var cfg = DefaultConfig();
        cfg.OnRenderError = errors.Add;

        // A <link> to a non-existent stylesheet triggers a render error from the engine
        PdfGenerator.GeneratePdf(
            "<html><head><link rel='stylesheet' href='nonexistent_abc123.css'/></head><body><p>x</p></body></html>",
            cfg);

        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ContinueOnError_True_DoesNotThrowOnNormalRender()
    {
        var cfg = DefaultConfig();
        cfg.ContinueOnError = true;

        var ex = Record.Exception(() => PdfGenerator.GeneratePdf("<p>Normal</p>", cfg));

        Assert.Null(ex);
    }

    [Fact]
    public void ContinueOnError_False_IsDefault()
    {
        var cfg = new PdfOptions();
        Assert.False(cfg.ContinueOnError);
    }

    [Fact]
    public void OnRenderError_AndLogger_BothFireForSameError()
    {
        var errors = new List<PdfRenderError>();
        var loggedWarnings = new List<string>();

        var cfg = DefaultConfig();
        cfg.OnRenderError = errors.Add;
        cfg.Logger = new CapturingLogger(loggedWarnings);

        PdfGenerator.GeneratePdf(
            "<html><head><link rel='stylesheet' href='no_such_file_xyz.css'/></head><body><p>x</p></body></html>",
            cfg);

        Assert.NotEmpty(errors);
        Assert.NotEmpty(loggedWarnings);
    }

    private sealed class CapturingLogger(List<string> warnings) : Microsoft.Extensions.Logging.ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            warnings.Add(formatter(state, exception));
        }
    }

    [Fact]
    public void GeneratePdfFromFile_ExplicitStyleSheetOverridesFileResolution()
    {
        var dir = Directory.CreateTempSubdirectory("HtmlToPdfTest").FullName;
        try
        {
            // File on disk has one color; StyleSheets dict overrides it — no exception expected
            File.WriteAllText(Path.Combine(dir, "styles.css"), "body { color: blue; }");
            File.WriteAllText(Path.Combine(dir, "index.html"),
                "<html><head><link rel='stylesheet' href='styles.css'/></head><body><p>Override</p></body></html>");

            var cfg = DefaultConfig();
            cfg.StyleSheets = new Dictionary<string, string> { ["styles.css"] = "body { color: green; }" };

            var ex = Record.Exception(() =>
                PdfGenerator.GeneratePdfFromFile(Path.Combine(dir, "index.html"), cfg));

            Assert.Null(ex);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // #61 - IRenderingOptions interface
    [Fact]
    public void PdfOptions_ImplementsIRenderingOptions()
    {
        IRenderingOptions opts = new PdfOptions
        {
            MarginTop = 10,
            MarginBottom = 10,
            MarginLeft = 10,
            MarginRight = 10
        };

        Assert.Equal(10, opts.MarginTop);
        Assert.Equal(10, opts.MarginBottom);
    }

    [Fact]
    public void GeneratePdf_AcceptsIRenderingOptions()
    {
        IRenderingOptions opts = DefaultConfig();
        var ex = Record.Exception(() => PdfGenerator.GeneratePdf("<p>Hello</p>", opts));
        Assert.Null(ex);
    }

    [Fact]
    public void GeneratePdf_IRenderingOptions_GetEffectivePageSize_Landscape()
    {
        IRenderingOptions opts = new PdfOptions
        {
            PageSize = PageSize.A4,
            PageOrientation = PageOrientation.Landscape
        };

        var size = opts.GetEffectivePageSize();
        Assert.True(size.Width > size.Height);
    }

    // #64 - CancellationToken
    [Fact]
    public async Task GeneratePdfAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            PdfGenerator.GeneratePdfAsync(
                string.Concat(Enumerable.Repeat("<p>Line</p>", 300)),
                DefaultConfig(),
                cts.Token));
    }

    [Fact]
    public async Task GeneratePdfAsync_NotCancelled_Succeeds()
    {
        var bytes = await PdfGenerator.GeneratePdfAsync("<p>Hello</p>", DefaultConfig(), CancellationToken.None);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void GeneratePdf_CancelledToken_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            PdfGenerator.GeneratePdf(
                string.Concat(Enumerable.Repeat("<p>Line</p>", 300)),
                DefaultConfig(),
                cts.Token));
    }

    [Fact]
    public void ContinueOnError_DoesNotSwallowOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var cfg = DefaultConfig();
        cfg.ContinueOnError = true;

        Assert.ThrowsAny<OperationCanceledException>(() =>
            PdfGenerator.GeneratePdf(
                string.Concat(Enumerable.Repeat("<p>Line</p>", 300)),
                cfg,
                cts.Token));
    }

    // #67 - PdfGenerateResult
    [Fact]
    public void GeneratePdfWithResult_ReturnsPageCount()
    {
        using var result = PdfGenerator.GeneratePdfWithResult("<p>Hello</p>", DefaultConfig());
        Assert.True(result.PageCount >= 1);
        Assert.Equal(result.Document.PageCount, result.PageCount);
    }

    [Fact]
    public void GeneratePdfWithResult_ReturnsRenderTime()
    {
        using var result = PdfGenerator.GeneratePdfWithResult("<p>Hello</p>", DefaultConfig());
        Assert.True(result.RenderTime > TimeSpan.Zero);
    }

    [Fact]
    public void GeneratePdfWithResult_NoWarnings_EmptyList()
    {
        using var result = PdfGenerator.GeneratePdfWithResult("<p>Hello</p>", DefaultConfig());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void GeneratePdfWithResult_WithRenderError_CapturesWarning()
    {
        var cfg = DefaultConfig();
        cfg.ContinueOnError = true;
        // Force a warning by pointing to a stylesheet that cannot be loaded by the base-dir handler
        // (no BaseDirectory set, so a missing link fires a render warning)
        using var result = PdfGenerator.GeneratePdfWithResult(
            "<html><head><link rel='stylesheet' href='no_such_xyz.css'/></head><body><p>x</p></body></html>",
            cfg);

        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public void GeneratePdfWithResult_OriginalOnRenderError_StillFires()
    {
        var externalErrors = new List<PdfRenderError>();
        var cfg = DefaultConfig();
        cfg.ContinueOnError = true;
        cfg.OnRenderError = externalErrors.Add;

        using var result = PdfGenerator.GeneratePdfWithResult(
            "<html><head><link rel='stylesheet' href='no_such_xyz.css'/></head><body><p>x</p></body></html>",
            cfg);

        Assert.NotEmpty(result.Warnings);
        Assert.Equal(result.Warnings.Count, externalErrors.Count);
    }

    [Fact]
    public void PdfGenerateResult_Dispose_ClosesDocument()
    {
        var result = PdfGenerator.GeneratePdfWithResult("<p>Hello</p>", DefaultConfig());
        var doc = result.Document;
        result.Dispose();

        // Double-dispose should not throw
        var ex = Record.Exception(() => result.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void PdfBuilder_GeneratePdfWithResult_Works()
    {
        using var result = PdfGenerator.Create()
            .WithMargin(20)
            .GeneratePdfWithResult("<p>Builder result</p>");

        Assert.True(result.PageCount >= 1);
        Assert.NotNull(result.Document);
    }

    // #49 - PDF/A-1b compliance
    [Fact]
    public void GeneratePdf_PdfA1B_DoesNotThrow()
    {
        var cfg = DefaultConfig();
        cfg.Compliance = PdfCompliance.PdfA1B;

        var ex = Record.Exception(() => PdfGenerator.GeneratePdf("<p>Hello PDF/A</p>", cfg));

        Assert.Null(ex);
    }

    [Fact]
    public void GeneratePdf_PdfA1B_DocumentHasPages()
    {
        var cfg = DefaultConfig();
        cfg.Compliance = PdfCompliance.PdfA1B;

        using var doc = PdfGenerator.GeneratePdf("<p>Hello PDF/A</p>", cfg);

        Assert.True(doc.PageCount >= 1);
    }

    [Fact]
    public void GeneratePdf_PdfA1B_WithSecurity_Throws()
    {
        var cfg = DefaultConfig();
        cfg.Compliance = PdfCompliance.PdfA1B;
        cfg.Security = new PdfSecurityOptions { UserPassword = "secret" };

        Assert.Throws<InvalidOperationException>(() =>
            PdfGenerator.GeneratePdf("<p>Hello</p>", cfg));
    }

    [Fact]
    public void GeneratePdf_PdfA1B_WithIccProfile_DoesNotThrow()
    {
        var cfg = DefaultConfig();
        cfg.Compliance = PdfCompliance.PdfA1B;
        // Minimal synthetic ICC profile bytes — real profiles are ~3KB;
        // this test just verifies the code path runs without error
        cfg.IccProfileBytes = new byte[128];

        var ex = Record.Exception(() => PdfGenerator.GeneratePdf("<p>Hello PDF/A</p>", cfg));

        Assert.Null(ex);
    }

    [Fact]
    public void PdfBuilder_WithCompliance_FluentApi()
    {
        var ex = Record.Exception(() =>
            PdfGenerator.Create()
                .WithMargin(20)
                .WithCompliance(PdfCompliance.PdfA1B)
                .GeneratePdf("<p>PDF/A via builder</p>"));

        Assert.Null(ex);
    }

    [Fact]
    public void GeneratePdf_NoCompliance_DefaultIsNone()
    {
        var cfg = new PdfOptions();
        Assert.Equal(PdfCompliance.None, cfg.Compliance);
    }
}

# HtmlToPdf.Renderer

[![NuGet](https://img.shields.io/nuget/v/HtmlToPdf.Renderer.svg)](https://www.nuget.org/packages/HtmlToPdf.Renderer)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A .NET 8 library that converts HTML/CSS to PDF using [PDFsharp 6.x](https://github.com/empira/PDFsharp) and [HtmlRenderer](https://github.com/ArthurHub/HTML-Renderer).

---

## Quick start

```csharp
// Fluent builder
var bytes = await PdfGenerator.Create()
    .WithPageSize(PageSize.A4)
    .WithMargin(40)
    .GeneratePdfAsync("<h1>Hello</h1><p>World</p>");

await File.WriteAllBytesAsync("output.pdf", bytes);

// Or save PdfDocument directly
var doc = PdfGenerator.Create()
    .WithPageSize(PageSize.A4)
    .WithMargin(40)
    .GeneratePdf(html);

doc.Save("output.pdf");
```

---

## Page setup

```csharp
PdfGenerator.Create()
    .WithPageSize(PageSize.A4)                        // A4, Letter, Legal, A3, or any XSize
    .WithOrientation(PageOrientation.Landscape)
    .WithMargin(top: 20, right: 20, bottom: 20, left: 20)
    .GeneratePdf(html);
```

---

## Fonts

Register a custom font from bytes or load a whole folder:

```csharp
byte[] fontBytes = File.ReadAllBytes("fonts/Roboto-Regular.ttf");

PdfGenerator.Create()
    .WithFont("Roboto", fontBytes)
    .GeneratePdf("<p style='font-family:Roboto;'>Custom font</p>");

// Or load all fonts from a folder at startup
PdfGenerator.LoadFontsFromFolder("fonts/");
```

---

## External CSS

Supply a dictionary of `href -> CSS content`. The engine resolves `<link rel="stylesheet">` hrefs against it:

```csharp
var css = "body { font-family: Arial; } .card { border: 1px solid #ccc; padding: 12px; }";

PdfGenerator.Create()
    .WithStyleSheet("styles.css", css)
    .GeneratePdf("<link rel='stylesheet' href='styles.css'/><div class='card'>Hello</div>");
```

---

## Generate from file

Reads an HTML file from disk and automatically resolves relative `<link>` and `<img>` paths against the file's directory:

```csharp
// Stylesheets and images referenced with relative paths just work
var doc = PdfGenerator.GeneratePdfFromFile("report/index.html", options);

// Async
var bytes = await PdfGenerator.GeneratePdfFromFileAsync("report/index.html", options);

// Builder
var doc = PdfGenerator.Create()
    .WithMargin(20)
    .GeneratePdfFromFile("report/index.html");
```

---

## Merge multiple HTML fragments

```csharp
var pages = new[]
{
    "<h1>Cover page</h1>",
    "<h1>Chapter 1</h1><p>Content...</p>",
    "<h1>Chapter 2</h1><p>Content...</p>",
};

// Static API
var doc = PdfGenerator.GeneratePdf(pages, options);

// Builder
var doc = PdfGenerator.Create().WithMargin(20).GeneratePdf(pages);
```

---

## Append HTML to an existing PDF

```csharp
byte[] existing = File.ReadAllBytes("report.pdf");

// Returns PdfDocument — call Save() or serialize to bytes
using var doc = PdfGenerator.AppendHtml(existing, "<h1>Appendix</h1>", options);
doc.Save("report-with-appendix.pdf");

// Async (returns byte[])
var bytes = await PdfGenerator.AppendHtmlAsync(existing, "<h1>Appendix</h1>", options);
```

---

## Split and extract pages

```csharp
byte[] pdfBytes = File.ReadAllBytes("multi-page.pdf");

// Extract specific pages (0-based)
using var extracted = PdfGenerator.ExtractPages(pdfBytes, [0, 2, 4]);

// Split every page into its own document
PdfDocument[] pages = PdfGenerator.SplitPages(pdfBytes);
```

---

## Header and footer

Use `{{PageNumber}}` and `{{TotalPages}}` as placeholders. The header/footer HTML is rendered independently on every page:

```csharp
PdfGenerator.Create()
    .WithHeader("<div style='font-size:9px;color:#888;'>My Report</div>")
    .WithFooter("<div style='font-size:9px;text-align:right;'>Page {{PageNumber}} of {{TotalPages}}</div>")
    .GeneratePdf(html);
```

---

## Watermark / stamp

```csharp
// Text stamp
var options = new PdfOptions
{
    Stamp = new StampOptions
    {
        Text = "DRAFT",
        FontSize = 60,
        Color = new RColor(1, 0, 0),   // R G B — floats 0-1
        Opacity = 0.2,
        Angle = -45
    }
};

// Image stamp
var options = new PdfOptions
{
    Stamp = new StampOptions
    {
        ImageData = File.ReadAllBytes("logo.png"),
        Position = StampPosition.BottomRight,
        ImageWidth = 100,
        ImageHeight = 40,
        Opacity = 0.6
    }
};
```

---

## PDF security

```csharp
var options = new PdfOptions
{
    Security = new PdfSecurityOptions
    {
        UserPassword  = "user123",
        OwnerPassword = "admin456",
        Permissions   = PdfPermissions.Print | PdfPermissions.CopyContent
    }
};
```

Available permissions: `Print`, `HighQualityPrint`, `ModifyContent`, `CopyContent`, `Annotate`, `FillForms`, `AssembleDocument`, `All`, `None`.

---

## PDF/A-1b compliance

PDF/A is an ISO archiving standard that requires all resources (fonts, colors) to be self-contained in the file so it renders identically in the future.

```csharp
// Basic — XMP metadata + OutputIntents added automatically
var options = new PdfOptions
{
    Compliance = PdfCompliance.PdfA1B
};

// With embedded ICC color profile (required for strict validator compliance)
options.IccProfileBytes = File.ReadAllBytes("sRGB_IEC61966-2-1.icc");

// Builder
PdfGenerator.Create()
    .WithCompliance(PdfCompliance.PdfA1B)
    .WithIccProfile(File.ReadAllBytes("sRGB_IEC61966-2-1.icc"))
    .GeneratePdf(html);
```

Notes:
- PDF/A cannot be combined with encryption — an `InvalidOperationException` is thrown if `Security` is also set
- Without `IccProfileBytes`, the `OutputIntents` entry references `sRGB IEC61966-2.1` by name only. For strict validation (e.g. veraPDF), supply an actual ICC v2 profile
- PDF/A-1a (tagged PDF with logical structure tree) is not supported — it requires architectural changes to the rendering engine

---

## PDF metadata

```csharp
var options = new PdfOptions
{
    Title    = "Quarterly Report",
    Author   = "Finance Team",
    Subject  = "Q4 2025",
    Keywords = "finance, quarterly, report"
};
```

---

## Progress callback

`OnProgress` fires after each page is rendered with `(currentPage, totalPages)`:

```csharp
PdfGenerator.Create()
    .WithOnProgress((current, total) => Console.WriteLine($"Page {current}/{total}"))
    .GeneratePdf(html);

// Or on PdfOptions directly
options.OnProgress = (current, total) => progressBar.Value = (double)current / total * 100;
```

---

## Error handling

```csharp
// Receive all render errors (fires alongside ILogger if both are set)
options.OnRenderError = err =>
{
    Console.Error.WriteLine($"[{err.ErrorType}] {err.Message}");
    if (err.Exception != null)
        Console.Error.WriteLine(err.Exception);
};

// Swallow rendering exceptions instead of propagating them
options.ContinueOnError = true;

// Builder
PdfGenerator.Create()
    .WithOnRenderError(err => logger.LogWarning("{Type}: {Msg}", err.ErrorType, err.Message))
    .WithContinueOnError()
    .GeneratePdf(html);
```

---

## CancellationToken

All async methods and the synchronous render loop accept a `CancellationToken`. The token is checked before each page, so a cancelled render stops promptly and throws `OperationCanceledException`. `ContinueOnError` does not suppress cancellation.

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

byte[] pdf = await PdfGenerator.GeneratePdfAsync(html, options, cts.Token);

// Builder
byte[] pdf = await PdfGenerator.Create()
    .WithMargin(20)
    .GeneratePdfAsync(html, cts.Token);
```

---

## Generate PDF with result metadata

`GeneratePdfWithResult` returns a `PdfGenerateResult` that wraps the `PdfDocument` together with `PageCount`, `RenderTime`, and any `Warnings` captured during rendering:

```csharp
using var result = PdfGenerator.GeneratePdfWithResult(html, options);

Console.WriteLine($"Pages: {result.PageCount}, Time: {result.RenderTime.TotalMilliseconds}ms");

foreach (var w in result.Warnings)
    Console.WriteLine($"[{w.ErrorType}] {w.Message}");

result.Document.Save("output.pdf");

// Builder
using var result = PdfGenerator.Create()
    .WithMargin(20)
    .GeneratePdfWithResult(html);
```

`PdfGenerateResult` implements `IDisposable` — dispose it when you no longer need the `PdfDocument`.

---

## IRenderingOptions

All `PdfGenerator` methods (except `GeneratePdfFromFile`) accept `IRenderingOptions` instead of a concrete `PdfOptions`. This allows DI-friendly mocking or custom implementations:

```csharp
public class MyRenderingOptions : IRenderingOptions
{
    public XSize PageSize => PageSize.A4;
    public double MarginTop => 20;
    // ... other members
    public XSize GetEffectivePageSize() => PageSize;
}

PdfGenerator.GeneratePdf(html, new MyRenderingOptions());
```

`PdfOptions` implements `IRenderingOptions` directly, so existing code continues to work unchanged.

---

## Non-ASCII characters

Code-page encoding is registered automatically when the library is first used — no setup required:

```csharp
// Romanian, German, French, Czech, etc. all render without any extra configuration
var doc = PdfGenerator.Create()
    .WithPageSize(PageSize.A4)
    .GeneratePdf("<p>ăîșțâ · äöü · éàùèê · čšžřů</p>");
```

---

## ILogger integration

```csharp
// Wire any Microsoft.Extensions.Logging.ILogger
using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var logger = loggerFactory.CreateLogger("HtmlToPdf");

PdfGenerator.Create()
    .WithLogger(logger)
    .GeneratePdf(html);

// Or on PdfOptions
options.Logger = logger;
```

---

## ASP.NET Core / DI

```csharp
// Program.cs
builder.Services.AddHtmlToPdf(opt =>
{
    opt.PageSize = PageSize.A4;
    opt.DefaultMargin = 20;
});

// Controller / service
public class ReportService(IPdfGenerator pdf)
{
    public async Task<byte[]> BuildAsync(string html)
        => await pdf.GeneratePdfAsync(html);
}
```

`IPdfGenerator` exposes the full surface: `GeneratePdf`, `GeneratePdfAsync`, `GeneratePdfWithResult`, `GeneratePdfFromFile`, `GeneratePdfFromFileAsync`, `AppendHtml`, `AppendHtmlAsync`, `AddPdfPages`.

---

## Thread safety

`PdfGenerator` is safe to call concurrently from multiple threads. Each call creates its own `HtmlContainer` and `XGraphics` objects with no shared mutable state. The internal CSS regex cache uses `ConcurrentDictionary`, and font registration uses `ConcurrentDictionary` as well.

```csharp
// Safe — concurrent PDF generation
var tasks = htmlDocuments.Select(html =>
    Task.Run(() => PdfGenerator.GeneratePdf(html, options)));

var documents = await Task.WhenAll(tasks);
```

---

## Supported HTML/CSS

| Feature | Notes |
|---------|-------|
| Inline CSS (`style="..."`) | Full support |
| `<style>` blocks | Full support |
| External CSS (`<link>`) | Via `StyleSheets` dict or `GeneratePdfFromFile` |
| Base64 images (`data:image/...`) | Inline in `src` |
| File images | Via `GeneratePdfFromFile` or `ImageLoad` event |
| Tables — `colspan`, `rowspan`, nested | Full support |
| `thead` repeating across pages | Supported |
| `page-break-before/after` | Supported |
| `text-transform` (upper/lower/capitalize) | Supported |
| `word-spacing` | Supported |
| `border-style: dashed/dotted` | Supported |
| Lists — ordered/unordered, nested | Supported |
| Subscript / superscript | Supported |
| `background-color` on `<body>` | Full-page fill |
| Non-ASCII (Latin, accented, CJK…) | Supported — code-page provider registered automatically |
| PDF/A-1b compliance | Supported — `PdfCompliance.PdfA1B`; supply an ICC v2 profile for strict validation |
| Concurrent / multi-threaded rendering | Supported — each call owns its own objects; CSS regex cache is `ConcurrentDictionary` |
| `letter-spacing`, `border-radius`, SVG | Not yet supported |

---

## License

MIT. HTML/CSS parsing by [HtmlRenderer](https://github.com/ArthurHub/HTML-Renderer) (BSD-3, see [NOTICE.txt](NOTICE.txt)).

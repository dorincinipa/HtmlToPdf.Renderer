# HtmlToPdf.Renderer

[![NuGet](https://img.shields.io/nuget/v/HtmlToPdf.Renderer.svg)](https://www.nuget.org/packages/HtmlToPdf.Renderer)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A .NET 8 library that converts HTML/CSS to PDF using [PdfSharp 6.x](https://github.com/empira/PDFsharp) and [HtmlRenderer](https://github.com/ArthurHub/HTML-Renderer).

Supports inline CSS, tables, images, multi-page pagination, and configurable page sizes (A4, Letter, Legal, A3) with portrait/landscape orientation.

## Usage

### Fluent API

```csharp
using HtmlToPdf.Renderer;

var doc = PdfGenerator.Create()
    .WithPageSize(PageSize.A4)
    .WithMargin(40)
    .GeneratePdf("<h1>Hello</h1><p>World</p>");

doc.Save("output.pdf");
```

### Async (returns byte[])

```csharp
var bytes = await PdfGenerator.Create()
    .WithPageSize(PageSize.A4)
    .WithMargin(40)
    .GeneratePdfAsync(html);

await File.WriteAllBytesAsync("output.pdf", bytes);
```

### Direct API

```csharp
var options = new PdfOptions
{
    PageSize = PageSize.A4,
    PageOrientation = PageOrientation.Landscape,
    MarginTop = 20,
    MarginBottom = 20,
    MarginLeft = 40,
    MarginRight = 40
};

var doc = PdfGenerator.GeneratePdf(html, options);
doc.Save("output.pdf");
```


## License

Licensed under MIT. HTML/CSS parsing by [HtmlRenderer](https://github.com/ArthurHub/HTML-Renderer) (BSD-3, see [NOTICE.txt](NOTICE.txt)).

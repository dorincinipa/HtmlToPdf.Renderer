# HtmlToPdf.Renderer

A .NET 8 library that converts HTML/CSS to PDF using [PdfSharp 6.x](https://github.com/empira/PDFsharp) and [HtmlRenderer](https://github.com/ArthurHub/HTML-Renderer).

```csharp
var config = new PdfGenerateConfig();
config.SetMargins(40);
var pdf = PdfGenerator.GeneratePdf("<h1>Hello</h1><p>World</p>", config);
pdf.Save("output.pdf");
```

Supports inline CSS, tables, images, multi-page pagination, and configurable page sizes (A4, Letter, Legal, A3) with portrait/landscape orientation.

Licensed under MIT. HTML/CSS parsing by [HtmlRenderer](https://github.com/ArthurHub/HTML-Renderer) (BSD-3, see [NOTICE.txt](NOTICE.txt)).

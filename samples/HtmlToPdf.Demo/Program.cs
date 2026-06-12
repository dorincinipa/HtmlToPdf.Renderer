using HtmlToPdf.Renderer;

var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
Directory.CreateDirectory(outputDir);

var html = """
    <div style="font-family: Arial; padding: 10px;">
        <h1 style="color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 8px;">
            HtmlToPdf.Renderer Demo
        </h1>
        <p style="color: #555; line-height: 1.6;">
            This is a <strong>console demo application</strong> that converts HTML to PDF
            using <span style="color: #e74c3c;">HtmlToPdf.Renderer</span>.
        </p>

        <h2 style="color: #2c3e50; margin-top: 24px;">Features</h2>
        <ul style="color: #555; line-height: 1.8;">
            <li>Inline CSS styling (colors, fonts, borders)</li>
            <li>Tables with headers and styled cells</li>
            <li>Nested tables inside cells</li>
            <li>Automatic multi-page pagination</li>
            <li>Configurable page size, orientation, and margins</li>
        </ul>

        <h2 style="color: #2c3e50; margin-top: 24px;">Sample Table</h2>
        <table style="width: 100%; border-collapse: collapse;">
            <tr style="background-color: #3498db; color: white;">
                <th style="padding: 8px; text-align: left;">Item</th>
                <th style="padding: 8px; text-align: left;">Description</th>
                <th style="padding: 8px; text-align: right;">Price</th>
            </tr>
            <tr>
                <td style="padding: 8px; border-bottom: 1px solid #ddd;">Widget A</td>
                <td style="padding: 8px; border-bottom: 1px solid #ddd;">Standard widget</td>
                <td style="padding: 8px; border-bottom: 1px solid #ddd; text-align: right;">$9.99</td>
            </tr>
            <tr style="background-color: #f9f9f9;">
                <td style="padding: 8px; border-bottom: 1px solid #ddd;">Widget B</td>
                <td style="padding: 8px; border-bottom: 1px solid #ddd;">Premium widget</td>
                <td style="padding: 8px; border-bottom: 1px solid #ddd; text-align: right;">$19.99</td>
            </tr>
            <tr>
                <td style="padding: 8px; border-bottom: 1px solid #ddd;">Widget C</td>
                <td style="padding: 8px; border-bottom: 1px solid #ddd;">Deluxe widget</td>
                <td style="padding: 8px; border-bottom: 1px solid #ddd; text-align: right;">$29.99</td>
            </tr>
        </table>

        <h2 style="color: #2c3e50; margin-top: 24px;">Nested Table</h2>
        <p style="color: #555; line-height: 1.6;">Each order cell contains its own line-items table.</p>
        <table style="width: 100%; border-collapse: collapse;">
            <tr style="background-color: #3498db; color: white;">
                <th style="padding: 8px; text-align: left;">Order</th>
                <th style="padding: 8px; text-align: left;">Line Items</th>
            </tr>
            <tr>
                <td style="padding: 8px; border: 1px solid #ddd; vertical-align: top;">#1001</td>
                <td style="padding: 8px; border: 1px solid #ddd;">
                    <table style="width: 100%; border-collapse: collapse;">
                        <tr>
                            <td style="padding: 4px; border: 1px solid #eee;">Widget A</td>
                            <td style="padding: 4px; border: 1px solid #eee; text-align: right;">$9.99</td>
                        </tr>
                        <tr>
                            <td style="padding: 4px; border: 1px solid #eee;">Widget B</td>
                            <td style="padding: 4px; border: 1px solid #eee; text-align: right;">$19.99</td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td style="padding: 8px; border: 1px solid #ddd; vertical-align: top;">#1002</td>
                <td style="padding: 8px; border: 1px solid #ddd;">
                    <table style="width: 100%; border-collapse: collapse;">
                        <tr>
                            <td style="padding: 4px; border: 1px solid #eee;">Widget C</td>
                            <td style="padding: 4px; border: 1px solid #eee; text-align: right;">$29.99</td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
    """;

var pdfPath = Path.Combine(outputDir, "demo.pdf");
var bytes = await PdfGenerator.Create()
    .WithPageSize(PageSize.A4)
    .WithMargin(40)
    .GeneratePdfAsync(html);

await File.WriteAllBytesAsync(pdfPath, bytes);
Console.WriteLine($"Created: {pdfPath}");

Console.WriteLine("\nDone! PDF saved to: " + outputDir);

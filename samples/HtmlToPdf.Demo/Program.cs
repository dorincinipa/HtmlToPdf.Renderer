using HtmlToPdf.Renderer;

var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
Directory.CreateDirectory(outputDir);

var html = """
            <div style="font-family: Arial, sans-serif; color: #333; padding: 10px;">

                <h1 style="color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 8px;">
                    HtmlToPdf.Renderer Feature Showcase
                </h1>

                <p style="line-height: 1.6;">
                    This document demonstrates the <strong>HTML</strong> and <em>CSS</em> features
                    supported by <span style="color: #e74c3c; font-weight: bold;">HtmlToPdf.Renderer</span>.
                    See the <a href="https://github.com/dorincinipa/HtmlToPdf.Renderer">project page</a> for details.
                    Water is H<sub>2</sub>O and 2<sup>10</sup> = 1024. Inline
                    <code style="background:#f4f4f4; padding:2px 4px; font-family:monospace;">code</code>,
                    <del>strikethrough</del> and <u>underline</u> all render.
                </p>

                <h2 style="color: #2c3e50;">Inline formatting &amp; alignment</h2>
                <p style="text-align: left;"><b>Bold</b>, <i>italic</i>, <small>small</small> and
                    <span style="background-color:#fff3cd;">highlighted</span> text. Left aligned.</p>
                <p style="text-align: center; color:#16a085;">Center aligned paragraph.</p>
                <p style="text-align: right; color:#8e44ad;">Right aligned paragraph.</p>
                <p style="text-align: justify;">
                    Justified paragraph: the renderer lays out text across the full content width and
                    wraps automatically, so longer passages of prose distribute evenly between the left
                    and right margins of the page as they flow downward.
                </p>

                <hr />

                <h2 style="color: #2c3e50;">Lists</h2>
                <ul style="line-height: 1.6;">
                    <li>Unordered item one</li>
                    <li>Unordered item two
                        <ul>
                            <li>Nested item (circle bullet)</li>
                            <li>Another nested item</li>
                        </ul>
                    </li>
                    <li>Unordered item three</li>
                </ul>
                <ol style="line-height: 1.6;">
                    <li>Ordered item one</li>
                    <li>Ordered item two</li>
                    <li>Ordered item three</li>
                </ol>

                <blockquote style="color:#555; border-left:4px solid #3498db; padding-left:12px; margin-left:0;">
                    A blockquote demonstrates indented, quoted content set apart from the body text.
                </blockquote>

                <h2 style="color: #2c3e50;">Styled table (header, zebra rows, colspan)</h2>
                <table style="width: 100%; border-collapse: collapse;">
                    <thead>
                        <tr style="background-color: #3498db; color: white;">
                            <th style="padding: 8px; text-align: left;">Item</th>
                            <th style="padding: 8px; text-align: left;">Description</th>
                            <th style="padding: 8px; text-align: right;">Price</th>
                        </tr>
                    </thead>
                    <tbody>
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
                        <tr style="background-color: #ecf0f1; font-weight: bold;">
                            <td colspan="2" style="padding: 8px; text-align: right;">Total</td>
                            <td style="padding: 8px; text-align: right;">$59.97</td>
                        </tr>
                    </tbody>
                </table>

                <h2 style="color: #2c3e50;">Nested tables</h2>
                <p>Each order cell contains its own line-items table.</p>
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

                <div style="page-break-before: always;"></div>

                <h2 style="color: #2c3e50;">Pagination</h2>
                <p style="line-height: 1.6;">
                    This section begins on a new page via
                    <code style="font-family:monospace;">page-break-before: always</code>.
                    The footer shows <strong>page X / Y</strong>, and content longer than one page
                    flows across additional pages automatically.
                </p>

                <h2 style="color: #2c3e50;">Image (base64 data URI)</h2>
                <p>Images embedded as base64 data URIs render inline (shown here scaled with a border):</p>
                <img src="data:image/png;base64,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"
                     width="280" height="140" style="border:1px solid #bbb;" />

                <h2 style="color: #2c3e50;">Preformatted text</h2>
                <pre style="background:#f4f4f4; border:1px solid #e1e1e1; padding:10px;">var bytes = await PdfGenerator.Create()
                    .WithPageSize(PageSize.A4)
                    .WithMargin(40)
                    .GeneratePdfAsync(html);</pre>

                <p style="color:#888; text-align:center; margin-top:24px;">- End of showcase -</p>
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

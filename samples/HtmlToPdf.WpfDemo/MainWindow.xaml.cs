using System.Diagnostics;
using System.Windows;
using HtmlToPdf.Renderer;
using Microsoft.Win32;

namespace HtmlToPdf.WpfDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadSampleHtml();
    }

    private void LoadSampleBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadSampleHtml();
    }

    private void GenerateBtn_Click(object sender, RoutedEventArgs e)
    {
        var html = HtmlEditor.Text;
        if (string.IsNullOrWhiteSpace(html))
        {
            StatusText.Text = "Please enter some HTML first.";
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            FileName = "output.pdf"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var config = BuildConfig();
            var document = PdfGenerator.GeneratePdf(html, config);
            document.Save(dialog.FileName);

            StatusText.Text = $"Saved to {dialog.FileName}";

            // Open the PDF
            Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private PdfOptions BuildConfig()
    {
        var config = new PdfOptions();

        config.PageSize = (PageSizeCombo.SelectedIndex) switch
        {
            0 => PageSize.A4,
            1 => PageSize.Letter,
            2 => PageSize.Legal,
            3 => PageSize.A3,
            _ => PageSize.A4
        };

        config.PageOrientation = OrientationCombo.SelectedIndex == 0
            ? PageOrientation.Portrait
            : PageOrientation.Landscape;

        if (double.TryParse(MarginBox.Text, out var margin))
            config.SetMargins(margin);

        config.Header = "<div style='font-size:9px; color:#888;'>Sample</div>";
        config.Footer = "<div style='font-size:9px; color:#888; text-align:right;'>{{PageNumber}} / {{TotalPages}}</div>";

        // do not use hardocoded passwords
        config.Security = new PdfSecurityOptions {
            UserPassword = "user",
            OwnerPassword = "owner",
            Permissions = PdfPermissions.None
        };

        return config;
    }

    private void LoadSampleHtml()
    {
        HtmlEditor.Text = """
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
                <img src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg=="
                     width="160" height="40" style="border:1px solid #bbb;" />

                <h2 style="color: #2c3e50;">Preformatted text</h2>
                <pre style="background:#f4f4f4; border:1px solid #e1e1e1; padding:10px;">var bytes = await PdfGenerator.Create()
                    .WithPageSize(PageSize.A4)
                    .WithMargin(40)
                    .GeneratePdfAsync(html);</pre>

                <p style="color:#888; text-align:center; margin-top:24px;">- End of showcase -</p>
            </div>
            """;
    }
}

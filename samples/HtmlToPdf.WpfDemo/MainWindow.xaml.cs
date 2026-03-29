using System.Diagnostics;
using System.Windows;
using HtmlToPdf.Renderer;
using Microsoft.Win32;
using PdfSharp.Drawing;

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

    private PdfGenerateConfig BuildConfig()
    {
        var config = new PdfGenerateConfig();

        config.PageSize = (PageSizeCombo.SelectedIndex) switch
        {
            0 => PdfGenerateConfig.PageSizes.A4,
            1 => PdfGenerateConfig.PageSizes.Letter,
            2 => PdfGenerateConfig.PageSizes.Legal,
            3 => PdfGenerateConfig.PageSizes.A3,
            _ => PdfGenerateConfig.PageSizes.A4
        };

        config.PageOrientation = OrientationCombo.SelectedIndex == 0
            ? PageOrientation.Portrait
            : PageOrientation.Landscape;

        if (double.TryParse(MarginBox.Text, out var margin))
            config.SetMargins(margin);

        return config;
    }

    private void LoadSampleHtml()
    {
        HtmlEditor.Text = """
            <div style="font-family: Arial; padding: 10px;">
                <h1 style="color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 8px;">
                    HtmlToPdf.Renderer Demo
                </h1>
                <p style="color: #555; line-height: 1.6;">
                    This is a <strong>WPF demo application</strong> that converts HTML to PDF
                    using <span style="color: #e74c3c;">HtmlToPdf.Renderer</span>.
                    Edit the HTML below and click <em>Generate PDF</em> to try it out.
                </p>

                <h2 style="color: #2c3e50; margin-top: 24px;">Features</h2>
                <ul style="color: #555; line-height: 1.8;">
                    <li>Inline CSS styling (colors, fonts, borders)</li>
                    <li>Tables with headers and styled cells</li>
                    <li>Automatic multi-page pagination</li>
                    <li>Configurable page size, orientation, and margins</li>
                </ul>

                <h2 style="color: #2c3e50; margin-top: 24px;">Sample Table</h2>
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
                    </tbody>
                </table>
            </div>
            """;
    }
}

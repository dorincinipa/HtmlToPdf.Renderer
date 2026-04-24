using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;

namespace HtmlToPdf.Renderer.Tests;

public class PdfSecurityTests
{
    [Fact]
    public void GeneratePdf_SecurityWithoutAnyPassword_Throws()
    {
        var options = new PdfOptions
        {
            Security = new PdfSecurityOptions
            {
                Permissions = PdfPermissions.ReadOnly
            }
        };

        var ex = Assert.Throws<InvalidOperationException>(
            () => PdfGenerator.GeneratePdf("<p>x</p>", options));

        Assert.Contains("UserPassword", ex.Message);
        Assert.Contains("OwnerPassword", ex.Message);
    }
}

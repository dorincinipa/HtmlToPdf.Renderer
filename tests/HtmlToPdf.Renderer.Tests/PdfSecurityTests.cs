using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

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

    [Fact]
    public void GeneratePdf_NullSecurity_ProducesUnencryptedDocument()
    {
        var options = new PdfOptions { Security = null };

        using var doc = PdfGenerator.GeneratePdf("<p>plain</p>", options);

        Assert.False(doc.SecuritySettings.IsEncrypted);
    }

    [Fact]
    public void GeneratePdf_UserPasswordOnly_EncryptsAndGatesReopen()
    {
        var options = new PdfOptions
        {
            Security = new PdfSecurityOptions { UserPassword = "secret" }
        };

        using var doc = PdfGenerator.GeneratePdf("<p>locked</p>", options);
        using var stream = new MemoryStream();
        doc.Save(stream, false);

        stream.Position = 0;
        using (var reopened = PdfReader.Open(stream, "secret", PdfDocumentOpenMode.Modify))
        {
            Assert.True(reopened.PageCount >= 1);
        }

        stream.Position = 0;
        Assert.ThrowsAny<Exception>(() =>
            PdfReader.Open(stream, PdfDocumentOpenMode.Modify));
    }

    [Fact]
    public void GeneratePdf_OwnerPasswordOnly_EncryptsDocument()
    {
        var options = new PdfOptions
        {
            Security = new PdfSecurityOptions { OwnerPassword = "owner" }
        };

        using var doc = PdfGenerator.GeneratePdf("<p>owner-protected</p>", options);

        Assert.True(doc.SecuritySettings.IsEncrypted);

        using var stream = new MemoryStream();
        doc.Save(stream, false);

        stream.Position = 0;
        using var reopened = PdfReader.Open(stream, "owner", PdfDocumentOpenMode.Modify);
        Assert.True(reopened.PageCount >= 1);
    }

    [Fact]
    public void GeneratePdf_BothPasswords_UserOpensOwnerOpens()
    {
        var options = new PdfOptions
        {
            Security = new PdfSecurityOptions
            {
                UserPassword = "user",
                OwnerPassword = "owner"
            }
        };

        using var doc = PdfGenerator.GeneratePdf("<p>both</p>", options);
        using var stream = new MemoryStream();
        doc.Save(stream, false);

        // User password grants read-only (Import) access; Modify requires owner.
        stream.Position = 0;
        using (var r1 = PdfReader.Open(stream, "user", PdfDocumentOpenMode.Import))
            Assert.True(r1.PageCount >= 1);

        stream.Position = 0;
        using (var r2 = PdfReader.Open(stream, "owner", PdfDocumentOpenMode.Modify))
            Assert.True(r2.PageCount >= 1);

        stream.Position = 0;
        Assert.ThrowsAny<Exception>(() =>
            PdfReader.Open(stream, "wrong", PdfDocumentOpenMode.Modify));
    }

    [Fact]
    public void GeneratePdf_PermissionsReadOnly_MapsToExpectedPermitBools()
    {
        var options = new PdfOptions
        {
            Security = new PdfSecurityOptions
            {
                OwnerPassword = "owner",
                Permissions = PdfPermissions.ReadOnly
            }
        };

        using var doc = PdfGenerator.GeneratePdf("<p>readonly</p>", options);
        var s = doc.SecuritySettings;

        Assert.True(s.PermitPrint);
        Assert.True(s.PermitFullQualityPrint);
        Assert.True(s.PermitExtractContent);

        Assert.False(s.PermitModifyDocument);
        Assert.False(s.PermitAnnotations);
        Assert.False(s.PermitFormsFill);
        Assert.False(s.PermitAssembleDocument);
    }

    [Fact]
    public void GeneratePdf_PermissionsAll_EveryPermitTrue()
    {
        var options = new PdfOptions
        {
            Security = new PdfSecurityOptions
            {
                OwnerPassword = "owner",
                Permissions = PdfPermissions.All
            }
        };

        using var doc = PdfGenerator.GeneratePdf("<p>all</p>", options);
        var s = doc.SecuritySettings;

        Assert.True(s.PermitPrint);
        Assert.True(s.PermitFullQualityPrint);
        Assert.True(s.PermitModifyDocument);
        Assert.True(s.PermitExtractContent);
        Assert.True(s.PermitAnnotations);
        Assert.True(s.PermitFormsFill);
        Assert.True(s.PermitAssembleDocument);
    }

    [Fact]
    public void Fluent_WithPassword_ProducesEncryptedDocument()
    {
        using var doc = PdfGenerator.Create()
            .WithPassword("pw")
            .GeneratePdf("<p>fluent-pw</p>");

        Assert.True(doc.SecuritySettings.IsEncrypted);
    }

    [Fact]
    public void Fluent_WithOwnerPassword_ProducesEncryptedDocument()
    {
        using var doc = PdfGenerator.Create()
            .WithOwnerPassword("admin")
            .GeneratePdf("<p>fluent-owner</p>");

        Assert.True(doc.SecuritySettings.IsEncrypted);
    }

    [Fact]
    public void Fluent_WithPermissions_AppliesToDocument()
    {
        using var doc = PdfGenerator.Create()
            .WithOwnerPassword("admin")
            .WithPermissions(PdfPermissions.ReadOnly)
            .GeneratePdf("<p>fluent-perm</p>");

        Assert.True(doc.SecuritySettings.PermitPrint);
        Assert.False(doc.SecuritySettings.PermitModifyDocument);
    }

    [Fact]
    public void Fluent_FullChain_AllValuesLandOnSameInstance()
    {
        using var doc = PdfGenerator.Create()
            .WithPassword("user")
            .WithOwnerPassword("owner")
            .WithPermissions(PdfPermissions.ReadOnly)
            .GeneratePdf("<p>chain</p>");

        using var stream = new MemoryStream();
        doc.Save(stream, false);

        // User password survived subsequent With* calls and grants read access.
        stream.Position = 0;
        using (var r = PdfReader.Open(stream, "user", PdfDocumentOpenMode.Import))
            Assert.True(r.PageCount >= 1);

        Assert.True(doc.SecuritySettings.PermitPrint);
        Assert.False(doc.SecuritySettings.PermitModifyDocument);
    }

    [Fact]
    public void GeneratePdf_PermissionsNone_EveryPermitFalse()
    {
        var options = new PdfOptions
        {
            Security = new PdfSecurityOptions
            {
                OwnerPassword = "owner",
                Permissions = PdfPermissions.None
            }
        };

        using var doc = PdfGenerator.GeneratePdf("<p>none</p>", options);
        var s = doc.SecuritySettings;

        Assert.False(s.PermitPrint);
        Assert.False(s.PermitFullQualityPrint);
        Assert.False(s.PermitModifyDocument);
        Assert.False(s.PermitExtractContent);
        Assert.False(s.PermitAnnotations);
        Assert.False(s.PermitFormsFill);
        Assert.False(s.PermitAssembleDocument);
    }
}

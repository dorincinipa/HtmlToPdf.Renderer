using HtmlToPdf.Renderer;
using HtmlToPdf.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHtmlToPdf(options =>
{
    options.PageSize = PageSize.A4;
    options.DefaultMargin = 20;
});

var app = builder.Build();

// POST raw HTML body to PDF
app.MapPost("/api/pdf", async (IPdfGenerator pdf, HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var html = await reader.ReadToEndAsync();
    var bytes = await pdf.GeneratePdfAsync(html);
    return Results.File(bytes, "application/pdf", "output.pdf");
});

// POST file upload to PDF
app.MapPost("/api/pdf/upload", async (IPdfGenerator pdf, HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files.FirstOrDefault();
    if (file is null || file.Length == 0)
        return Results.BadRequest("No file uploaded.");

    using var reader = new StreamReader(file.OpenReadStream());
    var html = await reader.ReadToEndAsync();
    var bytes = await pdf.GeneratePdfAsync(html);
    return Results.File(bytes, "application/pdf", "output.pdf");
}).DisableAntiforgery();

await app.RunAsync();

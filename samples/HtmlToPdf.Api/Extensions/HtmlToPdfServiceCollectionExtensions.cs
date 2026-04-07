using HtmlToPdf.Renderer;
using Microsoft.Extensions.DependencyInjection;

namespace HtmlToPdf.Api.Extensions;

public static class HtmlToPdfServiceCollectionExtensions
{
    public static IServiceCollection AddHtmlToPdf(
        this IServiceCollection services,
        Action<PdfOptions>? configure = null)
    {
        var options = new PdfOptions();
        configure?.Invoke(options);

        if (options.DefaultMargin > 0)
            options.SetMargins(options.DefaultMargin);

        if (!string.IsNullOrEmpty(options.FontFolder))
            PdfGenerator.LoadFontsFromFolder(options.FontFolder);

        services.AddSingleton(options);
        services.AddSingleton<IPdfGenerator>(new HtmlToPdfGenerator(options));

        return services;
    }
}

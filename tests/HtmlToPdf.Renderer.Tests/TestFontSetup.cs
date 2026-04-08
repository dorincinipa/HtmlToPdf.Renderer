using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HtmlToPdf.Renderer.Adapters;
using PdfSharp.Fonts;

namespace HtmlToPdf.Renderer.Tests;

internal static class TestFontSetup
{
    [ModuleInitializer]
    internal static void Init()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
        else
            FontResolver.Instance.EnsureRegistered();
    }
}

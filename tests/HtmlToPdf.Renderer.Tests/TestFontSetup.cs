// tests/HtmlToPdf.Renderer.Tests/TestFontSetup.cs
using System.Runtime.CompilerServices;
using PdfSharp.Fonts;

namespace HtmlToPdf.Renderer.Tests;

internal static class TestFontSetup
{
    [ModuleInitializer]
    internal static void Init()
    {
        // PdfSharp 6.2.x cross-platform build needs font resolver configured
        // before any XFont can be created.
        // Only effective for Core build on Windows; no-op otherwise.
        GlobalFontSettings.UseWindowsFontsUnderWindows = true;
    }
}

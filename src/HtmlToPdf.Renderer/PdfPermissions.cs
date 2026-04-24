namespace HtmlToPdf.Renderer;

[Flags]
public enum PdfPermissions
{
    None = 0,
    Print = 1 << 0,
    HighQualityPrint = 1 << 1,
    ModifyContent = 1 << 2,
    CopyContent = 1 << 3,
    Annotate = 1 << 4,
    FillForms = 1 << 5,
    AssembleDocument = 1 << 6,

    All = Print | HighQualityPrint | ModifyContent | CopyContent |
          Annotate | FillForms | AssembleDocument,

    ReadOnly = Print | HighQualityPrint | CopyContent
}

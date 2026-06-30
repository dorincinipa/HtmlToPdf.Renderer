namespace HtmlToPdf.Renderer;

public sealed class PdfRenderError
{
    public string ErrorType { get; }
    public string Message { get; }
    public Exception? Exception { get; }

    internal PdfRenderError(string errorType, string message, Exception? exception)
    {
        ErrorType = errorType;
        Message = message;
        Exception = exception;
    }
}


namespace Ulfbou.Portfolio.Core;

public sealed class ContentValidationException : Exception
{
    public ContentValidationException(
        string code,
        string sourcePath,
        string reason)
        : base($"{code}: {sourcePath}: {reason}")
    {
        Code = code;
        SourcePath = sourcePath;
        Reason = reason;
    }

    public string Code { get; }

    public string SourcePath { get; }

    public string Reason { get; }
}

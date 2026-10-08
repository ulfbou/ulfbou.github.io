
using Ulfbou.Portfolio.Core;

if (args.Length != 2)
{
    Console.Error.WriteLine(
        "Usage: Ulfbou.Portfolio.Generator <content-directory> <output-file>");
    return 2;
}

try
{
    var contentDirectory = Path.GetFullPath(args[0]);
    var outputFile = Path.GetFullPath(args[1]);

    var loader = new ApprovedContentLoader();
    var compiler = new CompiledPortfolioGenerator();

    var first = compiler.CompileBytes(
        loader.Load(contentDirectory));

    var second = compiler.CompileBytes(
        loader.Load(contentDirectory));

    if (!first.AsSpan().SequenceEqual(second))
    {
        throw new InvalidOperationException(
            "Deterministic recompilation failed.");
    }

    var outputDirectory = Path.GetDirectoryName(outputFile)
        ?? throw new InvalidOperationException(
            "Output file has no parent directory.");

    Directory.CreateDirectory(outputDirectory);

    var temporaryFile = Path.Combine(
        outputDirectory,
        Path.GetFileName(outputFile)
        + ".tmp."
        + Guid.NewGuid().ToString("N"));

    try
    {
        File.WriteAllBytes(temporaryFile, first);

        if (
            !File.ReadAllBytes(temporaryFile)
                .AsSpan()
                .SequenceEqual(first))
        {
            throw new IOException(
                "Temporary-file verification failed.");
        }

        File.Move(
            temporaryFile,
            outputFile,
            overwrite: true);
    }
    finally
    {
        if (File.Exists(temporaryFile))
        {
            File.Delete(temporaryFile);
        }
    }

    return 0;
}
catch (ContentValidationException exception)
{
    Console.Error.WriteLine(
        $"{exception.Code}: {exception.SourcePath}: {exception.Reason}");
    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

namespace AlvorKit;

/// <summary>Restores both process console streams after nonparallel output tests.</summary>
internal class BenchOutputCapture : IDisposable
{
    private readonly TextWriter originalOutput = Console.Out;
    private readonly TextWriter originalError = Console.Error;
    private readonly StringWriter output = new();
    private readonly StringWriter error = new();

    internal string Text => output.ToString();
    internal string Error => error.ToString();

    internal BenchOutputCapture()
    {
        Console.SetOut(output);
        Console.SetError(error);
    }

    public void Dispose()
    {
        Console.SetOut(originalOutput);
        Console.SetError(originalError);
        output.Dispose();
        error.Dispose();
    }
}

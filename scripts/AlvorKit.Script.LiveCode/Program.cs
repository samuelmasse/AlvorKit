namespace AlvorKit;

/// <summary>Entry point for discovering targets and submitting compiled C# LiveCode commands.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Runs the LiveCode command line.</summary>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var context = new LiveCodeCliContext(
                Console.In,
                Console.Out,
                Directory.GetCurrentDirectory());
            var command = LiveCodeCommandTree.Create(
                new(context),
                new(context),
                new(Console.Out, Directory.GetCurrentDirectory()));
            return await command.Parse(args).InvokeAsync(
                new() { EnableDefaultExceptionHandler = false });
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}

namespace AlvorKit.Script.NewGame;

/// <summary>Entry point for creating a sibling AlvorKit game repository.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Creates the game repository and returns a process exit code.</summary>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var command = NewGameOptions.CreateRootCommand(
                () => ProjectRoot.FindFromCurrentProcess(typeof(NewGameOptions)),
                RunAsync);
            return await command.Parse(args).InvokeAsync(new() { EnableDefaultExceptionHandler = false });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    /// <summary>Executes repository generation after command-line parsing succeeds.</summary>
    private static Task<int> RunAsync(NewGameOptions options)
    {
        var result = new NewGameGenerator().Generate(options);
        Console.WriteLine($"Created {result.OutputPath} ({result.FileCount} files).");
        return Task.FromResult(0);
    }
}

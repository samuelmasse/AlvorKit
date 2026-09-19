namespace AlvorKit;

/// <summary>Runs repository solution generation without requiring a solution to bootstrap the tool.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Routes one-shot generation, validation, discovery, and continuous reconciliation.</summary>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            return await SolutionOptions.Command(RunAsync).Parse(args)
                .InvokeAsync(new() { EnableDefaultExceptionHandler = false });
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    /// <summary>Executes a validated command, keeping all generation on one thread.</summary>
    private static async Task<int> RunAsync(SolutionOptions options)
    {
        if (options.ListOnly)
        {
            foreach (var root in options.DiscoverRepositories())
                Console.WriteLine(root);

            return 0;
        }

        if (options.Watch)
        {
            using var cancellation = new CancellationTokenSource();
            Console.CancelKeyPress += (_, args) =>
            {
                args.Cancel = true;
                cancellation.Cancel();
            };
            using var watcher = new SolutionWatcher(options);
            await watcher.RunAsync(cancellation.Token);
            return 0;
        }

        var success = true;

        foreach (var root in options.DiscoverRepositories())
            success &= SolutionGenerator.Generate(root, options.Check);

        return success ? 0 : 1;
    }
}

namespace AlvorKit.Script.ZoneSolution;

/// <summary>Entry point for the sibling-repository zone solution generator.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Generates an AlvorZone solution and returns a process exit code.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var command = ZoneSolutionOptions.CreateRootCommand(ZoneSolutionDefaults.FindZoneRoot, RunAsync);
            return await command.Parse(args).InvokeAsync(new() { EnableDefaultExceptionHandler = false });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    /// <summary>Runs the generator or lists the repositories that would be included.</summary>
    private static Task<int> RunAsync(ZoneSolutionOptions options)
    {
        if (options.ListOnly)
        {
            foreach (var repository in ZoneRepositoryDiscovery.Discover(options))
            {
                var solutionPath = PathText.ToSlnxPath(Path.GetRelativePath(options.ZoneRoot, repository.SolutionPath));
                Console.WriteLine($"{repository.Name}: {solutionPath} -> /{repository.Name}/");
            }

            Console.WriteLine($"Output: {options.OutputPath}");
            return Task.FromResult(0);
        }

        var result = new ZoneSolutionGenerator().Generate(options);
        var verb = result.Changed ? "Wrote" : "Unchanged";
        Console.WriteLine($"{verb} {result.OutputPath} ({result.RepositoryCount} repositories, {result.ProjectCount} projects).");
        foreach (var repository in result.Repositories)
            Console.WriteLine($"  {repository.Name}: {repository.ProjectCount} projects");

        return Task.FromResult(0);
    }
}

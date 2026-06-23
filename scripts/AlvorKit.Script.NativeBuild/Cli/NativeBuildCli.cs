namespace AlvorKit.Script.NativeBuild;

/// <summary>Coordinates parsed command-line requests with repository metadata and build services.</summary>
[ExcludeFromCodeCoverage]
internal sealed class NativeBuildCli
{
    /// <summary>Runs the requested command and converts handled exceptions into exit code 1.</summary>
    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            var root = CliParser.CreateRootCommand(RunParsedAsync);
            return await root.Parse(args).InvokeAsync(new() { EnableDefaultExceptionHandler = false });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("error: " + ex.Message);
            return 1;
        }
    }

    /// <summary>Runs a parsed request after the repository root has been found.</summary>
    private static async Task<int> RunParsedAsync(CliRequest request)
    {
        var repository = RepositoryLayout.FindFrom(AppContext.BaseDirectory);
        switch (request.Command)
        {
            case CliCommand.List:
                foreach (var library in repository.NativeBuildLibraries())
                    Console.WriteLine(library);
                return 0;

            case CliCommand.Version:
                Console.WriteLine(LibraryBuildContext.Load(repository, request.Selection!).NativeVersion);
                return 0;

            case CliCommand.Build:
                var target = request.Rid is null ? TargetRid.Current() : TargetRid.Parse(request.Rid);
                foreach (var library in SelectedLibraries(repository, request.Selection!))
                    await new NativeBuildRunner(LibraryBuildContext.Load(repository, library), target).BuildAsync();
                return 0;

            case CliCommand.Verify:
                await new NativeVerifyRunner(
                    LibraryBuildContext.Load(repository, request.Selection!),
                    TargetRid.Parse(request.Rid!)).VerifyAsync();
                return 0;

            default:
                throw new InvalidOperationException($"Unknown command '{request.Command}'.");
        }
    }

    /// <summary>Expands the special all selection into the configured native package names.</summary>
    private static IEnumerable<string> SelectedLibraries(RepositoryLayout repository, string selection) =>
        selection == "all" ? repository.NativeBuildLibraries() : [selection];
}

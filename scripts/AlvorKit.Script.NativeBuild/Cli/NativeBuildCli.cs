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
            var request = CliParser.Parse(args);
            if (request.ShowHelp)
            {
                PrintUsage();
                return 0;
            }

            var repository = RepositoryLayout.FindFrom(AppContext.BaseDirectory);
            return await RunParsedAsync(repository, request);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("error: " + ex.Message);
            return 1;
        }
    }

    /// <summary>Runs a parsed request after the repository root has been found.</summary>
    private static async Task<int> RunParsedAsync(RepositoryLayout repository, CliRequest request)
    {
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

            default:
                throw new InvalidOperationException($"Unknown command '{request.Command}'.");
        }
    }

    /// <summary>Expands the special all selection into the configured native package names.</summary>
    private static IEnumerable<string> SelectedLibraries(RepositoryLayout repository, string selection) =>
        selection == "all" ? repository.NativeBuildLibraries() : [selection];

    /// <summary>Writes the supported command syntax to standard output.</summary>
    private static void PrintUsage()
    {
        Console.WriteLine("""
            AlvorKit native build runner

            Usage:
              dotnet run --project scripts/AlvorKit.Script.NativeBuild -- list
              dotnet run --project scripts/AlvorKit.Script.NativeBuild -- version <library>
              dotnet run --project scripts/AlvorKit.Script.NativeBuild -- build <library|all> [--rid <rid>]

            RIDs:
              win-x64, win-x86, win-arm64
              linux-x64, linux-arm64, linux-arm
              osx-x64, osx-arm64
            """);
    }
}

namespace AlvorKit.Script.ZoneSolution;

/// <summary>Command-line options for generating an AlvorZone solution.</summary>
/// <param name="ZoneRoot">Directory that contains sibling repositories.</param>
/// <param name="OutputPath">Generated zone solution path.</param>
/// <param name="RepoNames">Optional repository folder names to include.</param>
/// <param name="ListOnly">True when the command should only print discovered repositories.</param>
internal sealed record ZoneSolutionOptions(
    string ZoneRoot,
    string OutputPath,
    IReadOnlyList<string> RepoNames,
    bool ListOnly)
{
    /// <summary>Creates the command-line surface for the generator.</summary>
    /// <param name="defaultZoneRoot">Zone root provider used when <c>--zone-root</c> is omitted.</param>
    /// <param name="execute">Action that executes the generator with parsed options.</param>
    internal static RootCommand CreateRootCommand(Func<string> defaultZoneRoot, Func<ZoneSolutionOptions, Task<int>> execute)
    {
        var zoneRoot = new Option<string?>("--zone-root") { Description = "Sibling repository root. Defaults to the parent of the AlvorKit checkout." };
        var output = new Option<string?>("--output") { Description = "Generated .slnx output path. Defaults to AlvorZone.slnx under the zone root." };
        var repo = new Option<string[]>("--repo") { Description = "Repository folder name to include. May be repeated." };
        var listOnly = new Option<bool>("--list-only") { Description = "List discovered repositories without writing the solution." };
        var command = new RootCommand("Generate one .slnx containing AlvorKit and sibling repositories.");
        command.Options.Add(zoneRoot);
        command.Options.Add(output);
        command.Options.Add(repo);
        command.Options.Add(listOnly);
        command.SetAction(parse => execute(Options(parse, defaultZoneRoot, zoneRoot, output, repo, listOnly)));
        return command;
    }

    /// <summary>Parses command-line arguments into validated generator options.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public static ZoneSolutionOptions Parse(IReadOnlyList<string> args) =>
        Parse(args, ZoneSolutionDefaults.FindZoneRoot);

    /// <summary>Parses command-line arguments using explicit defaults for tests.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    /// <param name="defaultZoneRoot">Zone root provider used when <c>--zone-root</c> is omitted.</param>
    internal static ZoneSolutionOptions Parse(IReadOnlyList<string> args, Func<string> defaultZoneRoot)
    {
        var zoneRoot = new Option<string?>("--zone-root") { Description = "Sibling repository root. Defaults to the parent of the AlvorKit checkout." };
        var output = new Option<string?>("--output") { Description = "Generated .slnx output path. Defaults to AlvorZone.slnx under the zone root." };
        var repo = new Option<string[]>("--repo") { Description = "Repository folder name to include. May be repeated." };
        var listOnly = new Option<bool>("--list-only") { Description = "List discovered repositories without writing the solution." };
        var command = new RootCommand("Generate one .slnx containing AlvorKit and sibling repositories.");
        command.Options.Add(zoneRoot);
        command.Options.Add(output);
        command.Options.Add(repo);
        command.Options.Add(listOnly);
        var result = command.Parse(args.ToArray());
        ThrowIfErrors(result);

        return Options(result, defaultZoneRoot, zoneRoot, output, repo, listOnly);
    }

    /// <summary>Creates generator options from parsed command-line values.</summary>
    private static ZoneSolutionOptions Options(
        ParseResult parse,
        Func<string> defaultZoneRoot,
        Option<string?> zoneRoot,
        Option<string?> output,
        Option<string[]> repo,
        Option<bool> listOnly)
    {
        var zoneRootPath = Path.GetFullPath(parse.GetValue(zoneRoot) ?? defaultZoneRoot());
        var outputPath = Path.GetFullPath(parse.GetValue(output) ?? DefaultOutputPath(zoneRootPath));
        var repoNames = ValidateRepoNames(parse.GetValue(repo) ?? []);

        return new(zoneRootPath, outputPath, repoNames, parse.GetValue(listOnly));
    }

    /// <summary>Returns the default generated solution path for a zone root.</summary>
    private static string DefaultOutputPath(string zoneRoot) =>
        Path.Combine(zoneRoot, "AlvorZone.slnx");

    /// <summary>Rejects blank or path-shaped repository filters.</summary>
    private static IReadOnlyList<string> ValidateRepoNames(IReadOnlyList<string> names)
    {
        var repoNames = new List<string>();
        foreach (var rawName in names)
        {
            var name = rawName.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Repository name must not be blank.", nameof(names));
            if (Path.GetFileName(name) != name)
                throw new ArgumentException("Repository name must be a folder name, not a path.", nameof(names));
            if (name is "." or "..")
                throw new ArgumentException("Repository name must not be a traversal marker.", nameof(names));

            if (!repoNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                repoNames.Add(name);
        }

        return repoNames;
    }

    /// <summary>Throws an argument exception when System.CommandLine found parse errors.</summary>
    private static void ThrowIfErrors(ParseResult result)
    {
        if (result.Action is System.CommandLine.Help.HelpAction)
            throw new ArgumentException("Help is generated by the command-line app.");
        if (result.Errors.Count > 0)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(error => error.Message)));
    }
}

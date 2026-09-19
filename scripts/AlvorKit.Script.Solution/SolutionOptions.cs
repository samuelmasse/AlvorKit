namespace AlvorKit;

/// <summary>Chooses explicit repositories or sibling discovery and one-shot or continuous generation.</summary>
internal record SolutionOptions(IReadOnlyList<string> RepositoryRoots, string? ParentDirectory, bool Watch, bool Check, bool ListOnly)
{
    /// <summary>Creates the public command surface shared by local development and CI.</summary>
    public static RootCommand Command(Func<SolutionOptions, Task<int>> execute)
    {
        var roots = new Option<string[]>("--repo-root")
        {
            Description = "Repository roots. Defaults to the current Git checkout.",
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
        };
        var parent = new Option<string?>("--parent-directory")
        {
            Description = "Discover immediate sibling Git repositories under this directory.",
        };
        var watch = new Option<bool>("--watch") { Description = "Keep each repository solution synchronized." };
        var check = new Option<bool>("--check") { Description = "Fail if solutions differ from their project graphs; write nothing." };
        var list = new Option<bool>("--list-only") { Description = "List discovered repositories without evaluation or writes." };
        var command = new RootCommand("Generate one ignored .slnx inside each repository from its projects and dependencies.");
        command.Options.Add(roots);
        command.Options.Add(parent);
        command.Options.Add(watch);
        command.Options.Add(check);
        command.Options.Add(list);
        command.SetAction(parse => execute(Create(
            parse.GetValue(roots) ?? [], parse.GetValue(parent), parse.GetValue(watch),
            parse.GetValue(check), parse.GetValue(list))));
        return command;
    }

    /// <summary>Validates mutually exclusive operating modes and resolves paths once.</summary>
    internal static SolutionOptions Create(string[] roots, string? parent, bool watch, bool check, bool list)
    {
        if (roots.Length > 0 && parent is not null)
            throw new ArgumentException("Choose --repo-root or --parent-directory, not both.");

        if ((watch && check) || (list && (watch || check)))
            throw new ArgumentException("--watch, --check, and --list-only are separate operating modes.");

        if (roots.Length == 0 && parent is null)
            roots = [RepositoryRoot.FindFrom(Environment.CurrentDirectory)];

        return new(roots.Select(Path.GetFullPath).Distinct(SolutionPaths.Comparer).ToArray(),
            parent is null ? null : Path.GetFullPath(parent), watch, check, list);
    }

    /// <summary>Rediscovers sibling repositories on every reconciliation so additions and removals take effect.</summary>
    public IReadOnlyList<string> DiscoverRepositories()
    {
        if (ParentDirectory is null)
        {
            foreach (var root in RepositoryRoots)
            {
                if (!Directory.Exists(root))
                    throw new DirectoryNotFoundException($"Repository root does not exist: {root}");
            }

            return RepositoryRoots;
        }

        return Directory.EnumerateDirectories(ParentDirectory)
            .Where(root => File.Exists(Path.Combine(root, ".git")) || Directory.Exists(Path.Combine(root, ".git")))
            .Where(root => RepositoryProjects.Discover(root).Count > 0 || SolutionGenerator.HasGeneratedSolution(root))
            .Order(StringComparer.Ordinal).ToArray();
    }
}

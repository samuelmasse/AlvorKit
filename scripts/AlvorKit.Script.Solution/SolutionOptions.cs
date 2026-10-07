namespace AlvorKit;

/// <summary>Chooses explicit repositories or sibling discovery and one-shot or continuous generation.</summary>
internal record SolutionOptions(IReadOnlyList<string> RepositoryRoots, string? ParentDirectory,
    bool Watch, bool Check, bool ListOnly, string? AggregateSolution)
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
        var aggregate = new Option<string?>("--aggregate-solution")
        {
            Description = "Also generate a combined .slnx outside the selected repositories.",
        };
        var command = new RootCommand("Generate one ignored .slnx inside each repository from its projects and dependencies.");
        command.Options.Add(roots);
        command.Options.Add(parent);
        command.Options.Add(watch);
        command.Options.Add(check);
        command.Options.Add(list);
        command.Options.Add(aggregate);
        command.SetAction(parse => execute(Create(
            parse.GetValue(roots) ?? [], parse.GetValue(parent), parse.GetValue(watch),
            parse.GetValue(check), parse.GetValue(list), parse.GetValue(aggregate))));
        return command;
    }

    /// <summary>Validates mutually exclusive operating modes and resolves paths once.</summary>
    internal static SolutionOptions Create(string[] roots, string? parent, bool watch, bool check, bool list, string? aggregate)
    {
        if (roots.Length > 0 && parent is not null)
            throw new ArgumentException("Choose --repo-root or --parent-directory, not both.");

        if ((watch && check) || (list && (watch || check)))
            throw new ArgumentException("--watch, --check, and --list-only are separate operating modes.");

        if (roots.Length == 0 && parent is null)
            roots = [RepositoryRoot.FindFrom(Environment.CurrentDirectory)];

        if (aggregate != null)
        {
            aggregate = Path.GetFullPath(aggregate);

            if (!Path.GetExtension(aggregate).Equals(".slnx", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("--aggregate-solution must name a .slnx file.");

            if (!Directory.Exists(Path.GetDirectoryName(aggregate)))
                throw new DirectoryNotFoundException($"Aggregate solution directory does not exist: {aggregate}");
        }

        return new(roots.Select(Path.GetFullPath).Distinct(SolutionPaths.Comparer).ToArray(),
            parent is null ? null : Path.GetFullPath(parent), watch, check, list, aggregate);
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

            ValidateAggregate(RepositoryRoots);
            return RepositoryRoots;
        }

        var roots = Directory.EnumerateDirectories(ParentDirectory)
            .Where(root => File.Exists(Path.Combine(root, ".git")) || Directory.Exists(Path.Combine(root, ".git")))
            .Where(root => RepositoryProjects.Discover(root).Count > 0 || SolutionGenerator.HasGeneratedSolution(root))
            .Order(StringComparer.Ordinal).ToArray();
        ValidateAggregate(roots);
        return roots;
    }

    /// <summary>Keeps aggregate output outside repository-owned solutions and gives each checkout a distinct folder.</summary>
    internal void ValidateAggregateLocation()
    {
        if (AggregateSolution == null)
            return;

        var roots = ParentDirectory == null ? RepositoryRoots : Directory.EnumerateDirectories(ParentDirectory)
            .Where(root => File.Exists(Path.Combine(root, ".git")) || Directory.Exists(Path.Combine(root, ".git")));
        ValidateAggregate(roots);
    }

    /// <summary>Validates output ownership against a known selection before publishing or removing any files.</summary>
    internal void ValidateAggregate(IEnumerable<string> roots)
    {
        if (AggregateSolution == null)
            return;

        var names = new HashSet<string>(SolutionPaths.Comparer);

        foreach (var root in roots)
        {
            if (SolutionPaths.IsWithin(root, AggregateSolution))
                throw new ArgumentException($"Aggregate solution must be outside selected repository: {root}");

            if (!names.Add(Path.GetFileName(Path.TrimEndingDirectorySeparator(root))))
                throw new ArgumentException("Aggregate repositories must have distinct checkout directory names.");
        }
    }
}

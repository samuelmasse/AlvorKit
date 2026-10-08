namespace AlvorKit;

/// <summary>Chooses explicit repositories or sibling discovery and one-shot or continuous generation.</summary>
internal record SolutionOptions(IReadOnlyList<string> RepositoryRoots, string? ParentDirectory,
    bool Watch, bool Check, bool ListOnly, string? AggregateSolution)
{
    /// <summary>Includes the parent checkout's graph in its aggregate instead of publishing a competing local solution.</summary>
    public bool IncludeParent { get; init; }

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
            Description = "Generate a combined .slnx outside selected repositories, or in the parent with --include-parent.",
        };
        var includeParent = new Option<bool>("--include-parent")
        {
            Description = "Include the parent Git checkout in the aggregate written to its repository solution path.",
        };
        var command = new RootCommand("Generate one ignored .slnx inside each repository from its projects and dependencies.");
        command.Options.Add(roots);
        command.Options.Add(parent);
        command.Options.Add(watch);
        command.Options.Add(check);
        command.Options.Add(list);
        command.Options.Add(aggregate);
        command.Options.Add(includeParent);
        command.SetAction(parse => execute(Create(
            parse.GetValue(roots) ?? [], parse.GetValue(parent), parse.GetValue(watch),
            parse.GetValue(check), parse.GetValue(list), parse.GetValue(aggregate)) with
        {
            IncludeParent = parse.GetValue(includeParent),
        }));
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
        ValidateParent();

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

        if (IncludeParent)
            roots = [ParentDirectory, .. roots];

        ValidateAggregate(roots);
        return roots;
    }

    /// <summary>Validates aggregate output ownership and distinct checkout folders.</summary>
    internal void ValidateAggregateLocation()
    {
        ValidateParent();

        if (AggregateSolution == null)
            return;

        var roots = ParentDirectory == null ? RepositoryRoots : Directory.EnumerateDirectories(ParentDirectory)
            .Where(root => File.Exists(Path.Combine(root, ".git")) || Directory.Exists(Path.Combine(root, ".git")));

        if (IncludeParent)
            roots = roots.Prepend(ParentDirectory!);

        ValidateAggregate(roots);
    }

    /// <summary>Identifies the checkout whose ordinary solution path is owned exclusively by the aggregate writer.</summary>
    internal bool IsAggregateOwner(string root) => IncludeParent && SolutionPaths.Comparer.Equals(root, ParentDirectory);

    /// <summary>Requires an initialized parent checkout and one unambiguous output for its combined graph.</summary>
    private void ValidateParent()
    {
        if (!IncludeParent)
            return;

        if (ParentDirectory == null || AggregateSolution == null)
            throw new ArgumentException("--include-parent requires --parent-directory and --aggregate-solution.");

        var marker = Path.Combine(ParentDirectory, ".git");

        if (!Directory.Exists(marker) && !File.Exists(marker))
            throw new ArgumentException("--include-parent requires the parent directory to be a Git checkout.");

        if (!SolutionPaths.Comparer.Equals(AggregateSolution, RepositoryProjects.SolutionPath(ParentDirectory)))
            throw new ArgumentException("--include-parent must write the aggregate to the parent checkout's repository solution path.");
    }

    /// <summary>Validates output ownership against a known selection before publishing or removing any files.</summary>
    internal void ValidateAggregate(IEnumerable<string> roots)
    {
        ValidateParent();

        if (AggregateSolution == null)
            return;

        var names = new HashSet<string>(SolutionPaths.Comparer);

        foreach (var root in roots)
        {
            if (SolutionPaths.IsWithin(root, AggregateSolution) && !IsAggregateOwner(root))
                throw new ArgumentException($"Aggregate solution must be outside selected repository: {root}");

            if (!names.Add(Path.GetFileName(Path.TrimEndingDirectorySeparator(root))))
                throw new ArgumentException("Aggregate repositories must have distinct checkout directory names.");
        }
    }
}

namespace AlvorKit;

/// <summary>Reconciles one generated repository solution without consulting any previous solution membership.</summary>
internal static class SolutionGenerator
{
    /// <summary>Reconciles selected repositories and combines their evaluated graphs without reading generated solutions.</summary>
    internal static bool Generate(SolutionOptions options)
    {
        var generations = new List<SolutionGeneration>();
        var success = true;
        options.ValidateAggregateLocation();

        try
        {
            var roots = options.DiscoverRepositories();

            if (options.AggregateSolution != null)
                SolutionOutput.Validate(options.AggregateSolution);

            foreach (var root in roots)
            {
                try
                {
                    var generation = Prepare(root, null);

                    if (!options.IsAggregateOwner(root))
                        success &= Publish(generation, options.Check);

                    generations.Add(generation);
                }
                catch
                {
                    if (!options.Check)
                        Invalidate(root);

                    throw;
                }
            }

            if (options.AggregateSolution != null)
                success &= SolutionAggregate.Publish(options.AggregateSolution, generations, options.Check);

            return success;
        }
        catch
        {
            if (!options.Check && options.AggregateSolution != null)
                SolutionOutput.Invalidate(options.AggregateSolution);

            throw;
        }
    }

    /// <summary>Evaluates the graph, checks freshness, and optionally publishes changed output atomically.</summary>
    public static bool Generate(string root, bool check, SolutionWatchInputs? inputs)
    {
        try { return Publish(Prepare(root, inputs), check); }
        catch
        {
            if (!check)
                Invalidate(root);

            throw;
        }
    }

    /// <summary>Evaluates and serializes without writing, allowing a watcher to reject a changed checkout.</summary>
    internal static SolutionGeneration Prepare(string root, SolutionWatchInputs? inputs)
    {
        var fullRoot = Path.GetFullPath(root);

        foreach (var path in Directory.EnumerateFiles(fullRoot, "*.slnx"))
        {
            if (!SolutionOutput.IsGenerated(path))
                throw new InvalidOperationException($"Remove the authored solution before generating this repository: {path}");
        }

        var projects = SolutionGraph.Read(fullRoot, inputs);
        return new(fullRoot, SolutionDocument.Format(fullRoot, projects), projects);
    }

    /// <summary>Publishes only a complete document, replacing existing output atomically when it changes.</summary>
    internal static bool Publish(SolutionGeneration generation, bool check)
    {
        var (fullRoot, content, projects) = generation;
        var output = RepositoryProjects.SolutionPath(fullRoot);
        var current = SolutionOutput.Publish(output, content, projects.Count, check);

        var obsolete = Directory.EnumerateFiles(fullRoot, "*.slnx")
            .Where(path => !SolutionPaths.Comparer.Equals(path, output) && SolutionOutput.IsGenerated(path)).ToArray();

        foreach (var path in obsolete)
        {
            if (check)
                Console.WriteLine($"Obsolete generated solution: {path}");
            else File.Delete(path);
        }

        return !check || (current && obsolete.Length == 0);
    }

    /// <summary>Removes our previous output if the project graph is invalid; never deletes authored solutions.</summary>
    public static void Invalidate(string root)
    {
        try
        {
            foreach (var path in Directory.EnumerateFiles(root, "*.slnx").Where(SolutionOutput.IsGenerated))
                File.Delete(path);
        }
        catch (DirectoryNotFoundException) when (!Directory.Exists(root)) { }
    }

    /// <summary>Keeps empty repositories discoverable until their last generated solution is invalidated.</summary>
    internal static bool HasGeneratedSolution(string root)
        => Directory.EnumerateFiles(root, "*.slnx").Any(SolutionOutput.IsGenerated);
}

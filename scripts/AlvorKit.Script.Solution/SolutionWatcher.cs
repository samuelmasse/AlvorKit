namespace AlvorKit;

/// <summary>Regenerates only solutions whose discovery or evaluated inputs changed; performs no idle polling.</summary>
internal class SolutionWatcher(SolutionOptions options) : IDisposable
{
    /// <summary>Serializes discovery and evaluation work from concurrent native callbacks.</summary>
    private readonly SolutionWatchQueue queue = new();
    /// <summary>Owns live checkout subscriptions and their writer leases.</summary>
    private readonly Dictionary<string, SolutionRepositoryWatch> repositories = new(SolutionPaths.Comparer);
    /// <summary>Prevents duplicate parent-directory hosts, including when no child checkout exists.</summary>
    private FileStream? parentLease;
    /// <summary>Observes checkout creation, removal, and movement above repository directories.</summary>
    private SolutionWatchInputs? topology;
    /// <summary>Shares native directory handles across all discovery and graph subscriptions.</summary>
    private SolutionFileNotifications? notifications;

    /// <summary>Reports completed evaluation attempts for diagnostics and behavioral verification.</summary>
    internal event Action<string>? Evaluated;

    /// <summary>Registers notifications before initial discovery and consumes scoped invalidations until cancellation.</summary>
    public async Task RunAsync(CancellationToken cancellation)
    {
        var roots = options.ParentDirectory is { } parent ? [parent] : options.RepositoryRoots;
        var scopes = options.ParentDirectory is not null ? roots
            : roots.Select(root => Directory.GetParent(root)?.FullName ?? root).Distinct(SolutionPaths.Comparer).ToArray();
        notifications = new(scopes, queue.Fail);

        if (options.ParentDirectory is not null)
            parentLease = SolutionWatchLease.Acquire(options.ParentDirectory);

        Console.WriteLine($"Watching {string.Join(", ", roots)}. Press Ctrl+C to stop.");
        queue.Add(new("", true));

        while (true)
        {
            var batch = await queue.ReadAsync(cancellation);
            var discover = batch.Where(request => request.Discovery).Select(request => request.Root)
                .ToHashSet(SolutionPaths.Comparer);
            var generate = batch.Where(request => !request.Discovery).Select(request => request.Root)
                .ToHashSet(SolutionPaths.Comparer);

            if (discover.Remove(""))
                discover.UnionWith(RefreshRepositories());

            foreach (var root in discover.Where(repositories.ContainsKey))
            {
                try
                {
                    if (repositories[root].Discover())
                        generate.Add(root);
                }
                catch (Exception exception) { repositories[root].Invalidate(exception); }
            }

            foreach (var root in generate.Where(repositories.ContainsKey).Order(StringComparer.Ordinal))
            {
                cancellation.ThrowIfCancellationRequested();
                queue.ThrowIfFailed();
                var elapsed = Stopwatch.StartNew();

                try
                {
                    if (!repositories[root].Generate())
                        continue;
                }
                catch (Exception exception) { repositories[root].Invalidate(exception); }

                Console.WriteLine($"Evaluated {root} in {elapsed.Elapsed.TotalSeconds:F1}s.");
                Evaluated?.Invoke(root);
            }
        }
    }

    /// <summary>Checks checkout topology without rescanning unchanged repositories or evaluating their graphs.</summary>
    private IReadOnlyList<string> RefreshRepositories()
    {
        var next = new SolutionWatchInputs(notifications!, () => queue.Add(new("", true)));
        var roots = options.RepositoryRoots;

        if (options.ParentDirectory is { } parent)
        {
            next.Glob(parent, "*", false, false);
            var children = Directory.GetDirectories(parent);

            roots = children;
        }

        foreach (var root in roots)
            next.File(Path.Combine(root, ".git"));

        roots = roots.Where(root => File.Exists(Path.Combine(root, ".git")) || Directory.Exists(Path.Combine(root, ".git")))
            .ToArray();

        var added = roots.Except(repositories.Keys, SolutionPaths.Comparer).ToArray();

        foreach (var removed in repositories.Keys.Except(roots, SolutionPaths.Comparer).ToArray())
        {
            repositories[removed].Dispose();
            repositories.Remove(removed);

            if (Directory.Exists(removed))
                SolutionGenerator.Invalidate(removed);
        }

        foreach (var root in added)
            repositories.Add(root, new(root, queue, notifications!));

        topology?.Dispose();
        topology = next;
        return added;
    }

    /// <summary>Releases subscriptions and singleton locks when the process exits.</summary>
    public void Dispose()
    {
        topology?.Dispose();
        topology = null;

        foreach (var repository in repositories.Values)
            repository.Dispose();

        parentLease?.Dispose();
        parentLease = null;
        notifications?.Dispose();
        notifications = null;
        repositories.Clear();
    }
}

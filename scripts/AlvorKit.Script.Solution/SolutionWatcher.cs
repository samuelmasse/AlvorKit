namespace AlvorKit;

/// <summary>Serializes debounced filesystem reconciliation for a group of repository solutions.</summary>
internal class SolutionWatcher(SolutionOptions options) : IDisposable
{
    private readonly Channel<bool> changes = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropWrite,
        SingleReader = true,
    });
    private readonly List<FileSystemWatcher> watchers = [];
    private readonly List<FileStream> locks = [];
    private readonly HashSet<string> failures = new(SolutionPaths.Comparer);
    private readonly HashSet<string> repositories = new(SolutionPaths.Comparer);

    /// <summary>Registers notifications before the initial scan, then reconciles until cancellation.</summary>
    public async Task RunAsync(CancellationToken cancellation)
    {
        var roots = WatchRoots();

        foreach (var root in roots)
        {
            locks.Add(new FileStream(Path.Combine(root, ".alvorkit-solution-watch.lock"), FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose));
            Observe(root, false);
            Observe(root, true);
        }

        changes.Writer.TryWrite(true);
        Console.WriteLine($"Watching {string.Join(", ", roots)}. Press Ctrl+C to stop.");

        using var timer = new Timer(_ => changes.Writer.TryWrite(true), null,
            TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));

        while (await changes.Reader.WaitToReadAsync(cancellation))
        {
            changes.Reader.TryRead(out _);

            // Bound debounce so continuous checkout activity cannot starve reconciliation.
            for (var attempt = 0; attempt < 5; attempt++)
            {
                await Task.Delay(400, cancellation);

                if (!changes.Reader.TryRead(out _))
                    break;
            }

            Reconcile();
        }
    }

    /// <summary>Separates directory events so deleting a dotted project directory remains observable.</summary>
    private void Observe(string root, bool directory)
    {
        var watcher = new FileSystemWatcher(root)
        {
            IncludeSubdirectories = true,
            NotifyFilter = directory ? NotifyFilters.DirectoryName : NotifyFilters.FileName | NotifyFilters.LastWrite,
        };

        if (!directory)
        {
            foreach (var filter in new[] { "*.csproj", "*.props", "*.targets", "*.proj", "*.xml", "*.json", "*.config", ".git" })
                watcher.Filters.Add(filter);
        }

        watcher.Changed += (_, change) => Changed(root, change.FullPath, directory);
        watcher.Created += (_, change) => Changed(root, change.FullPath, directory);
        watcher.Deleted += (_, change) => Changed(root, change.FullPath, directory);
        watcher.Renamed += (_, change) =>
        {
            Changed(root, change.OldFullPath, directory);
            Changed(root, change.FullPath, directory);
        };
        watcher.Error += (_, error) =>
        {
            Console.Error.WriteLine($"Watcher invalidated: {error.GetException().Message}");

            if (error.GetException() is InternalBufferOverflowException)
                changes.Writer.TryWrite(true);
            else changes.Writer.TryComplete(error.GetException());
        };
        watchers.Add(watcher);
        watcher.EnableRaisingEvents = true;
    }

    /// <summary>Uses parent directories to observe repository moves and sibling dependency changes.</summary>
    private IReadOnlyList<string> WatchRoots()
    {
        if (options.ParentDirectory is not null)
            return [options.ParentDirectory];

        var roots = options.RepositoryRoots.Select(root => Directory.GetParent(root)?.FullName ?? root)
            .Distinct(SolutionPaths.Comparer).ToArray();
        return roots.Where(root => !roots.Any(other => other != root && SolutionPaths.IsWithin(other, root))).ToArray();
    }

    /// <summary>Notifications invalidate the graph; they never directly mutate solution membership.</summary>
    private void Changed(string root, string path, bool directory)
    {
        if (SolutionWatchInputs.Relevant(root, path, directory))
            changes.Writer.TryWrite(true);
    }

    /// <summary>Reports invalid graphs explicitly and reevaluates them on the next filesystem change.</summary>
    private void Reconcile()
    {
        try
        {
            var roots = options.DiscoverRepositories();

            foreach (var removed in repositories.Except(roots, SolutionPaths.Comparer))
            {
                if (Directory.Exists(removed))
                    SolutionGenerator.Invalidate(removed);
            }

            repositories.Clear();
            repositories.UnionWith(roots);
            failures.IntersectWith(roots);
            var elapsed = Stopwatch.StartNew();

            foreach (var root in roots)
            {
                try
                {
                    SolutionGenerator.Generate(root, false);

                    if (failures.Remove(root))
                        Console.WriteLine($"Recovered solution for {root}.");
                }
                catch (Exception exception)
                {
                    SolutionGenerator.Invalidate(root);

                    if (failures.Add(root))
                        Console.Error.WriteLine($"INVALID solution for {root}: {exception.Message}");
                }
            }

            Console.WriteLine($"Reconciled {roots.Count} repositories ({failures.Count} invalid) in {elapsed.Elapsed.TotalSeconds:F1}s.");
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Repository discovery failed: {exception.Message}");
        }
    }

    /// <summary>Releases native notifications when the watcher exits.</summary>
    public void Dispose()
    {
        foreach (var watcher in watchers)
            watcher.Dispose();

        foreach (var file in locks)
            file.Dispose();

        watchers.Clear();
        locks.Clear();
        changes.Writer.TryComplete();
    }
}

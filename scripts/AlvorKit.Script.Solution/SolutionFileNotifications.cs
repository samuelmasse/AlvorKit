namespace AlvorKit;

/// <summary>Shares native directory handles across solution inputs and overlapping evaluation lifetimes.</summary>
internal class SolutionFileNotifications(IReadOnlyList<string> roots, Action<Exception> failed) : IDisposable
{
    /// <summary>Serializes native handle ownership and index changes with concurrent callbacks.</summary>
    private readonly Lock gate = new();
    /// <summary>Deduplicates native handles by directory, recursion, and notification kind.</summary>
    private readonly Dictionary<(string Path, bool Recursive, bool Directory), FileSystemWatcher> watchers = [];
    /// <summary>Tracks readers that must be notified when a native handle loses its directory.</summary>
    private readonly Dictionary<FileSystemWatcher, HashSet<SolutionWatchInputs>> subscriptions = [];
    /// <summary>Releases handles when an evaluation's last subscription is removed.</summary>
    private readonly Dictionary<SolutionWatchInputs, HashSet<FileSystemWatcher>> owners = [];
    /// <summary>Routes changes by path instead of visiting every repository graph.</summary>
    private readonly SolutionWatchIndex index = new();

    /// <summary>Stops the owning session when evaluation cannot establish its filesystem inputs.</summary>
    public void Stop(Exception exception) => failed(exception);

    /// <summary>Subscribes before the filesystem read, including parent directories needed to detect moves.</summary>
    public void Add(SolutionWatchInputs owner, SolutionWatchInput input)
    {
        lock (gate)
        {
            index.Add(owner, input);
            var directory = input.Pattern is null ? Path.GetDirectoryName(input.Path) ?? input.Path : input.Path;

            while (!Directory.Exists(directory))
                directory = Path.GetDirectoryName(directory) ?? throw new DirectoryNotFoundException(input.Path);

            var recursive = input.Recursive && SolutionPaths.Comparer.Equals(directory, input.Path);
            Observe(directory, recursive, false, owner);
            Observe(directory, recursive, true, owner);

            // A handle inside a directory cannot tell us that an ancestor was moved or removed.
            while (Path.GetDirectoryName(directory) is { } parent)
            {
                Observe(parent, false, true, owner);
                directory = parent;
            }
        }
    }

    /// <summary>Creates at most one native handle for each directory, depth, and notification kind.</summary>
    private void Observe(string path, bool recursive, bool directory, SolutionWatchInputs owner)
    {
        // Windows permits moving descendants of a recursive watch but blocks moves above a watched subdirectory.
        var root = roots.FirstOrDefault(root => SolutionPaths.IsWithin(root, path));

        if (root is not null)
        {
            path = root;
            recursive = true;
        }

        var key = (path, recursive, directory);
        var created = !watchers.TryGetValue(key, out var watcher);

        if (created)
        {
            watcher = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = recursive,
                NotifyFilter = directory ? NotifyFilters.DirectoryName : NotifyFilters.FileName | NotifyFilters.LastWrite,
                InternalBufferSize = 64 * 1024,
            };
            subscriptions.Add(watcher, []);
            watcher.Changed += (_, change) => Notify(watcher, change.FullPath, directory, change.ChangeType);
            watcher.Created += (_, change) => Notify(watcher, change.FullPath, directory, change.ChangeType);
            watcher.Deleted += (_, change) => Notify(watcher, change.FullPath, directory, change.ChangeType);
            watcher.Renamed += (_, change) =>
            {
                Notify(watcher, change.OldFullPath, directory, change.ChangeType);
                Notify(watcher, change.FullPath, directory, change.ChangeType);
            };
            watcher.Error += (_, error) => WatchFailed(watcher, error.GetException());
            watchers.Add(key, watcher);
        }

        subscriptions[watcher!].Add(owner);

        if (!owners.TryGetValue(owner, out var handles))
            owners.Add(owner, handles = []);

        handles.Add(watcher!);

        if (created)
        {
            try { watcher!.EnableRaisingEvents = true; }
            catch (Exception exception) { failed(exception); throw; }
        }
    }

    /// <summary>Dispatches only to matching owners; callbacks never evaluate projects or write solutions.</summary>
    private void Notify(FileSystemWatcher watcher, string path, bool directory, WatcherChangeTypes change)
    {
        IReadOnlySet<SolutionWatchInputs> affected;

        lock (gate)
        {
            if (!subscriptions.ContainsKey(watcher))
                return;

            affected = index.Match(path, directory, change);
        }

        foreach (var owner in affected)
            owner.Notify();
    }

    /// <summary>Directory removal invalidates its readers; lost notifications on a live directory end the session.</summary>
    private void WatchFailed(FileSystemWatcher watcher, Exception exception)
    {
        lock (gate)
        {
            if (!subscriptions.ContainsKey(watcher))
                return;
        }

        try { File.GetAttributes(watcher.Path); }
        catch (FileNotFoundException) { Removed(watcher); return; }
        catch (DirectoryNotFoundException) { Removed(watcher); return; }
        catch (Exception error) { failed(error); return; }

        failed(new IOException($"Filesystem watch failed for '{watcher.Path}': {exception.Message}", exception));
    }

    /// <summary>Retires an invalid native handle so a recreated directory receives a new one.</summary>
    private void Removed(FileSystemWatcher watcher)
    {
        SolutionWatchInputs[] affected;

        lock (gate)
        {
            if (!subscriptions.Remove(watcher, out var rules))
                return;

            affected = [.. rules];

            foreach (var owner in affected)
                owners[owner].Remove(watcher);

            watchers.Remove((watcher.Path, watcher.IncludeSubdirectories, watcher.NotifyFilter == NotifyFilters.DirectoryName));
            watcher.Dispose();
        }

        foreach (var owner in affected)
            owner.Notify();
    }

    /// <summary>Releases a completed evaluation's rules; shared handles remain alive for their other owners.</summary>
    public void Remove(SolutionWatchInputs owner)
    {
        lock (gate)
        {
            index.Remove(owner);

            if (!owners.Remove(owner, out var handles))
                return;

            foreach (var watcher in handles)
            {
                subscriptions[watcher].Remove(owner);

                if (subscriptions[watcher].Count != 0)
                    continue;

                subscriptions.Remove(watcher);
                watchers.Remove((watcher.Path, watcher.IncludeSubdirectories, watcher.NotifyFilter == NotifyFilters.DirectoryName));
                watcher.Dispose();
            }
        }
    }

    /// <summary>Releases all native notifications on shutdown, even if an evaluation was interrupted.</summary>
    public void Dispose()
    {
        lock (gate)
        {
            foreach (var watcher in watchers.Values)
                watcher.Dispose();

            watchers.Clear();
            subscriptions.Clear();
            owners.Clear();
            index.Clear();
        }
    }
}

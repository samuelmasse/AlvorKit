namespace AlvorKit;

/// <summary>Maps native paths to input owners without scanning every graph on each filesystem event.</summary>
internal class SolutionWatchIndex
{
    /// <summary>Groups exact reads and globs by their observed path and owning evaluation.</summary>
    private readonly Dictionary<string, Dictionary<SolutionWatchInputs, HashSet<SolutionWatchInput>>> rules = new(SolutionPaths.Comparer);
    /// <summary>Finds readers below a renamed or removed directory.</summary>
    private readonly Dictionary<string, HashSet<SolutionWatchInputs>> ancestors = new(SolutionPaths.Comparer);
    /// <summary>Supports removing one owner's rules without scanning unrelated evaluations.</summary>
    private readonly Dictionary<SolutionWatchInputs, HashSet<string>> owners = [];

    /// <summary>Indexes exact reads, globs, and ancestor moves under the same input owner.</summary>
    public void Add(SolutionWatchInputs owner, SolutionWatchInput input)
    {
        if (!owners.TryGetValue(owner, out var paths))
            owners.Add(owner, paths = new(SolutionPaths.Comparer));

        if (!rules.TryGetValue(input.Path, out var entries))
            rules.Add(input.Path, entries = []);

        if (!entries.TryGetValue(owner, out var inputs))
            entries.Add(owner, inputs = []);

        inputs.Add(input);
        paths.Add(input.Path);
        var path = input.Path;

        while (Path.GetDirectoryName(path) is { } parent)
        {
            if (!ancestors.TryGetValue(parent, out var readers))
                ancestors.Add(parent, readers = []);

            readers.Add(owner);
            paths.Add(parent);
            path = parent;
        }
    }

    /// <summary>Looks up the changed path and its containing glob scopes; directory moves also invalidate descendants.</summary>
    public IReadOnlySet<SolutionWatchInputs> Match(string path, bool directory, WatcherChangeTypes change)
    {
        var result = new HashSet<SolutionWatchInputs>();

        if (directory && ancestors.TryGetValue(path, out var descendants))
            result.UnionWith(descendants);

        for (string? scope = path; scope is not null; scope = Path.GetDirectoryName(scope))
        {
            if (!rules.TryGetValue(scope, out var entries))
                continue;

            foreach (var pair in entries)
            {
                if (pair.Value.Any(rule => rule.Matches(path, directory, change)))
                    result.Add(pair.Key);
            }
        }

        return result;
    }

    /// <summary>Removes a superseded evaluation while retaining paths shared by other solutions.</summary>
    public void Remove(SolutionWatchInputs owner)
    {
        if (!owners.Remove(owner, out var paths))
            return;

        foreach (var path in paths)
        {
            if (rules.TryGetValue(path, out var entries))
            {
                entries.Remove(owner);

                if (entries.Count == 0)
                    rules.Remove(path);
            }

            if (ancestors.TryGetValue(path, out var readers))
            {
                readers.Remove(owner);

                if (readers.Count == 0)
                    ancestors.Remove(path);
            }
        }
    }

    /// <summary>Clears the index after all native handles are stopped.</summary>
    public void Clear()
    {
        rules.Clear();
        ancestors.Clear();
        owners.Clear();
    }
}

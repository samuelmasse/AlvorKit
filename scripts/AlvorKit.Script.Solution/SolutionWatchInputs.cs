namespace AlvorKit;

/// <summary>Subscribes before filesystem reads and owns their native notifications until replaced or disposed.</summary>
internal class SolutionWatchInputs(SolutionFileNotifications notifications, Action changed) : IDisposable
{
    /// <summary>Protects the read set while callbacks and evaluation replace subscriptions.</summary>
    private readonly Lock gate = new();
    /// <summary>Deduplicates filesystem reads owned by one discovery or graph evaluation.</summary>
    private readonly HashSet<SolutionWatchInput> inputs = [];

    /// <summary>Observes file contents, replacement, deletion, and creation when the file is absent.</summary>
    public void File(string path) => Add(new(Path.GetFullPath(path), null, false, true));

    /// <summary>Watches XML loaded by MSBuild's separate cache and detects changes during subscription setup.</summary>
    public void ReadProject(string path, DateTime readTime)
    {
        if (string.IsNullOrEmpty(path))
            return;

        File(path);

        if (System.IO.File.GetLastWriteTimeUtc(path) != readTime)
            changed();
    }

    /// <summary>Keeps known dependencies observable while a failed evaluation cannot produce a complete replacement.</summary>
    public void Include(SolutionWatchInputs previous)
    {
        SolutionWatchInput[] retained;

        lock (previous.gate)
            retained = [.. previous.inputs];

        foreach (var input in retained)
            Add(input);
    }

    /// <summary>Observes existence without treating ordinary content writes as graph changes.</summary>
    public void Exists(string path) => Add(new(Path.GetFullPath(path), null, false, false));

    /// <summary>Observes a directory query and any rename that changes its matching members.</summary>
    public void Glob(string path, string pattern, bool recursive, bool contents) =>
        Add(new(Path.GetFullPath(path), pattern, recursive, contents));

    /// <summary>Installs each distinct read before the caller evaluates its value.</summary>
    private void Add(SolutionWatchInput input)
    {
        lock (gate)
        {
            if (inputs.Add(input))
                notifications.Add(this, input);
        }
    }

    /// <summary>Schedules its owner when a matching native notification arrives.</summary>
    public void Notify() => changed();

    /// <summary>Rejects an evaluation whose remaining inputs cannot be observed reliably.</summary>
    public void Stop(Exception exception) => notifications.Stop(exception);

    /// <summary>Releases this evaluation's rules without disturbing another solution's subscriptions.</summary>
    public void Dispose()
    {
        lock (gate)
        {
            notifications.Remove(this);
            inputs.Clear();
        }
    }
}

namespace AlvorKit;

/// <summary>Observes Git initialization and index transactions without retaining checkout file handles.</summary>
internal class SolutionCheckout(string root, SolutionFileNotifications notifications, Action changed) : IDisposable
{
    /// <summary>Reserves this checkout without placing an open file inside it.</summary>
    private readonly FileStream lease = SolutionWatchLease.Acquire(root);
    /// <summary>Keeps metadata changes observable between discovery passes.</summary>
    private SolutionWatchInputs? inputs;
    /// <summary>Identifies the index and its adjacent transaction lock after Git initialization.</summary>
    private string? index;
    /// <summary>Detects metadata removal while an evaluation is in progress.</summary>
    private string? head;
    /// <summary>Counts actual metadata events so completed transactions still invalidate in-flight evaluation.</summary>
    private long revision;

    /// <summary>Distinguishes checkout changes during evaluation, even if Git already released its lock.</summary>
    public long Revision => Interlocked.Read(ref revision);

    /// <summary>Requires initialized metadata and no active index writer before reading the checkout.</summary>
    public bool Ready => index is not null && File.Exists(head) && !File.Exists(index + ".lock");

    /// <summary>Installs initialization and transaction watches before consulting Git or its index lock.</summary>
    public bool Refresh()
    {
        var next = new SolutionWatchInputs(notifications, Changed);
        index = null;
        head = null;

        try
        {
            var marker = Path.Combine(root, ".git");
            next.File(marker);
            var directory = marker;

            if (File.Exists(marker))
            {
                var text = File.ReadAllText(marker).Trim();

                if (!text.StartsWith("gitdir: ", StringComparison.Ordinal))
                    throw new InvalidOperationException($"Invalid Git directory marker: {marker}");

                directory = Path.GetFullPath(text[8..], root);
            }

            head = Path.Combine(directory, "HEAD");
            next.File(head);
            var commonFile = Path.Combine(directory, "commondir");
            next.File(commonFile);
            var common = File.Exists(commonFile)
                ? Path.GetFullPath(File.ReadAllText(commonFile).Trim(), directory) : directory;
            next.Exists(Path.Combine(common, "objects"));
            next.Exists(Path.Combine(common, "refs"));

            if (!File.Exists(head) || !Directory.Exists(Path.Combine(common, "objects")))
                return false;

            if (!Directory.Exists(Path.Combine(common, "refs")))
                return false;

            index = RepositoryGit.Read(root, "rev-parse", "--path-format=absolute", "--git-path", "index").TrimEnd('\r', '\n');
            next.File(index);
            next.Exists(index + ".lock");
            return Ready;
        }
        finally
        {
            inputs?.Dispose();
            inputs = next;
        }
    }

    /// <summary>Queues discovery on a real Git event; no timer retries an incomplete checkout.</summary>
    private void Changed()
    {
        Interlocked.Increment(ref revision);
        changed();
    }

    /// <summary>Releases subscriptions and the external lease when a repository leaves the watched set.</summary>
    public void Dispose()
    {
        inputs?.Dispose();
        inputs = null;
        lease.Dispose();
    }
}

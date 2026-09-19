namespace AlvorKit;

/// <summary>Prevents competing writers without keeping a lock file inside a managed checkout.</summary>
internal static class SolutionWatchLease
{
    /// <summary>Uses a stable per-user path identity; the stream owns the lease until disposed.</summary>
    public static FileStream Acquire(string root)
    {
        var identity = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));

        if (OperatingSystem.IsWindows())
            identity = identity.ToUpperInvariant();

        var key = System.IO.Hashing.XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(identity));
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AlvorKit", "SolutionWatcher");
        Directory.CreateDirectory(directory);

        try
        {
            return new FileStream(Path.Combine(directory, $"{key:X16}.lock"), FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);
        }
        catch (IOException exception)
        {
            throw new IOException($"Cannot acquire the solution watcher lease for '{root}': {exception.Message}", exception);
        }
    }
}

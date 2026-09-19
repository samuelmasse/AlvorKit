namespace AlvorKit;

/// <summary>A filesystem read: contents, existence, or directory membership.</summary>
internal record SolutionWatchInput(string Path, string? Pattern, bool Recursive, bool Contents)
{
    /// <summary>Matches content writes separately from file or directory topology changes.</summary>
    public bool Matches(string path, bool directory, WatcherChangeTypes change)
    {
        if (change == WatcherChangeTypes.Changed && (!Contents || directory))
            return false;

        if (SolutionPaths.Comparer.Equals(Path, path))
            return true;

        if (directory && SolutionPaths.IsWithin(path, Path))
            return true;

        if (Pattern is null || !SolutionPaths.IsWithin(Path, path))
            return false;

        var relative = System.IO.Path.GetRelativePath(Path, path);

        if (!Recursive && relative.Contains(System.IO.Path.DirectorySeparatorChar))
            return false;

        return directory || System.IO.Enumeration.FileSystemName.MatchesSimpleExpression(
            Pattern, System.IO.Path.GetFileName(path), OperatingSystem.IsWindows());
    }
}

namespace AlvorKit.Script.Workspace;

/// <summary>Finds the repository root for script tools that can run from build, test, or command-line directories.</summary>
public static class ProjectRoot
{
    /// <summary>The solution file that marks the repository root.</summary>
    public const string SolutionFileName = "AlvorKit.slnx";

    /// <summary>Finds the nearest repository root above a path.</summary>
    /// <param name="startPath">A file or directory path inside the repository.</param>
    public static string FindFrom(string startPath) =>
        FindFrom(startPath, requireResDirectory: false);

    /// <summary>Finds the nearest repository root above a path and optionally requires the root <c>res</c> directory.</summary>
    /// <param name="startPath">A file or directory path inside the repository.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    public static string FindFrom(string startPath, bool requireResDirectory)
    {
        if (string.IsNullOrWhiteSpace(startPath))
            throw new ArgumentException("Start path must not be blank.", nameof(startPath));

        if (TryFindFrom(startPath, requireResDirectory, out var root))
            return root;

        var marker = requireResDirectory ? $"{SolutionFileName} and res" : SolutionFileName;
        throw new InvalidOperationException($"{marker} not found above {startPath}.");
    }

    /// <summary>Finds the repository root from likely process directories and an optional assembly anchor.</summary>
    /// <param name="anchor">A type from the calling assembly, used to add its output directory as a search candidate.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    public static string FindFromCurrentProcess(Type? anchor = null, bool requireResDirectory = false) =>
        FindFromCandidates(CandidateDirectories(anchor), requireResDirectory);

    /// <summary>Finds the repository root from ordered candidate directories.</summary>
    /// <param name="startPaths">Candidate paths to search, in priority order.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    internal static string FindFromCandidates(IEnumerable<string?> startPaths, bool requireResDirectory = false)
    {
        foreach (var start in startPaths)
        {
            if (string.IsNullOrWhiteSpace(start))
                continue;

            if (TryFindFrom(start, requireResDirectory, out var root))
                return root;
        }

        var marker = requireResDirectory ? $"{SolutionFileName} and res" : SolutionFileName;
        throw new InvalidOperationException($"Could not find {marker} above the current process directories.");
    }

    /// <summary>Returns the root <c>res</c> directory for the current process.</summary>
    /// <param name="anchor">A type from the calling assembly, used to add its output directory as a search candidate.</param>
    public static string ResDirectory(Type? anchor = null) =>
        Path.Combine(FindFromCurrentProcess(anchor, requireResDirectory: true), "res");

    /// <summary>Attempts to find a repository root above a path.</summary>
    private static bool TryFindFrom(string startPath, bool requireResDirectory, [NotNullWhen(true)] out string? root)
    {
        for (var current = StartingDirectory(startPath); current is not null; current = Directory.GetParent(current)?.FullName)
        {
            if (IsRepositoryRoot(current, requireResDirectory))
            {
                root = current;
                return true;
            }
        }

        root = null;
        return false;
    }

    /// <summary>Normalizes an input path to a directory from which upward search can begin.</summary>
    private static string StartingDirectory(string startPath)
    {
        var fullPath = Path.GetFullPath(startPath);
        if (Directory.Exists(fullPath))
            return fullPath;

        return Path.GetDirectoryName(fullPath)!;
    }

    /// <summary>Returns whether a directory has the required repository-root markers.</summary>
    private static bool IsRepositoryRoot(string directory, bool requireResDirectory) =>
        File.Exists(Path.Combine(directory, SolutionFileName))
        && (!requireResDirectory || Directory.Exists(Path.Combine(directory, "res")));

    /// <summary>Returns likely starting directories for tools launched from tests, builds, or the command line.</summary>
    private static IEnumerable<string?> CandidateDirectories(Type? anchor) =>
    [
        Environment.CurrentDirectory,
        anchor is null ? null : Path.GetDirectoryName(anchor.Assembly.Location),
        AppContext.BaseDirectory
    ];
}

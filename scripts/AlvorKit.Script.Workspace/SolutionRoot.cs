namespace AlvorKit.Script.Workspace;

/// <summary>Finds repository roots and solution files for script tools.</summary>
public static class SolutionRoot
{
    /// <summary>Generated local development solution suffix excluded from primary solution discovery.</summary>
    public const string GeneratedSolutionSuffix = ".Dev.slnx";

    /// <summary>Finds the nearest repository root above a path using a specific solution file marker.</summary>
    /// <param name="startPath">A file or directory path inside the repository.</param>
    /// <param name="solutionFileName">Solution file name that marks the repository root.</param>
    public static string FindFrom(string startPath, string solutionFileName) =>
        FindFrom(startPath, solutionFileName, requireResDirectory: false);

    /// <summary>Finds the nearest repository root above a path using a specific solution file marker.</summary>
    /// <param name="startPath">A file or directory path inside the repository.</param>
    /// <param name="solutionFileName">Solution file name that marks the repository root.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    public static string FindFrom(string startPath, string solutionFileName, bool requireResDirectory)
    {
        ValidateStartPath(startPath);
        ValidateSolutionFileName(solutionFileName);

        if (TryFindFrom(startPath, solutionFileName, requireResDirectory, out var root))
            return root;

        var marker = MarkerText(solutionFileName, requireResDirectory);
        throw new InvalidOperationException($"{marker} not found above {startPath}.");
    }

    /// <summary>Finds the nearest repository root above a path using its primary non-generated solution file.</summary>
    /// <param name="startPath">A file or directory path inside the repository.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    public static string FindPrimaryFrom(string startPath, bool requireResDirectory = false)
    {
        ValidateStartPath(startPath);

        if (TryFindPrimaryFrom(startPath, requireResDirectory, out var root))
            return root;

        var marker = requireResDirectory ? "primary solution file and res" : "primary solution file";
        throw new InvalidOperationException($"{marker} not found above {startPath}.");
    }

    /// <summary>Finds the repository root from likely process directories and an optional assembly anchor.</summary>
    /// <param name="solutionFileName">Solution file name that marks the repository root.</param>
    /// <param name="anchor">A type from the calling assembly, used to add its output directory as a search candidate.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    public static string FindFromCurrentProcess(string solutionFileName, Type? anchor = null, bool requireResDirectory = false) =>
        FindFromCandidates(CandidateDirectories(anchor), solutionFileName, requireResDirectory);

    /// <summary>Finds the repository root from likely process directories using the primary non-generated solution file.</summary>
    /// <param name="anchor">A type from the calling assembly, used to add its output directory as a search candidate.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    public static string FindPrimaryFromCurrentProcess(Type? anchor = null, bool requireResDirectory = false) =>
        FindPrimaryFromCandidates(CandidateDirectories(anchor), requireResDirectory);

    /// <summary>Finds the primary non-generated solution file name at a repository root.</summary>
    /// <param name="repoRoot">Repository root directory.</param>
    public static string PrimarySolutionFileName(string repoRoot)
    {
        var root = Path.GetFullPath(repoRoot);
        if (TryPrimarySolutionFileName(root, out var solutionFileName))
            return solutionFileName;

        throw new InvalidOperationException($"No primary solution file found at '{root}'.");
    }

    /// <summary>Finds the primary non-generated solution file path at a repository root.</summary>
    /// <param name="repoRoot">Repository root directory.</param>
    public static string PrimarySolutionPath(string repoRoot) =>
        Path.Combine(Path.GetFullPath(repoRoot), PrimarySolutionFileName(repoRoot));

    /// <summary>Finds a repository root from ordered candidate directories using a specific solution file marker.</summary>
    /// <param name="startPaths">Candidate paths to search, in priority order.</param>
    /// <param name="solutionFileName">Solution file name that marks the repository root.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    internal static string FindFromCandidates(IEnumerable<string?> startPaths, string solutionFileName, bool requireResDirectory = false)
    {
        ValidateSolutionFileName(solutionFileName);

        foreach (var start in startPaths)
        {
            if (string.IsNullOrWhiteSpace(start))
                continue;

            if (TryFindFrom(start, solutionFileName, requireResDirectory, out var root))
                return root;
        }

        var marker = MarkerText(solutionFileName, requireResDirectory);
        throw new InvalidOperationException($"Could not find {marker} above the current process directories.");
    }

    /// <summary>Finds a repository root from ordered candidate directories using the primary non-generated solution file.</summary>
    /// <param name="startPaths">Candidate paths to search, in priority order.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    internal static string FindPrimaryFromCandidates(IEnumerable<string?> startPaths, bool requireResDirectory = false)
    {
        foreach (var start in startPaths)
        {
            if (string.IsNullOrWhiteSpace(start))
                continue;

            if (TryFindPrimaryFrom(start, requireResDirectory, out var root))
                return root;
        }

        var marker = requireResDirectory ? "primary solution file and res" : "primary solution file";
        throw new InvalidOperationException($"Could not find {marker} above the current process directories.");
    }

    /// <summary>Attempts to find a repository root above a path using a specific solution file marker.</summary>
    private static bool TryFindFrom(string startPath, string solutionFileName, bool requireResDirectory, [NotNullWhen(true)] out string? root)
    {
        for (var current = StartingDirectory(startPath); current is not null; current = Directory.GetParent(current)?.FullName)
        {
            if (File.Exists(Path.Combine(current, solutionFileName)) &&
                (!requireResDirectory || Directory.Exists(Path.Combine(current, "res"))))
            {
                root = current;
                return true;
            }
        }

        root = null;
        return false;
    }

    /// <summary>Attempts to find a repository root above a path using the primary non-generated solution file.</summary>
    private static bool TryFindPrimaryFrom(string startPath, bool requireResDirectory, [NotNullWhen(true)] out string? root)
    {
        for (var current = StartingDirectory(startPath); current is not null; current = Directory.GetParent(current)?.FullName)
        {
            if (TryPrimarySolutionFileName(current, out _) && (!requireResDirectory || Directory.Exists(Path.Combine(current, "res"))))
            {
                root = current;
                return true;
            }
        }

        root = null;
        return false;
    }

    /// <summary>Returns the single primary solution file at a root, or fails on ambiguity.</summary>
    private static bool TryPrimarySolutionFileName(string repoRoot, [NotNullWhen(true)] out string? solutionFileName)
    {
        var solutions = Directory.GetFiles(repoRoot, "*.slnx", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(repoRoot, "*.sln", SearchOption.TopDirectoryOnly))
            .Select(Path.GetFileName)
            .Where(name => name is not null && !name.EndsWith(GeneratedSolutionSuffix, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToArray();

        switch (solutions)
        {
            case []:
                solutionFileName = null;
                return false;
            case [var single]:
                solutionFileName = single!;
                return true;
            default:
                throw new InvalidOperationException(
                    $"Multiple primary solution files found at '{Path.GetFullPath(repoRoot)}': {string.Join(", ", solutions)}.");
        }
    }

    /// <summary>Normalizes an input path to a directory from which upward search can begin.</summary>
    private static string StartingDirectory(string startPath)
    {
        var fullPath = Path.GetFullPath(startPath);
        if (Directory.Exists(fullPath))
            return fullPath;

        return Path.GetDirectoryName(fullPath)!;
    }

    /// <summary>Returns likely starting directories for tools launched from tests, builds, or the command line.</summary>
    private static IEnumerable<string?> CandidateDirectories(Type? anchor) =>
    [
        Environment.CurrentDirectory,
        anchor is null ? null : Path.GetDirectoryName(anchor.Assembly.Location),
        AppContext.BaseDirectory
    ];

    /// <summary>Rejects blank starting paths before attempting filesystem access.</summary>
    private static void ValidateStartPath(string startPath)
    {
        if (string.IsNullOrWhiteSpace(startPath))
            throw new ArgumentException("Start path must not be blank.", nameof(startPath));
    }

    /// <summary>Rejects solution markers that cannot be matched safely by file name.</summary>
    private static void ValidateSolutionFileName(string solutionFileName)
    {
        if (string.IsNullOrWhiteSpace(solutionFileName))
            throw new ArgumentException("Solution file name must not be blank.", nameof(solutionFileName));
        if (Path.GetFileName(solutionFileName) != solutionFileName)
            throw new ArgumentException("Solution file name must be a file name, not a path.", nameof(solutionFileName));
    }

    /// <summary>Builds the human-readable repository marker used in diagnostics.</summary>
    private static string MarkerText(string solutionFileName, bool requireResDirectory) =>
        requireResDirectory ? $"{solutionFileName} and res" : solutionFileName;
}

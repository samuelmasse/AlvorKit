namespace AlvorKit.Script.Lint;

/// <summary>Resolved repository files selected by scoped lint include patterns.</summary>
/// <param name="AllFiles">Every existing file selected for lint checks.</param>
/// <param name="CSharpFiles">Selected C# files that can be checked by dotnet format.</param>
/// <param name="PrettierFiles">Selected files covered by the repository Prettier policy.</param>
/// <param name="ActionlintFiles">Selected GitHub Actions workflow files covered by actionlint.</param>
internal sealed record LintScope(
    IReadOnlyList<string> AllFiles,
    IReadOnlyList<string> CSharpFiles,
    IReadOnlyList<string> PrettierFiles,
    IReadOnlyList<string> ActionlintFiles)
{
    /// <summary>Prettier globs owned by the lint policy.</summary>
    public static IReadOnlyList<string> PrettierGlobPatterns { get; } =
    [
        ".github/workflows/*.yml",
        ".vscode/*.json",
        ".config/*.json",
        "native/**/*.yml",
        "*.md",
        "native/**/*.md",
        "src/**/*.md",
        "scripts/**/*.md",
        "demos/**/*.md",
    ];

    /// <summary>GitHub Actions workflow globs owned by actionlint.</summary>
    public static IReadOnlyList<string> ActionlintGlobPatterns { get; } = [".github/workflows/*.yml"];

    /// <summary>True when the scope contains no existing files to check.</summary>
    public bool IsEmpty => AllFiles.Count == 0;

    /// <summary>Expands include patterns into a classified lint file scope.</summary>
    public static LintScope FromPatterns(string repoRoot, IReadOnlyList<string> includePatterns)
    {
        var root = Path.GetFullPath(repoRoot);
        var files = includePatterns
            .SelectMany(pattern => ResolvePattern(root, pattern))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new(
            files,
            files.Where(IsCSharpFile).ToArray(),
            files.Where(IsPrettierFile).ToArray(),
            files.Where(IsActionlintFile).ToArray());
    }

    /// <summary>Resolves one include pattern to existing repository-relative files.</summary>
    private static IEnumerable<string> ResolvePattern(string repoRoot, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Scoped lint include patterns cannot be empty.");

        return HasGlobMeta(pattern)
            ? ResolveGlob(repoRoot, pattern)
            : ResolveLiteral(repoRoot, pattern);
    }

    /// <summary>Resolves an include glob by matching it against existing repository files.</summary>
    private static IEnumerable<string> ResolveGlob(string repoRoot, string pattern)
    {
        var normalizedPattern = GlobPattern.NormalizePath(pattern);
        if (Path.IsPathRooted(pattern) || normalizedPattern.Split('/').Contains("..", StringComparer.Ordinal))
            throw new ArgumentException($"Scoped lint glob '{pattern}' must stay inside the repository.");

        var regex = GlobPattern.ToRegex(normalizedPattern);
        return EnumerateFiles(repoRoot).Where(file => regex.IsMatch(file));
    }

    /// <summary>Resolves an include file or directory, skipping missing paths such as deleted files.</summary>
    private static IEnumerable<string> ResolveLiteral(string repoRoot, string pattern)
    {
        var fullPath = Path.GetFullPath(Path.Combine(repoRoot, pattern));
        EnsureInsideRepo(repoRoot, fullPath, pattern);

        if (File.Exists(fullPath))
            return [GlobPattern.NormalizePath(Path.GetRelativePath(repoRoot, fullPath))];

        return Directory.Exists(fullPath)
            ? EnumerateFiles(fullPath).Select(file => GlobPattern.NormalizePath(Path.GetRelativePath(repoRoot, Path.Combine(fullPath, file))))
            : [];
    }

    /// <summary>Enumerates files under a root while avoiding generated and tool-output directories.</summary>
    private static IEnumerable<string> EnumerateFiles(string root)
    {
        var stack = new Stack<string>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var directory = stack.Pop();
            foreach (var childDirectory in Directory.EnumerateDirectories(directory).Where(ShouldEnterDirectory))
                stack.Push(childDirectory);
            foreach (var file in Directory.EnumerateFiles(directory))
                yield return GlobPattern.NormalizePath(Path.GetRelativePath(root, file));
        }
    }

    /// <summary>Returns true when a directory is useful for lint input discovery.</summary>
    private static bool ShouldEnterDirectory(string path)
    {
        var name = Path.GetFileName(path);
        return !StringComparer.OrdinalIgnoreCase.Equals(name, ".git")
            && !StringComparer.OrdinalIgnoreCase.Equals(name, "bin")
            && !StringComparer.OrdinalIgnoreCase.Equals(name, "obj")
            && !StringComparer.OrdinalIgnoreCase.Equals(name, "out");
    }

    /// <summary>Rejects literal include paths outside the repository.</summary>
    private static void EnsureInsideRepo(string repoRoot, string fullPath, string originalPattern)
    {
        if (!IsInsideRepo(repoRoot, fullPath))
            throw new ArgumentException($"Scoped lint path '{originalPattern}' must stay inside the repository.");
    }

    /// <summary>Returns true when a full path is inside the repository root.</summary>
    private static bool IsInsideRepo(string repoRoot, string fullPath)
    {
        var root = TrimTrailingSeparators(Path.GetFullPath(repoRoot));
        var candidate = TrimTrailingSeparators(Path.GetFullPath(fullPath));
        return string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase)
            || candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Trims trailing directory separators so full paths compare consistently.</summary>
    private static string TrimTrailingSeparators(string path) =>
        path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    /// <summary>Returns true when a pattern contains supported glob metacharacters.</summary>
    private static bool HasGlobMeta(string pattern) =>
        pattern.Contains('*', StringComparison.Ordinal) || pattern.Contains('?', StringComparison.Ordinal);

    /// <summary>Returns true when a file is a C# source file.</summary>
    private static bool IsCSharpFile(string file) =>
        string.Equals(Path.GetExtension(file), ".cs", StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when Prettier owns formatting for the file.</summary>
    private static bool IsPrettierFile(string file) =>
        PrettierGlobPatterns.Any(glob => GlobPattern.Matches(file, glob));

    /// <summary>Returns true when actionlint owns validation for the file.</summary>
    private static bool IsActionlintFile(string file) =>
        ActionlintGlobPatterns.Any(glob => GlobPattern.Matches(file, glob));
}

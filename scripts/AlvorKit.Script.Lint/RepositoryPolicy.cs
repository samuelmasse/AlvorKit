namespace AlvorKit;

/// <summary>Checks repository files against policies shared by AlvorKit and its game repositories.</summary>
internal static class RepositoryPolicy
{
    /// <summary>Directory names excluded from repository policy discovery.</summary>
    private static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".vs",
        "bin",
        "dist",
        "node_modules",
        "obj",
        "out",
        "packages",
        "tmp",
    };

    /// <summary>Returns hand-authored assembly metadata files prohibited by the shared repository policy.</summary>
    public static IReadOnlyList<string> FindAssemblyInfoFiles(string repoRoot, LintScope? scope = null)
    {
        var files = scope is null
            ? EnumerateFiles(Path.GetFullPath(repoRoot))
            : scope.AllFiles;
        return files
            .Where(file => string.Equals(Path.GetFileName(file), "AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>Returns uses of the prohibited C# checked keyword in source and source templates.</summary>
    public static IReadOnlyList<RepositoryKeywordUsage> FindCheckedKeywordUsages(string repoRoot, LintScope? scope = null)
    {
        var root = Path.GetFullPath(repoRoot);
        var files = scope is null ? EnumerateFiles(root) : scope.AllFiles;
        return files
            .Where(IsCSharpSource)
            .SelectMany(file => FindCheckedKeywordUsages(root, file))
            .OrderBy(usage => usage.File, StringComparer.Ordinal)
            .ThenBy(usage => usage.Line)
            .ThenBy(usage => usage.Column)
            .ToArray();
    }

    /// <summary>Returns authored type declarations outside the repository's single root namespace.</summary>
    public static IReadOnlyList<string> FindNamespaceViolations(string repoRoot, LintScope? scope = null)
    {
        var root = Path.GetFullPath(repoRoot);
        var repositoryNamespace = FindRepositoryNamespace(root);
        if (repositoryNamespace is null)
            return [];

        var files = scope is null ? EnumerateFiles(root) : scope.AllFiles;
        return files
            .Where(file => file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .SelectMany(file => FindNamespaceViolations(root, file, repositoryNamespace))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>Returns structural violations in the canonical agent-policy graph when the repository owns one.</summary>
    public static IReadOnlyList<string> FindAgentPolicyViolations(string repoRoot) =>
        AgentPolicyGraph.FindViolations(repoRoot);

    /// <summary>Returns uses of the prohibited keyword in one repository-relative source file.</summary>
    private static IEnumerable<RepositoryKeywordUsage> FindCheckedKeywordUsages(string repoRoot, string file)
    {
        var source = File.ReadAllText(Path.Combine(repoRoot, file));
        var tree = CSharpSyntaxTree.ParseText(source, path: file);
        foreach (var token in tree.GetRoot().DescendantTokens())
        {
            if (token.RawKind != (int)SyntaxKind.CheckedKeyword)
                continue;

            var position = tree.GetLineSpan(token.Span).StartLinePosition;
            yield return new(file, position.Line + 1, position.Character + 1);
        }
    }

    /// <summary>Returns namespace violations in one C# source file.</summary>
    private static IEnumerable<string> FindNamespaceViolations(
        string repoRoot,
        string file,
        string repositoryNamespace)
    {
        var expectedNamespace = file.StartsWith("res/templates/new-game/source/", StringComparison.OrdinalIgnoreCase)
            ? "AlvorStarter"
            : repositoryNamespace;
        var source = File.ReadAllText(Path.Combine(repoRoot, file));
        var tree = CSharpSyntaxTree.ParseText(source, path: file);
        foreach (var declaration in tree.GetRoot().DescendantNodes().Where(IsTypeDeclaration))
        {
            var actualNamespace = NamespaceOf(declaration);
            if (actualNamespace == expectedNamespace || IsExternalNamespaceException(file, actualNamespace))
                continue;

            var position = tree.GetLineSpan(declaration.Span).StartLinePosition;
            var displayNamespace = string.IsNullOrEmpty(actualNamespace) ? "<global>" : actualNamespace;
            yield return $"{file}({position.Line + 1},{position.Character + 1}): " +
                $"namespace '{displayNamespace}' must be '{expectedNamespace}'";
        }
    }

    /// <summary>Returns the namespace surrounding one declaration.</summary>
    private static string NamespaceOf(SyntaxNode declaration) =>
        string.Join(
            ".",
            declaration.Ancestors()
                .OfType<BaseNamespaceDeclarationSyntax>()
                .Reverse()
                .Select(item => item.Name.ToString()));

    /// <summary>Returns true for syntax nodes that introduce authored CLR types.</summary>
    private static bool IsTypeDeclaration(SyntaxNode node) =>
        node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax;

    /// <summary>Returns the root namespace declared by the repository's primary solution.</summary>
    private static string? FindRepositoryNamespace(string repoRoot) =>
        Directory.EnumerateFiles(repoRoot, "*.slnx", SearchOption.TopDirectoryOnly)
            .Where(path => !path.EndsWith(".Dev.slnx", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileNameWithoutExtension)
            .Order(StringComparer.Ordinal)
            .FirstOrDefault();

    /// <summary>Allows the two intentional external contracts that must use namespaces owned by their hosts.</summary>
    private static bool IsExternalNamespaceException(string file, string actualNamespace) =>
        (actualNamespace == "System.Runtime.CompilerServices" &&
            (Path.GetFileName(file) is "IsExternalInit.cs" or "IgnoresAccessChecksToAttribute.cs"))
        || (actualNamespace == "AgentSubmissions" &&
            file.StartsWith("demos/AlvorKit.Engine.LiveCode.Demo/Submissions/", StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns true for C# files and repository templates that emit C# source.</summary>
    private static bool IsCSharpSource(string file) =>
        file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
        || file.EndsWith(".cs.tmpl", StringComparison.OrdinalIgnoreCase)
        || file.EndsWith(".csfrag.tmpl", StringComparison.OrdinalIgnoreCase);

    /// <summary>Enumerates repository-relative files while skipping generated, dependency, and tool-output directories.</summary>
    private static IEnumerable<string> EnumerateFiles(string repoRoot)
    {
        var directories = new Stack<string>();
        directories.Push(repoRoot);

        while (directories.Count > 0)
        {
            var directory = directories.Pop();
            foreach (var childDirectory in Directory.EnumerateDirectories(directory).Where(ShouldEnterDirectory))
                directories.Push(childDirectory);
            foreach (var file in Directory.EnumerateFiles(directory))
                yield return GlobPattern.NormalizePath(Path.GetRelativePath(repoRoot, file));
        }
    }

    /// <summary>Returns true when repository policy discovery should enter a directory.</summary>
    private static bool ShouldEnterDirectory(string path)
    {
        var attributes = File.GetAttributes(path);
        return !ExcludedDirectories.Contains(Path.GetFileName(path))
            && !attributes.HasFlag(FileAttributes.ReparsePoint);
    }
}

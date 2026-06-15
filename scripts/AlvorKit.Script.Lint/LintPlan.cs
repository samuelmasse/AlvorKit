namespace AlvorKit.Script.Lint;

/// <summary>Builds the external tool command plan for repository linting.</summary>
internal static class LintPlan
{
    /// <summary>Repository project roots that participate in dotnet format checks.</summary>
    private static readonly string[] ProjectRoots = ["src", "scripts", "tests"];

    /// <summary>Prettier globs owned by the lint policy.</summary>
    private static readonly string[] PrettierGlobs =
    [
        ".github/workflows/*.yml",
        ".vscode/*.json",
        ".config/*.json",
        "native/**/*.json",
        "*.md",
        "native/**/*.md",
        "src/**/*.md",
        "scripts/**/*.md",
        "demos/**/*.md",
    ];

    /// <summary>Creates lint commands that can run before actionlint is available.</summary>
    public static IReadOnlyList<CommandSpec> CommandsBeforeActionlint(string repoRoot, bool fix) =>
    [
        .. DotNetFormatCommands(repoRoot, fix),
        PrettierCommand(repoRoot, fix),
        EditorConfigCommand(repoRoot),
    ];

    /// <summary>Creates dotnet format commands for source, script, and test projects.</summary>
    public static IReadOnlyList<CommandSpec> DotNetFormatCommands(string repoRoot, bool fix) =>
        DiscoverProjects(repoRoot)
            .Select(project => new CommandSpec("dotnet", DotNetFormatArguments(project, fix), repoRoot, $"dotnet format {project}"))
            .ToArray();

    /// <summary>Creates the Prettier command for JSON, YAML, and Markdown files.</summary>
    public static CommandSpec PrettierCommand(string repoRoot, bool fix) =>
        new(ToolExecutable.Npx(), ["--yes", "prettier@3", fix ? "--write" : "--check", .. PrettierGlobs], repoRoot, "prettier");

    /// <summary>Creates the EditorConfig checker command for repo-wide whitespace and line-length rules.</summary>
    public static CommandSpec EditorConfigCommand(string repoRoot) =>
        new(ToolExecutable.Npx(), ["--yes", "editorconfig-checker@6.1.1", "-disable-indentation", "-format", "github-actions"], repoRoot, "editorconfig");

    /// <summary>Creates the actionlint command for GitHub Actions workflow files.</summary>
    public static CommandSpec ActionlintCommand(string repoRoot, string actionlintPath) =>
        new(actionlintPath, ["-color"], repoRoot, "actionlint");

    /// <summary>Discovers project files under the linted source roots.</summary>
    public static IReadOnlyList<string> DiscoverProjects(string repoRoot) =>
        ProjectRoots
            .Select(root => Path.Combine(repoRoot, root))
            .Where(Directory.Exists)
            .SelectMany(root => Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories))
            .Where(project => !project.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(project => !project.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Select(project => Normalize(Path.GetRelativePath(repoRoot, project)))
            .Order(StringComparer.Ordinal)
            .ToArray();

    /// <summary>Creates dotnet format arguments for a single project.</summary>
    private static IReadOnlyList<string> DotNetFormatArguments(string project, bool fix) =>
        fix
            ? ["format", project, "--verbosity", "minimal"]
            : ["format", project, "--verify-no-changes", "--verbosity", "minimal"];

    /// <summary>Normalizes paths for stable command output across platforms.</summary>
    private static string Normalize(string path) =>
        path.Replace(Path.DirectorySeparatorChar, '/');
}

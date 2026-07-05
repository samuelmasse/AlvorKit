namespace AlvorKit.Script.Lint;

/// <summary>Builds the external tool command plan for repository linting.</summary>
internal static class LintPlan
{
    /// <summary>Maximum number of scoped files to pass to one EditorConfig checker process.</summary>
    private const int EditorConfigFileBatchSize = 80;

    /// <summary>Creates lint commands that can run before actionlint is available.</summary>
    public static IReadOnlyList<CommandSpec> CommandsBeforeActionlint(string repoRoot, bool fix) =>
    [
        .. DotNetFormatCommands(repoRoot, fix),
        PrettierCommand(repoRoot, fix),
        EditorConfigCommand(repoRoot),
    ];

    /// <summary>Creates scoped lint commands that can run before actionlint is available.</summary>
    public static IReadOnlyList<CommandSpec> CommandsBeforeActionlint(string repoRoot, bool fix, LintScope scope)
    {
        if (scope.IsEmpty)
            return [];

        var commands = new List<CommandSpec>();
        commands.AddRange(DotNetFormatCommands(repoRoot, fix, scope));

        var prettierCommand = PrettierCommand(repoRoot, fix, scope);
        if (prettierCommand is not null)
            commands.Add(prettierCommand);

        commands.AddRange(EditorConfigCommands(repoRoot, scope.AllFiles));
        return commands;
    }

    /// <summary>Creates a solution-wide dotnet format command for full repository linting.</summary>
    public static IReadOnlyList<CommandSpec> DotNetFormatCommands(string repoRoot, bool fix) =>
        DotNetFormatCommands(FindSolutionFile(repoRoot), repoRoot, fix);

    /// <summary>Creates dotnet format commands for the scoped C# files grouped by owning project.</summary>
    public static IReadOnlyList<CommandSpec> DotNetFormatCommands(string repoRoot, bool fix, LintScope scope) =>
        scope.CSharpFiles
            .GroupBy(file => FindOwningProject(repoRoot, file), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .SelectMany(group => DotNetFormatCommands(group.Key, repoRoot, fix, group.Order(StringComparer.Ordinal).ToArray()))
            .ToArray();

    /// <summary>
    /// Creates the Prettier command for JSON, YAML, and Markdown files; unmatched globs are tolerated
    /// so the plan works in repos without every directory.
    /// </summary>
    public static CommandSpec PrettierCommand(string repoRoot, bool fix) =>
        new(
            ToolExecutable.Npx(),
            ["--yes", "prettier@3", fix ? "--write" : "--check", "--no-error-on-unmatched-pattern", .. LintScope.PrettierGlobPatterns],
            repoRoot,
            "prettier");

    /// <summary>Creates the Prettier command for scoped JSON, YAML, and Markdown files.</summary>
    public static CommandSpec? PrettierCommand(string repoRoot, bool fix, LintScope scope) =>
        scope.PrettierFiles.Count == 0
            ? null
            : new(ToolExecutable.Npx(), ["--yes", "prettier@3", fix ? "--write" : "--check", .. scope.PrettierFiles], repoRoot, "prettier");

    /// <summary>Creates the EditorConfig checker command for repo-wide whitespace and line-length rules.</summary>
    public static CommandSpec EditorConfigCommand(string repoRoot) =>
        new(ToolExecutable.Npx(), ["--yes", "editorconfig-checker@6.1.1", "-disable-indentation", "-format", "github-actions"], repoRoot, "editorconfig");

    /// <summary>Creates the EditorConfig checker command for scoped whitespace and line-length rules.</summary>
    public static CommandSpec EditorConfigCommand(string repoRoot, IReadOnlyList<string> files) =>
        new(ToolExecutable.Npx(), ["--yes", "editorconfig-checker@6.1.1", "-disable-indentation", "-format", "github-actions", .. files], repoRoot, "editorconfig");

    /// <summary>Creates scoped EditorConfig checker commands, batching file arguments for Windows command-line limits.</summary>
    public static IReadOnlyList<CommandSpec> EditorConfigCommands(string repoRoot, IReadOnlyList<string> files) =>
        files
            .Chunk(EditorConfigFileBatchSize)
            .Select(batch => EditorConfigCommand(repoRoot, batch))
            .ToArray();

    /// <summary>Creates the actionlint command for GitHub Actions workflow files.</summary>
    public static CommandSpec ActionlintCommand(string repoRoot, string actionlintPath, LintScope? scope = null) =>
        new(actionlintPath, scope is null ? ["-color"] : ["-color", .. scope.ActionlintFiles], repoRoot, "actionlint");

    /// <summary>Returns true when actionlint should run for the selected scope.</summary>
    public static bool RequiresActionlint(LintScope? scope) =>
        scope is null || scope.ActionlintFiles.Count > 0;

    /// <summary>Creates the normal and info-level code style dotnet format commands for a single project.</summary>
    private static IReadOnlyList<CommandSpec> DotNetFormatCommands(
        string project,
        string repoRoot,
        bool fix,
        IReadOnlyList<string>? includedFiles = null) =>
    [
        new("dotnet", DotNetFormatArguments(project, fix, includedFiles), repoRoot, $"dotnet format {project}"),
        new("dotnet", DotNetStyleInfoArguments(project, fix, includedFiles), repoRoot, $"dotnet format style {project}"),
    ];

    /// <summary>Creates normal dotnet format arguments for a project or solution.</summary>
    private static IReadOnlyList<string> DotNetFormatArguments(string projectOrSolution, bool fix, IReadOnlyList<string>? includedFiles = null)
    {
        var arguments = new List<string> { "format", projectOrSolution };
        if (!fix)
            arguments.Add("--verify-no-changes");
        arguments.AddRange(["--verbosity", "minimal"]);
        if (includedFiles is { Count: > 0 })
            arguments.AddRange(["--include", .. includedFiles]);
        return arguments;
    }

    /// <summary>Creates info-level code style dotnet format arguments for a single project.</summary>
    private static IReadOnlyList<string> DotNetStyleInfoArguments(string project, bool fix, IReadOnlyList<string>? includedFiles = null)
    {
        var arguments = new List<string> { "format", "style", project };
        if (!fix)
            arguments.Add("--verify-no-changes");
        arguments.AddRange(["--severity", "info", "--verbosity", "minimal"]);
        if (includedFiles is { Count: > 0 })
            arguments.AddRange(["--include", .. includedFiles]);
        return arguments;
    }

    /// <summary>Finds the single solution file at the repository root used for full C# formatting checks.</summary>
    private static string FindSolutionFile(string repoRoot)
    {
        var solutions = Directory.GetFiles(Path.GetFullPath(repoRoot), "*.slnx", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(Path.GetFullPath(repoRoot), "*.sln", SearchOption.TopDirectoryOnly))
            .Select(path => Path.GetFileName(path)!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return solutions switch
        {
            [var single] => single,
            [] => throw new InvalidOperationException($"No solution file found at '{repoRoot}' for repo-wide dotnet format."),
            _ => throw new InvalidOperationException($"Multiple solution files found at '{repoRoot}'; repo-wide lint expects exactly one."),
        };
    }

    /// <summary>Finds the nearest project file that owns a scoped C# source file.</summary>
    private static string FindOwningProject(string repoRoot, string file)
    {
        var root = Path.GetFullPath(repoRoot);
        var directory = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(root, file)))!;

        while (directory.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            var projects = Directory.GetFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly).Order(StringComparer.Ordinal).ToArray();
            if (projects.Length == 1)
                return Normalize(Path.GetRelativePath(root, projects[0]));
            if (projects.Length > 1)
                throw new InvalidOperationException($"Scoped lint file '{file}' is under multiple project files in '{directory}'.");

            if (string.Equals(directory, root, StringComparison.OrdinalIgnoreCase))
                break;
            directory = Directory.GetParent(directory)!.FullName;
        }

        throw new InvalidOperationException($"Scoped lint file '{file}' is not under a project file.");
    }

    /// <summary>Normalizes paths for stable command output across platforms.</summary>
    private static string Normalize(string path) =>
        path.Replace(Path.DirectorySeparatorChar, '/');
}

namespace AlvorKit.Script.NewGame;

/// <summary>Loads and parameterizes the concrete starter game that backs new-game generation.</summary>
internal sealed class NewGameStarterProject
{
    /// <summary>Concrete project and namespace name used in the starter source tree.</summary>
    private const string SourceName = "AlvorStarter";
    /// <summary>Concrete display title used in the starter source tree.</summary>
    private const string SourceTitle = "Alvor Starter";
    /// <summary>Non-solution reference file kept beside the starter source for humans, not copied into games.</summary>
    private const string SourceSolutionTemplateFile = SourceName + ".slnx.template";
    /// <summary>Relative AlvorKit path used by the starter source when built in place.</summary>
    private const string SourceAlvorKitRelativePath = @"..\..\..\..";

    /// <summary>Text extensions that receive starter-name substitutions.</summary>
    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs",
        ".csproj",
        ".editorconfig",
        ".json",
        ".md",
        ".props",
        ".ps1",
        ".targets",
        ".template",
        ".toml",
        ".txt",
        ".xml",
        ".yaml",
        ".yml",
    };

    /// <summary>Extensionless text files that receive starter-name substitutions.</summary>
    private static readonly HashSet<string> TextFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".gitattributes",
        ".gitignore",
        "LICENSE",
    };

    /// <summary>Loads every copied file from the starter source tree in deterministic order.</summary>
    public IReadOnlyList<NewGameSourceFile> ReadFiles()
    {
        var sourceRoot = SourceRoot();
        return Directory
            .EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
            .Select(path => (Path: path, RelativePath: RelativePath(sourceRoot, path)))
            .Where(IsSourceFile)
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .Select(file => new NewGameSourceFile(
                file.RelativePath,
                File.ReadAllBytes(file.Path),
                IsTextFile(file.RelativePath)))
            .ToArray();
    }

    /// <summary>Renders a starter source path for a generated game repository.</summary>
    public string RenderPath(string relativePath, NewGameOptions options) =>
        relativePath.Replace(SourceName, options.Name.Identifier, StringComparison.Ordinal);

    /// <summary>Renders starter source content for a generated game repository.</summary>
    public string RenderText(NewGameSourceFile file, NewGameOptions options)
    {
        if (!file.IsText)
            throw new InvalidOperationException($"File '{file.RelativePath}' is not a text file.");

        return Encoding.UTF8.GetString(file.Bytes)
            .Replace(SourceTitle, options.Name.Title, StringComparison.Ordinal)
            .Replace(SourceName, options.Name.Identifier, StringComparison.Ordinal)
            .Replace(SourceAlvorKitRelativePath, AlvorKitRelativePath(options), StringComparison.Ordinal);
    }

    /// <summary>Returns the generated repository solution file name.</summary>
    public static string SolutionPath(NewGameOptions options) =>
        options.Name.Identifier + ".slnx";

    /// <summary>Generates a solution from the concrete starter project's current project files.</summary>
    public string RenderSolution(NewGameOptions options)
    {
        var projects = SolutionProjects(options);
        var builder = new StringBuilder();
        builder.AppendLine("<Solution>");
        foreach (var project in projects)
        {
            builder.Append("    <Project Path=\"");
            builder.Append(project.Path);
            builder.Append('"');
            if (project.IsStartup)
                builder.Append(" DefaultStartup=\"true\"");
            builder.AppendLine(" />");
        }

        builder.AppendLine("</Solution>");
        return builder.ToString();
    }

    /// <summary>Resolves the concrete starter source directory under AlvorKit resources.</summary>
    private static string SourceRoot() =>
        Path.Combine(ProjectRoot.ResDirectory(typeof(NewGameStarterProject)), "templates", "new-game", "source");

    /// <summary>Converts a filesystem path to a repository-style path relative to the starter source root.</summary>
    private static string RelativePath(string sourceRoot, string path) =>
        Path.GetRelativePath(sourceRoot, path).Replace('\\', '/');

    /// <summary>Filters local build and tooling output from a buildable starter project tree.</summary>
    private static bool IsSourceFile((string Path, string RelativePath) file) =>
        file.RelativePath != SourceSolutionTemplateFile &&
        !file.RelativePath.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase) &&
        file.RelativePath.Split('/').All(segment => segment is not "bin" and not "obj" and not "out" and not ".git" and not ".vs");

    /// <summary>Returns whether the source file should be interpreted as UTF-8 text.</summary>
    private static bool IsTextFile(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath);
        return TextFileNames.Contains(fileName) || TextExtensions.Contains(Path.GetExtension(relativePath));
    }

    /// <summary>Computes the generated repository's relative reference back to AlvorKit.</summary>
    private static string AlvorKitRelativePath(NewGameOptions options) =>
        Path.GetRelativePath(options.OutputPath, options.AlvorKitRoot).Replace('/', '\\');

    /// <summary>Builds solution project entries from the current starter project files.</summary>
    private IReadOnlyList<NewGameSolutionProject> SolutionProjects(NewGameOptions options)
    {
        var sourceRoot = SourceRoot();
        var projects = Directory
            .EnumerateFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories)
            .Select(path => (Path: path, RelativePath: RelativePath(sourceRoot, path)))
            .Where(IsSourceFile)
            .Select(file => new NewGameSolutionProject(
                RenderPath(file.RelativePath, options),
                IsStartupProject(file.Path)))
            .OrderBy(project => !project.IsStartup)
            .ThenBy(project => project.Path, StringComparer.Ordinal)
            .ToArray();

        var startupCount = projects.Count(project => project.IsStartup);
        if (startupCount == 0)
            throw new InvalidOperationException("Starter project must contain one executable project for generated solution startup.");
        if (startupCount > 1)
            throw new InvalidOperationException("Starter project must contain only one executable project for generated solution startup.");

        return projects;
    }

    /// <summary>Detects the executable project that should become the generated solution startup project.</summary>
    private static bool IsStartupProject(string projectPath) =>
        File.ReadAllText(projectPath, Encoding.UTF8).Contains("<OutputType>Exe</OutputType>", StringComparison.OrdinalIgnoreCase);

    /// <summary>One generated solution project entry.</summary>
    private sealed record NewGameSolutionProject(string Path, bool IsStartup);
}

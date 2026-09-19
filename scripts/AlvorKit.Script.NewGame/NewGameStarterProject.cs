namespace AlvorKit;

/// <summary>Loads and parameterizes the concrete starter game that backs new-game generation.</summary>
internal sealed class NewGameStarterProject
{
    /// <summary>Concrete project and namespace name used in the starter source tree.</summary>
    private const string SourceName = "AlvorStarter";
    /// <summary>Concrete display title used in the starter source tree.</summary>
    private const string SourceTitle = "Alvor Starter";
    /// <summary>Inert agent-instruction payload emitted with its active repository filename.</summary>
    private const string SourceAgentsTemplateFile = "AGENTS.md.template";
    /// <summary>Relative AlvorKit path used by the starter source when built in place.</summary>
    private const string SourceAlvorKitRelativePath = @"..\..\..\..";
    /// <summary>Relative AlvorKit path emitted into generated sibling game repositories.</summary>
    private const string GeneratedAlvorKitRelativePath = @"..\AlvorKit";
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
        relativePath == SourceAgentsTemplateFile
            ? "AGENTS.md"
            : relativePath.Replace(SourceName, options.Name.Identifier, StringComparison.Ordinal);

    /// <summary>Renders starter source content for a generated game repository.</summary>
    public string RenderText(NewGameSourceFile file, NewGameOptions options)
    {
        if (!file.IsText)
            throw new InvalidOperationException($"File '{file.RelativePath}' is not a text file.");

        return Encoding.UTF8.GetString(file.Bytes)
            .Replace(SourceTitle, options.Name.Title, StringComparison.Ordinal)
            .Replace(SourceName, options.Name.Identifier, StringComparison.Ordinal)
            .Replace(SourceAlvorKitRelativePath, GeneratedAlvorKitRelativePath, StringComparison.Ordinal);
    }

    /// <summary>Resolves the concrete starter source directory under AlvorKit resources.</summary>
    private static string SourceRoot() =>
        Path.Combine(ProjectRoot.ResDirectory(typeof(NewGameStarterProject)), "templates", "new-game", "source");

    /// <summary>Converts a filesystem path to a repository-style path relative to the starter source root.</summary>
    private static string RelativePath(string sourceRoot, string path) =>
        Path.GetRelativePath(sourceRoot, path).Replace('\\', '/');

    /// <summary>Filters local build and tooling output from a buildable starter project tree.</summary>
    private static bool IsSourceFile((string Path, string RelativePath) file) =>
        !file.RelativePath.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase) &&
        file.RelativePath.Split('/').All(segment => segment is not "bin" and not "obj" and not "out" and not ".git" and not ".vs");

    /// <summary>Returns whether the source file should be interpreted as UTF-8 text.</summary>
    private static bool IsTextFile(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath);
        return TextFileNames.Contains(fileName) || TextExtensions.Contains(Path.GetExtension(relativePath));
    }
}

namespace AlvorKit.Script.ZoneSolution;

/// <summary>Formats generated VS Code workspace JSON for the selected sibling repositories.</summary>
internal static class CodeWorkspaceDocument
{
    /// <summary>JSON serializer settings for deterministic workspace output.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>Creates a VS Code workspace document rooted at the generated solution directory.</summary>
    /// <param name="repositories">Repositories included in the generated solution.</param>
    /// <param name="workspaceDirectory">Directory where the workspace file will be written.</param>
    /// <param name="solutionPath">Generated solution path referenced by the workspace settings.</param>
    public static string Build(
        IReadOnlyList<ZoneRepository> repositories,
        string workspaceDirectory,
        string solutionPath)
    {
        var document = new CodeWorkspace(
            repositories
                .Select(repository => new CodeWorkspaceFolder(RelativeWorkspacePath(workspaceDirectory, repository.RootPath)))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["dotnet.defaultSolution"] = RelativeWorkspacePath(workspaceDirectory, solutionPath),
            });

        return JsonSerializer.Serialize(document, Options) + Environment.NewLine;
    }

    /// <summary>Returns a slash-separated path relative to the generated workspace file.</summary>
    private static string RelativeWorkspacePath(string workspaceDirectory, string path) =>
        PathText.ToSlnxPath(Path.GetRelativePath(workspaceDirectory, path));

    /// <summary>Root JSON shape for a VS Code workspace file.</summary>
    private sealed record CodeWorkspace(
        IReadOnlyList<CodeWorkspaceFolder> Folders,
        IReadOnlyDictionary<string, string> Settings);

    /// <summary>Folder entry JSON shape for a VS Code workspace file.</summary>
    private sealed record CodeWorkspaceFolder(string Path);
}

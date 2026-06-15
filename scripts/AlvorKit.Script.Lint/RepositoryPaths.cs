using AlvorKit.Script.Workspace;

namespace AlvorKit.Script.Lint;

/// <summary>Locates repository-level paths used by the lint coordinator.</summary>
internal static class RepositoryPaths
{
    /// <summary>Finds the repository root by walking up from the current directory or supplied start path.</summary>
    public static string FindRoot(string? startPath = null) =>
        ProjectRoot.FindFrom(startPath ?? Directory.GetCurrentDirectory());
}

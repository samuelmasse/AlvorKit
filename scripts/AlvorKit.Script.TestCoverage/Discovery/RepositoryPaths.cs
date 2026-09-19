namespace AlvorKit;

/// <summary>Finds repository-relative paths used by the coverage tool.</summary>
internal static class RepositoryPaths
{
    /// <summary>Finds the Git checkout from the working directory or executable location.</summary>
    public static string FindRoot() =>
        RepositoryRoot.FindFromCurrentProcess(typeof(RepositoryPaths));

    /// <summary>Converts an absolute path under the repository into a slash-separated relative path.</summary>
    public static string Relative(string repoRoot, string path) =>
        Path.GetRelativePath(repoRoot, path).Replace('\\', '/');
}

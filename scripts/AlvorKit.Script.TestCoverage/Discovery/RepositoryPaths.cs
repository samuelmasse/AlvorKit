namespace AlvorKit.Script.TestCoverage;

/// <summary>Finds repository-relative paths used by the coverage tool.</summary>
internal static class RepositoryPaths
{
    /// <summary>Walks up from the executable location until the repository solution file is found.</summary>
    public static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlvorKit.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find repository root.");
    }

    /// <summary>Converts an absolute path under the repository into a slash-separated relative path.</summary>
    public static string Relative(string repoRoot, string path) =>
        Path.GetRelativePath(repoRoot, path).Replace('\\', '/');
}

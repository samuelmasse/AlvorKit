namespace AlvorKit.Script.Lint;

/// <summary>Locates repository-level paths used by the lint coordinator.</summary>
internal static class RepositoryPaths
{
    /// <summary>Finds the repository root by walking up from the current directory or supplied start path.</summary>
    public static string FindRoot(string? startPath = null)
    {
        var directory = new DirectoryInfo(startPath ?? Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlvorKit.slnx")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find AlvorKit.slnx in the current directory or any parent.");
    }
}

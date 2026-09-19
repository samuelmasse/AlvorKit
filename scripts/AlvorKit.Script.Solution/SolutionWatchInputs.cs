namespace AlvorKit;

/// <summary>Filters workspace invalidations while ignoring ordinary source edits and generated build output.</summary>
internal static class SolutionWatchInputs
{
    /// <summary>Includes graph inputs and directory moves, including active generated project roots.</summary>
    public static bool Relevant(string watchRoot, string path, bool directory)
    {
        var relative = SolutionPaths.Relative(watchRoot, path);
        var parts = relative.Split('/');

        if (directory && parts.Length == 2 && parts[1] == ".git")
            return true;

        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];

            if (part.Equals("out", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 == parts.Length)
                    return true;

                if (parts[index + 1] is not "bindgen" and not "mathgen")
                    return false;

                continue;
            }

            if (RepositoryProjects.IsExcludedDirectory(part))
                return false;
        }

        var name = Path.GetFileName(path);
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return directory || extension is ".csproj" or ".props" or ".targets" or ".proj" or ".xml"
            || name is "global.json" or "NuGet.Config" or "nuget.config"
            || name == ".git";
    }
}

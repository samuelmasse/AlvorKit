namespace AlvorKit.Engine;

/// <summary>Resolves root asset files by direct path or by name under a repository or application <c>res</c> directory.</summary>
internal static class RootAssetFiles
{
    /// <summary>Returns a usable path for an asset file, including root <c>res</c> fallback for simple names.</summary>
    internal static string Resolve(string file)
    {
        if (Path.IsPathRooted(file) || File.Exists(file))
            return file;

        var path = FindInRes(Environment.CurrentDirectory, file);
        if (path is not null)
            return path;

        path = FindInRes(AppContext.BaseDirectory, file);
        return path ?? file;
    }

    private static string? FindInRes(string start, string file)
    {
        var directory = new DirectoryInfo(start);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "res", file);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        return null;
    }
}

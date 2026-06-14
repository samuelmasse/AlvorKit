namespace AlvorKit.Script.NativeBuild;

/// <summary>Resolves symlink-style outputs before copying CMake products.</summary>
internal static class FileLinkResolver
{
    /// <summary>Returns the final linked target when the file is a link, otherwise the original path.</summary>
    public static string ResolveFile(string path)
    {
        var info = new FileInfo(path);
        return info.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? path;
    }
}

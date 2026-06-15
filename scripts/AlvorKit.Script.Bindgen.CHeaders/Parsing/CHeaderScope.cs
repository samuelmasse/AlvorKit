namespace AlvorKit.Script.Bindgen;

/// <summary>Determines whether Clang declarations belong to the requested source or shim roots.</summary>
internal sealed class CHeaderScope(string filterRoot, string libraryDirectory, string translationUnitPath)
{
    /// <summary>Normalized directory containing source declarations.</summary>
    private string SourceRoot { get; } = NormalizeDirectory(filterRoot);

    /// <summary>Normalized directory containing shim declarations.</summary>
    private string ShimRoot { get; } = NormalizeDirectory(libraryDirectory);

    /// <summary>Full path to the generated translation unit.</summary>
    private string TranslationUnitFullPath { get; } = Path.GetFullPath(translationUnitPath);

    /// <summary>Returns true when a source location belongs to the binding input surface.</summary>
    public bool IsInScope(CXSourceLocation location)
    {
        location.GetExpansionLocation(out var file, out _, out _, out _);
        var fileName = file.Name.ToString();
        if (fileName.Length == 0)
            return false;

        var path = Path.GetFullPath(fileName);
        return IsUnder(path, SourceRoot)
            || IsUnder(path, ShimRoot)
            || string.Equals(path, TranslationUnitFullPath, PathComparison);
    }

    /// <summary>Compares paths with the active platform's case-sensitivity rules.</summary>
    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    /// <summary>Normalizes a directory for prefix checks.</summary>
    private static string NormalizeDirectory(string directory) =>
        Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        + Path.DirectorySeparatorChar;

    /// <summary>Returns true when a path starts below a normalized directory.</summary>
    private static bool IsUnder(string path, string normalizedDirectory) =>
        path.StartsWith(normalizedDirectory, PathComparison);
}

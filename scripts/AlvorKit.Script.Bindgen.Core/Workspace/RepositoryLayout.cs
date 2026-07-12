namespace AlvorKit.Script.Bindgen;

/// <summary>Finds repository-level directories used by the bindgen scripts.</summary>
public sealed class RepositoryLayout
{
    /// <summary>Creates a repository layout rooted at the detected solution directory.</summary>
    private RepositoryLayout(string root)
    {
        Root = root;
        NativeDirectory = Path.Combine(root, "native");
    }

    /// <summary>Absolute repository root directory.</summary>
    public string Root { get; }

    /// <summary>Absolute repository native package directory.</summary>
    public string NativeDirectory { get; }

    /// <summary>Absolute non-active output root used when bindgen is run without <c>--output-root</c>.</summary>
    public string DefaultGeneratedOutputRoot =>
        Path.Combine(Root, "out", "generated", "bindgen");

    /// <summary>Resolves a generated-output root and requires it to stay under the repository out directory.</summary>
    public string ResolveGeneratedOutputRoot(string? outputRoot)
    {
        var resolved = string.IsNullOrWhiteSpace(outputRoot)
            ? Path.GetFullPath(DefaultGeneratedOutputRoot)
            : Path.GetFullPath(Path.Combine(Root, outputRoot));
        var outRoot = Path.GetFullPath(Path.Combine(Root, "out"));
        if (!IsInsideOrEqual(resolved, outRoot))
            throw new InvalidOperationException("--output-root must resolve inside the repository out directory.");

        return resolved;
    }

    /// <summary>Walks upward from a start directory until the AlvorKit solution file is found.</summary>
    public static RepositoryLayout FindFrom(string startDirectory) =>
        new(ProjectRoot.FindFrom(startDirectory));

    /// <summary>Returns a requested library or every configured binding with an active revision marker.</summary>
    public IEnumerable<string> SelectedLibraries(string selection)
    {
        var libraries = BindgenLibraries().ToArray();
        if (selection == "all")
            return libraries.Where(HasBindingRevision);

        var selected = libraries.FirstOrDefault(library => string.Equals(library, selection, StringComparison.OrdinalIgnoreCase));
        return selected is not null
            ? [selected]
            : throw new InvalidOperationException($"Unknown bindgen library '{selection}'. Known libraries: {string.Join(", ", libraries)}");
    }

    /// <summary>Returns every native library with bindgen metadata in sorted order.</summary>
    private IEnumerable<string> BindgenLibraries() =>
        Directory.Exists(NativeDirectory)
            ? Directory.GetDirectories(NativeDirectory)
                .Where(directory => RepositoryConfigFile.Find(Path.Combine(directory, "conf"), "bindgen") is not null)
                .Select(Path.GetFileName)
                .OfType<string>()
                .Order(StringComparer.OrdinalIgnoreCase)
            : [];

    /// <summary>Returns whether a configured library has been activated for aggregate generation.</summary>
    private bool HasBindingRevision(string library) =>
        File.Exists(Path.Combine(NativeDirectory, library, "version", "BINDING_REVISION"));

    /// <summary>Returns true when a resolved path is the expected directory or one of its descendants.</summary>
    private static bool IsInsideOrEqual(string path, string directory)
    {
        var relative = Path.GetRelativePath(directory, path);
        return relative == "."
            || (relative != ".."
                && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relative));
    }
}

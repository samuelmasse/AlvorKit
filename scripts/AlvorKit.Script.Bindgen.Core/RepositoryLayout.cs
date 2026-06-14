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

    /// <summary>Walks upward from a start directory until the AlvorKit solution file is found.</summary>
    public static RepositoryLayout FindFrom(string startDirectory) =>
        File.Exists(Path.Combine(startDirectory, "AlvorKit.slnx"))
            ? new(startDirectory)
            : FindFrom(Path.GetDirectoryName(startDirectory)
                ?? throw new InvalidOperationException("AlvorKit.slnx not found above " + startDirectory));

    /// <summary>Returns either a requested library name or every native library with bindgen metadata.</summary>
    public IEnumerable<string> SelectedLibraries(string selection)
    {
        var libraries = BindgenLibraries().ToArray();
        if (selection == "all")
            return libraries;

        var selected = libraries.FirstOrDefault(library => string.Equals(library, selection, StringComparison.OrdinalIgnoreCase));
        return selected is not null
            ? [selected]
            : throw new InvalidOperationException($"Unknown bindgen library '{selection}'. Known libraries: {string.Join(", ", libraries)}");
    }

    /// <summary>Returns every native library with bindgen metadata in sorted order.</summary>
    private IEnumerable<string> BindgenLibraries() =>
        Directory.Exists(NativeDirectory)
            ? Directory.GetDirectories(NativeDirectory)
                .Where(directory => File.Exists(Path.Combine(directory, "conf", "bindgen.json")))
                .Select(Path.GetFileName)
                .OfType<string>()
                .Order(StringComparer.OrdinalIgnoreCase)
            : [];
}

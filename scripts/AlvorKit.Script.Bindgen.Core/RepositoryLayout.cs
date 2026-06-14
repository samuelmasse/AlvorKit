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

    public string Root { get; }
    public string NativeDirectory { get; }

    /// <summary>Walks upward from a start directory until the AlvorKit solution file is found.</summary>
    public static RepositoryLayout FindFrom(string startDirectory)
    {
        var current = startDirectory;
        while (!File.Exists(Path.Combine(current, "AlvorKit.slnx")))
            current = Path.GetDirectoryName(current)
                ?? throw new InvalidOperationException("AlvorKit.slnx not found above " + startDirectory);
        return new(current);
    }

    /// <summary>Returns either a requested library name or every native library with bindgen metadata.</summary>
    public IEnumerable<string> SelectedLibraries(string selection)
    {
        if (selection != "all")
            return [selection];

        return Directory.GetDirectories(NativeDirectory)
            .Where(directory => File.Exists(Path.Combine(directory, "bindgen.json")))
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order();
    }
}

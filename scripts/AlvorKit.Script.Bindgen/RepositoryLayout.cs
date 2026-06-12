namespace AlvorKit.Script.Bindgen;

public sealed class RepositoryLayout
{
    private RepositoryLayout(string root)
    {
        Root = root;
        NativeDirectory = Path.Combine(root, "native");
    }

    public string Root { get; }
    public string NativeDirectory { get; }

    public static RepositoryLayout FindFrom(string startDirectory)
    {
        var current = startDirectory;
        while (!File.Exists(Path.Combine(current, "AlvorKit.slnx")))
            current = Path.GetDirectoryName(current)
                ?? throw new InvalidOperationException("AlvorKit.slnx not found above " + startDirectory);
        return new(current);
    }

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

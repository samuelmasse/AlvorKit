namespace AlvorKit.Script.NativeBuild;

/// <summary>Repository paths needed by the native build runner.</summary>
/// <param name="root">Absolute path to the repository root.</param>
internal sealed class RepositoryLayout(string root)
{
    /// <summary>Absolute path to the repository root.</summary>
    public string Root { get; } = root;

    /// <summary>Absolute path to the native package metadata directory.</summary>
    public string NativeDirectory => Path.Combine(Root, "native");

    /// <summary>Walks upward from a directory until the AlvorKit solution file is found.</summary>
    public static RepositoryLayout FindFrom(string startDirectory)
    {
        var current = startDirectory;
        while (!File.Exists(Path.Combine(current, "AlvorKit.slnx")))
            current = Path.GetDirectoryName(current)
                ?? throw new InvalidOperationException("AlvorKit.slnx not found above " + startDirectory);
        return new(current);
    }

    /// <summary>Returns all native libraries that have build manifests.</summary>
    public IEnumerable<string> NativeBuildLibraries() =>
        Directory.GetDirectories(NativeDirectory)
            .Where(directory => File.Exists(Path.Combine(directory, "native-build.json")))
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order(StringComparer.OrdinalIgnoreCase);
}

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
    public static RepositoryLayout FindFrom(string startDirectory) =>
        new(ProjectRoot.FindFrom(startDirectory));

    /// <summary>Returns all native libraries that have build manifests.</summary>
    public IEnumerable<string> NativeBuildLibraries() =>
        Directory.GetDirectories(NativeDirectory)
            .Where(directory => RepositoryConfigFile.Find(Path.Combine(directory, "conf"), "native-build") is not null)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order(StringComparer.OrdinalIgnoreCase);
}

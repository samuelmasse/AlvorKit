namespace AlvorKit.Script.Bindgen;

/// <summary>Resolved bindgen inputs, source paths, and package identity for one native library.</summary>
public sealed partial class NativeLibraryBinding
{
    /// <summary>Creates a binding after config validation and version file loading are complete.</summary>
    private NativeLibraryBinding(
        string repositoryRoot,
        string name,
        string directory,
        BindgenConfig config,
        string tag,
        string nativeRevision,
        string bindingRevision,
        string docTag)
    {
        RepositoryRoot = repositoryRoot;
        Name = name;
        Directory = directory;
        Config = config;
        Tag = tag;
        NativeRevision = nativeRevision;
        BindingRevision = bindingRevision;
        DocTag = docTag;

        // Registry bindings pin gl.xml by commit but package by API version, so GL 4.6 stays stable
        // even when the source tag is a hash.
        var versionBase = config.GlVersion ?? tag;
        NativeVersion = VersionWithRevision(versionBase, nativeRevision);
        BindingVersion = VersionWithRevision(versionBase, bindingRevision);
        Version = BindingVersion;

        WorkRoot = ResolveWorkRoot(repositoryRoot, config.WorkDir);
        SourceDirectory = ResolvePath(WorkRoot, ReplaceVersionTokens(config.SourceDir));
    }

    /// <summary>Repository root that contains the native library directory.</summary>
    public string RepositoryRoot { get; }

    /// <summary>Native library directory name under the repository's native folder.</summary>
    public string Name { get; }

    /// <summary>Absolute native library metadata directory.</summary>
    public string Directory { get; }

    /// <summary>Loaded bindgen configuration for this native library.</summary>
    public BindgenConfig Config { get; }

    /// <summary>Upstream source tag or version read from bindgen config or native metadata.</summary>
    public string Tag { get; }

    /// <summary>Native package revision suffix read from version metadata.</summary>
    public string NativeRevision { get; }

    /// <summary>Binding package revision suffix read from version metadata.</summary>
    public string BindingRevision { get; }

    /// <summary>Documentation archive tag read from bindgen config.</summary>
    public string DocTag { get; }

    /// <summary>Generated binding package version.</summary>
    public string BindingVersion { get; }

    /// <summary>Generated native package version.</summary>
    public string NativeVersion { get; }

    /// <summary>Backward-compatible alias for the generated binding package version.</summary>
    public string Version { get; }

    /// <summary>Loads and validates a native library binding from repository metadata.</summary>
    public static NativeLibraryBinding Load(RepositoryLayout repository, string name)
    {
        var directory = Path.Combine(repository.NativeDirectory, name);
        var config = BindgenConfig.Load(directory);
        var versionDirectory = Path.Combine(directory, "version");
        var tag = config.SourceTag ?? ReadOptionalVersion(versionDirectory, "TAG");
        var nativeRevision = config.Kind == BindgenConfig.GlRegistryKind
            ? ""
            : ReadOptionalVersion(versionDirectory, "REVISION");
        var bindingRevision = ReadOptionalVersion(versionDirectory, "BINDING_REVISION");
        if (bindingRevision.Length == 0)
            bindingRevision = nativeRevision;
        var docTag = config.DocTag ?? "";

        ValidateConfig(name, config, tag, docTag);
        return new(repository.Root, name, directory, config, tag, nativeRevision, bindingRevision, docTag);
    }

    /// <summary>Applies source archive tokens such as tag, tagDashes, and docTag.</summary>
    public string ReplaceVersionTokens(string text) =>
        text.Replace("{tag}", Tag)
            .Replace("{tagDashes}", Tag.Replace('.', '-'))
            .Replace("{docTag}", DocTag);

    /// <summary>Rejects bindgen configs that cannot be generated safely.</summary>
    private static void ValidateConfig(string name, BindgenConfig config, string tag, string docTag)
    {
        if (config.Kind is not (BindgenConfig.CHeaderKind or BindgenConfig.GlRegistryKind))
            throw new InvalidOperationException($"{name}: unknown bindgen kind '{config.Kind}'.");
        if (config.Kind == BindgenConfig.CHeaderKind && tag.Length == 0)
            throw new InvalidOperationException($"{name}: TAG is missing.");
        if (config.Kind == BindgenConfig.CHeaderKind && (config.NativeClass.Length == 0 || config.NativeLibrary.Length == 0))
            throw new InvalidOperationException($"{name}: c-header bindings require nativeClass and nativeLibrary.");
        if (config.Kind == BindgenConfig.GlRegistryKind && config.GlVersion is null)
            throw new InvalidOperationException($"{name}: gl-registry bindings require glVersion.");
        if (config.Kind == BindgenConfig.GlRegistryKind && tag.Length == 0)
            throw new InvalidOperationException($"{name}: gl-registry bindings require sourceTag.");
        if (config.Kind == BindgenConfig.GlRegistryKind && config.DocUrl is not null && docTag.Length == 0)
            throw new InvalidOperationException($"{name}: docUrl is set but docTag is missing.");
    }

    /// <summary>Reads an optional version marker file, returning an empty string when absent.</summary>
    private static string ReadOptionalVersion(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        return File.Exists(path) ? File.ReadAllText(path).Trim() : "";
    }

    /// <summary>Appends the AlvorKit package revision segment when present.</summary>
    private static string VersionWithRevision(string versionBase, string revision) =>
        revision.Length > 0 ? $"{versionBase}.{revision}" : versionBase;

    /// <summary>Resolves the library workspace under the repository's local native work directory.</summary>
    private static string ResolveWorkRoot(string repositoryRoot, string workDir)
    {
        var nativeWorkRoot = Path.GetFullPath(Path.Combine(repositoryRoot, "out", "native-work"));
        var resolved = ResolvePath(nativeWorkRoot, workDir);
        if (!IsInsideOrEqual(resolved, nativeWorkRoot))
            throw new InvalidOperationException("workDir must resolve inside out/native-work.");

        return resolved;
    }

    /// <summary>Combines repo/config paths and normalizes separators for the host platform.</summary>
    private static string ResolvePath(string root, string relative) =>
        Path.GetFullPath(Path.Combine(root, relative));

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

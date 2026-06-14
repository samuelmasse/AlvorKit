namespace AlvorKit.Script.Bindgen;

/// <summary>Resolved bindgen inputs, source paths, and package identity for one native library.</summary>
public sealed class NativeLibraryBinding
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

        WorkRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), config.WorkDir);
        SourceDirectory = Path.Combine(WorkRoot, ReplaceVersionTokens(config.SourceDir));
    }

    public string RepositoryRoot { get; }
    public string Name { get; }
    public string Directory { get; }
    public BindgenConfig Config { get; }
    public string Tag { get; }
    public string NativeRevision { get; }
    public string BindingRevision { get; }
    public string DocTag { get; }
    public string BindingVersion { get; }
    public string NativeVersion { get; }
    /// <summary>Backward-compatible alias for the generated binding package version.</summary>
    public string Version { get; }
    public string WorkRoot { get; }
    public string SourceDirectory { get; }
    public string IncludeDirectory => Path.Combine(SourceDirectory, Config.IncludeSubdir);
    public string HeaderPath => Path.Combine(SourceDirectory, Config.Header);
    public string? SizeofShimPath => Config.SizeofShim is null ? null : Path.Combine(Directory, Config.SizeofShim);
    public string NativePackageId => Config.Namespace + ".Native";
    public string HostRuntimeIdentifier => NativeHost.CurrentRuntimeIdentifier;
    public string HostNativeLibraryFileName => NativeHost.CurrentLibraryFileName(Config.NativeLibrary);

    /// <summary>Extracted reference-page tree, when documentation import is configured.</summary>
    public string? DocDirectory => Config.DocUrl is null ? null : Path.Combine(WorkRoot, ReplaceVersionTokens(Config.DocDir));

    /// <summary>Specific documentation subdirectory read by the doc parser.</summary>
    public string? DocReadDirectory => DocDirectory is null ? null : Path.Combine(DocDirectory, Config.DocSubdir);

    /// <summary>Loads and validates a native library binding from repository metadata.</summary>
    public static NativeLibraryBinding Load(RepositoryLayout repository, INativeLibrarySpec spec)
    {
        var name = spec.Name;
        var directory = Path.Combine(repository.NativeDirectory, name);
        var config = spec.LoadConfig(directory);
        ValidateConfig(name, directory, config);

        var tag = ReadRequiredVersion(directory, "TAG");
        var nativeRevision = ReadOptionalVersion(directory, "REVISION");
        var bindingRevision = ReadOptionalVersion(directory, "BINDING_REVISION");
        if (bindingRevision.Length == 0)
            bindingRevision = nativeRevision;
        var docTag = ReadOptionalVersion(directory, "DOC_TAG");
        return new(repository.Root, name, directory, config, tag, nativeRevision, bindingRevision, docTag);
    }

    /// <summary>Applies source archive tokens such as tag, tagDashes, and docTag.</summary>
    public string ReplaceVersionTokens(string text) =>
        text.Replace("{tag}", Tag)
            .Replace("{tagDashes}", Tag.Replace('.', '-'))
            .Replace("{docTag}", DocTag);

    /// <summary>Rejects bindgen configs that cannot be generated safely.</summary>
    private static void ValidateConfig(string name, string directory, BindgenConfig config)
    {
        if (config.Kind is not (BindgenConfig.CHeaderKind or BindgenConfig.GlRegistryKind))
            throw new InvalidOperationException($"{name}: unknown bindgen kind '{config.Kind}'.");
        if (config.Kind == BindgenConfig.CHeaderKind && (config.NativeClass.Length == 0 || config.NativeLibrary.Length == 0))
            throw new InvalidOperationException($"{name}: c-header bindings require nativeClass and nativeLibrary.");
        if (config.Kind == BindgenConfig.GlRegistryKind && config.GlVersion is null)
            throw new InvalidOperationException($"{name}: gl-registry bindings require glVersion.");
        if (config.Kind == BindgenConfig.GlRegistryKind && config.DocUrl is not null && !File.Exists(Path.Combine(directory, "DOC_TAG")))
            throw new InvalidOperationException($"{name}: docUrl is set but native/{name}/DOC_TAG is missing.");
    }

    /// <summary>Reads a required version marker file.</summary>
    private static string ReadRequiredVersion(string directory, string fileName) =>
        File.ReadAllText(Path.Combine(directory, fileName)).Trim();

    /// <summary>Reads an optional version marker file, returning an empty string when absent.</summary>
    private static string ReadOptionalVersion(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        return File.Exists(path) ? File.ReadAllText(path).Trim() : "";
    }

    /// <summary>Appends the AlvorKit package revision segment when present.</summary>
    private static string VersionWithRevision(string versionBase, string revision) =>
        revision.Length > 0 ? $"{versionBase}.{revision}" : versionBase;
}

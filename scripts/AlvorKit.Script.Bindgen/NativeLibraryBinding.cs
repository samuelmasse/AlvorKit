using System.Runtime.InteropServices;

namespace AlvorKit.Script.Bindgen;

public sealed class NativeLibraryBinding
{
    private NativeLibraryBinding(
        string repositoryRoot,
        string name,
        string directory,
        BindgenConfig config,
        string tag,
        string revision,
        string docTag)
    {
        RepositoryRoot = repositoryRoot;
        Name = name;
        Directory = directory;
        Config = config;
        Tag = tag;
        Revision = revision;
        DocTag = docTag;

        // gl-registry tags pin a registry commit; the package version is the bound GL version instead.
        var versionBase = config.GlVersion ?? tag;
        Version = revision.Length > 0 ? $"{versionBase}.{revision}" : versionBase;

        WorkRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), config.WorkDir);
        SourceDirectory = Path.Combine(WorkRoot, ReplaceVersionTokens(config.SourceDir));
    }

    public string RepositoryRoot { get; }
    public string Name { get; }
    public string Directory { get; }
    public BindgenConfig Config { get; }
    public string Tag { get; }
    public string Revision { get; }
    public string DocTag { get; }
    public string Version { get; }
    public string WorkRoot { get; }
    public string SourceDirectory { get; }
    public string IncludeDirectory => Path.Combine(SourceDirectory, Config.IncludeSubdir);
    public string HeaderPath => Path.Combine(SourceDirectory, Config.Header);
    public string? SizeofShimPath => Config.SizeofShim is null ? null : Path.Combine(Directory, Config.SizeofShim);
    public string HostNativeLibraryPath => Path.Combine(Directory, "runtimes", HostRid, "native", HostNativeLibraryFileName);

    /// <summary>The extracted reference-page tree, or null when no DocUrl is configured.</summary>
    public string? DocDirectory => Config.DocUrl is null ? null : Path.Combine(WorkRoot, ReplaceVersionTokens(Config.DocDir));
    /// <summary>The subdirectory of <see cref="DocDirectory"/> the doc parser reads (gl4), or null.</summary>
    public string? DocReadDirectory => DocDirectory is null ? null : Path.Combine(DocDirectory, Config.DocSubdir);

    public static NativeLibraryBinding Load(RepositoryLayout repository, string name)
    {
        var directory = Path.Combine(repository.NativeDirectory, name);
        var config = JsonSerializer.Deserialize<BindgenConfig>(
            File.ReadAllText(Path.Combine(directory, "bindgen.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException($"Could not read bindgen config for {name}.");
        if (config.Kind is not (BindgenConfig.CHeaderKind or BindgenConfig.GlRegistryKind))
            throw new InvalidOperationException($"{name}: unknown bindgen kind '{config.Kind}'.");
        if (config.Kind == BindgenConfig.CHeaderKind && (config.NativeClass.Length == 0 || config.NativeLibrary.Length == 0))
            throw new InvalidOperationException($"{name}: c-header bindings require nativeClass and nativeLibrary.");
        if (config.Kind == BindgenConfig.GlRegistryKind && config.GlVersion is null)
            throw new InvalidOperationException($"{name}: gl-registry bindings require glVersion.");
        if (config.Kind == BindgenConfig.GlRegistryKind && config.DocUrl is not null && !File.Exists(Path.Combine(directory, "DOC_TAG")))
            throw new InvalidOperationException($"{name}: docUrl is set but native/{name}/DOC_TAG is missing.");
        var tag = File.ReadAllText(Path.Combine(directory, "TAG")).Trim();
        var revisionPath = Path.Combine(directory, "REVISION");
        var revision = File.Exists(revisionPath) ? File.ReadAllText(revisionPath).Trim() : "";
        var docTagPath = Path.Combine(directory, "DOC_TAG");
        var docTag = File.Exists(docTagPath) ? File.ReadAllText(docTagPath).Trim() : "";
        return new(repository.Root, name, directory, config, tag, revision, docTag);
    }

    public string ReplaceVersionTokens(string text) =>
        text.Replace("{tag}", Tag).Replace("{tagDashes}", Tag.Replace('.', '-')).Replace("{docTag}", DocTag);

    private string HostNativeLibraryFileName
    {
        get
        {
            if (OperatingSystem.IsWindows())
                return Config.NativeLibrary + ".dll";
            if (OperatingSystem.IsLinux())
                return "lib" + Config.NativeLibrary + ".so";
            if (OperatingSystem.IsMacOS())
                return "lib" + Config.NativeLibrary + ".dylib";

            throw new PlatformNotSupportedException("Native export verification is not configured for this operating system.");
        }
    }

    private static string HostRid
    {
        get
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            if (OperatingSystem.IsWindows())
                return arch switch
                {
                    Architecture.X64 => "win-x64",
                    Architecture.X86 => "win-x86",
                    Architecture.Arm64 => "win-arm64",
                    _ => throw new PlatformNotSupportedException($"Unsupported Windows architecture: {arch}")
                };

            if (OperatingSystem.IsLinux())
                return arch switch
                {
                    Architecture.X64 => "linux-x64",
                    Architecture.Arm64 => "linux-arm64",
                    Architecture.Arm => "linux-arm",
                    _ => throw new PlatformNotSupportedException($"Unsupported Linux architecture: {arch}")
                };

            if (OperatingSystem.IsMacOS())
                return arch switch
                {
                    Architecture.X64 => "osx-x64",
                    Architecture.Arm64 => "osx-arm64",
                    _ => throw new PlatformNotSupportedException($"Unsupported macOS architecture: {arch}")
                };

            throw new PlatformNotSupportedException("Native export verification is not configured for this operating system.");
        }
    }
}

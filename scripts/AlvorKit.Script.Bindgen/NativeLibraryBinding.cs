namespace AlvorKit.Script.Bindgen;

public sealed class NativeLibraryBinding
{
    private NativeLibraryBinding(
        string repositoryRoot,
        string name,
        string directory,
        BindgenConfig config,
        string tag,
        string revision)
    {
        RepositoryRoot = repositoryRoot;
        Name = name;
        Directory = directory;
        Config = config;
        Tag = tag;
        Revision = revision;
        Version = revision.Length > 0 ? $"{tag}.{revision}" : tag;

        var sourceDirectoryName = ReplaceVersionTokens(config.SourceDir);
        SourceDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            config.WorkDir,
            sourceDirectoryName);
    }

    public string RepositoryRoot { get; }
    public string Name { get; }
    public string Directory { get; }
    public BindgenConfig Config { get; }
    public string Tag { get; }
    public string Revision { get; }
    public string Version { get; }
    public string SourceDirectory { get; }
    public string IncludeDirectory => Path.Combine(SourceDirectory, Config.IncludeSubdir);
    public string HeaderPath => Path.Combine(SourceDirectory, Config.Header);
    public string? SizeofShimPath => Config.SizeofShim is null ? null : Path.Combine(Directory, Config.SizeofShim);
    public string WindowsX64NativeLibraryPath => Path.Combine(Directory, "runtimes", "win-x64", "native", Config.NativeLibrary + ".dll");

    public static NativeLibraryBinding Load(RepositoryLayout repository, string name)
    {
        var directory = Path.Combine(repository.NativeDirectory, name);
        var config = JsonSerializer.Deserialize<BindgenConfig>(
            File.ReadAllText(Path.Combine(directory, "bindgen.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException($"Could not read bindgen config for {name}.");
        var tag = File.ReadAllText(Path.Combine(directory, "TAG")).Trim();
        var revisionPath = Path.Combine(directory, "REVISION");
        var revision = File.Exists(revisionPath) ? File.ReadAllText(revisionPath).Trim() : "";
        return new(repository.Root, name, directory, config, tag, revision);
    }

    public string ReplaceVersionTokens(string text) =>
        text.Replace("{tag}", Tag).Replace("{tagDashes}", Tag.Replace('.', '-'));
}

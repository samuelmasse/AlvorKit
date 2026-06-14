namespace AlvorKit.Script.NativeBuild;

/// <summary>Resolved metadata and paths for one native library build.</summary>
internal sealed class LibraryBuildContext
{
    /// <summary>Creates a resolved build context.</summary>
    public LibraryBuildContext(
        string repositoryRoot,
        string name,
        string libraryDirectory,
        BindgenMetadata metadata,
        NativeBuildConfig build,
        string tag,
        string nativeRevision)
    {
        RepositoryRoot = repositoryRoot;
        Name = name;
        LibraryDirectory = libraryDirectory;
        Metadata = metadata;
        Build = build;
        Tag = tag;
        NativeRevision = nativeRevision;
    }

    /// <summary>Absolute repository root path.</summary>
    public string RepositoryRoot { get; }

    /// <summary>Native library directory name under native/.</summary>
    public string Name { get; }

    /// <summary>Absolute path to native/&lt;library&gt;.</summary>
    public string LibraryDirectory { get; }

    /// <summary>Build-relevant values loaded from bindgen.json.</summary>
    public BindgenMetadata Metadata { get; }

    /// <summary>Native build manifest values.</summary>
    public NativeBuildConfig Build { get; }

    /// <summary>Upstream version tag read from TAG.</summary>
    public string Tag { get; }

    /// <summary>AlvorKit package revision read from REVISION.</summary>
    public string NativeRevision { get; }

    /// <summary>Package version formed from TAG and REVISION.</summary>
    public string NativeVersion => NativeRevision.Length > 0 ? $"{Tag}.{NativeRevision}" : Tag;

    /// <summary>User-profile work root for cached source and build directories.</summary>
    public string WorkRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Metadata.WorkDir);

    /// <summary>Directory where the upstream source archive is extracted.</summary>
    public string SourceDirectory => Path.Combine(WorkRoot, ReplaceVersionTokens(Metadata.SourceDir));

    /// <summary>Loads one library context from repository files.</summary>
    public static LibraryBuildContext Load(RepositoryLayout repository, string name)
    {
        var directory = Path.Combine(repository.NativeDirectory, name);
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"native/{name} does not exist.");

        var metadata = JsonFile.Read<BindgenMetadata>(Path.Combine(directory, "bindgen.json"));
        var build = JsonFile.Read<NativeBuildConfig>(Path.Combine(directory, "native-build.json"));
        var tag = File.ReadAllText(Path.Combine(directory, "TAG")).Trim();
        var revisionPath = Path.Combine(directory, "REVISION");
        var revision = File.Exists(revisionPath) ? File.ReadAllText(revisionPath).Trim() : "";
        return new(repository.Root, name, directory, metadata, build, tag, revision);
    }

    /// <summary>Applies source archive tokens used by bindgen metadata.</summary>
    public string ReplaceVersionTokens(string text) =>
        text.Replace("{tag}", Tag, StringComparison.Ordinal)
            .Replace("{tagDashes}", Tag.Replace('.', '-'), StringComparison.Ordinal);

    /// <summary>Returns the build directory for a target RID.</summary>
    public string BuildDirectory(TargetRid target) =>
        Path.Combine(WorkRoot, "build-" + target.Value);

    /// <summary>Returns the package runtimes directory for a target RID.</summary>
    public string OutputDirectory(TargetRid target) =>
        Path.Combine(LibraryDirectory, "runtimes", target.Value, "native");

    /// <summary>Returns the final native library file path for a target RID.</summary>
    public string OutputFile(TargetRid target) =>
        Path.Combine(OutputDirectory(target), target.LibraryFileName(Metadata.NativeLibrary));

    /// <summary>Returns a CMake-produced file path under the target build directory.</summary>
    public string BuildFile(TargetRid target, string relativePath) =>
        Path.Combine([BuildDirectory(target), .. relativePath.Split('/')]);
}

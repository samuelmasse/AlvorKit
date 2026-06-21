namespace AlvorKit.Script.NativeBuild;

/// <summary>Resolved metadata and paths for one native library build.</summary>
/// <param name="RepositoryRoot">Absolute repository root path.</param>
/// <param name="Name">Native library directory name under native/.</param>
/// <param name="LibraryDirectory">Absolute path to native/&lt;library&gt;.</param>
/// <param name="Metadata">Build-relevant values loaded from conf/bindgen.yml.</param>
/// <param name="Build">Native build manifest values.</param>
/// <param name="Tag">Upstream version tag read from version/TAG.</param>
/// <param name="NativeRevision">AlvorKit package revision read from version/REVISION.</param>
internal sealed record LibraryBuildContext(
    string RepositoryRoot,
    string Name,
    string LibraryDirectory,
    BindgenMetadata Metadata,
    NativeBuildConfig Build,
    string Tag,
    string NativeRevision)
{
    /// <summary>Package version formed from version/TAG and version/REVISION.</summary>
    public string NativeVersion => NativeRevision.Length > 0 ? $"{Tag}.{NativeRevision}" : Tag;

    /// <summary>Repository-local work root for cached source and build directories.</summary>
    public string WorkRoot => ResolvePath(Path.Combine(RepositoryRoot, "out", "native-work"), Metadata.WorkDir);

    /// <summary>Directory where the upstream source archive is extracted.</summary>
    public string SourceDirectory => Path.Combine(WorkRoot, ReplaceVersionTokens(Metadata.SourceDir));

    /// <summary>Loads one library context from repository files.</summary>
    public static LibraryBuildContext Load(RepositoryLayout repository, string name)
    {
        var directory = Path.Combine(repository.NativeDirectory, name);
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"native/{name} does not exist.");

        var confDirectory = Path.Combine(directory, "conf");
        var versionDirectory = Path.Combine(directory, "version");
        var metadata = RepositoryConfigFile.Read<BindgenMetadata>(confDirectory, "bindgen");
        var build = RepositoryConfigFile.Read<NativeBuildConfig>(confDirectory, "native-build");
        var tag = File.ReadAllText(Path.Combine(versionDirectory, "TAG")).Trim();
        var revisionPath = Path.Combine(versionDirectory, "REVISION");
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

    /// <summary>Combines and normalizes a root-relative path that must stay under the root.</summary>
    private static string ResolvePath(string root, string relative)
    {
        var normalizedRoot = Path.GetFullPath(root);
        var resolved = Path.GetFullPath(Path.Combine(normalizedRoot, relative));
        if (!IsInsideOrEqual(resolved, normalizedRoot))
            throw new InvalidOperationException("workDir must resolve inside out/native-work.");

        return resolved;
    }

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

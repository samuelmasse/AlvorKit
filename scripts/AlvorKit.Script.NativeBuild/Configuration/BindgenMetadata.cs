namespace AlvorKit.Script.NativeBuild;

/// <summary>Subset of conf/bindgen.json needed to build and package native binaries.</summary>
internal sealed class BindgenMetadata
{
    /// <summary>Native library base name used by .NET runtime probing.</summary>
    public required string NativeLibrary { get; init; }

    /// <summary>User-profile work directory that caches downloaded upstream source.</summary>
    public required string WorkDir { get; init; }

    /// <summary>Directory name created by extracting the upstream source archive.</summary>
    public required string SourceDir { get; init; }

    /// <summary>Optional URL for a gzip-compressed tarball containing upstream source.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>Optional local C translation unit used by single-file builds.</summary>
    public string? ImplFile { get; init; }
}

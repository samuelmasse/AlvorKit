namespace AlvorKit.Script.Bindgen;

/// <summary>Resolved bindgen filesystem paths and package names for one native library.</summary>
public sealed partial class NativeLibraryBinding
{
    /// <summary>Workspace root under the current user's profile for downloaded sources.</summary>
    public string WorkRoot { get; }

    /// <summary>Resolved source directory after version token replacement.</summary>
    public string SourceDirectory { get; }

    /// <summary>Resolved include directory used by generated native parsing and verification.</summary>
    public string IncludeDirectory => Path.Combine(SourceDirectory, Config.IncludeSubdir);

    /// <summary>Resolved primary header or registry file path.</summary>
    public string HeaderPath => Path.Combine(SourceDirectory, Config.Header);

    /// <summary>Resolved size verification shim path when one is configured.</summary>
    public string? SizeofShimPath => Config.SizeofShim is null ? null : Path.Combine(Directory, Config.SizeofShim);

    /// <summary>Generated native package identifier.</summary>
    public string NativePackageId => Config.Namespace + ".Native";

    /// <summary>Runtime identifier for the current host process.</summary>
    public string HostRuntimeIdentifier => NativeHost.CurrentRuntimeIdentifier;

    /// <summary>Native library file name for the current host process.</summary>
    public string HostNativeLibraryFileName => NativeHost.CurrentLibraryFileName(Config.NativeLibrary);

    /// <summary>Extracted reference-page tree when documentation import is configured.</summary>
    public string? DocDirectory => Config.DocUrl is null ? null : Path.Combine(WorkRoot, ReplaceVersionTokens(Config.DocDir));

    /// <summary>Specific documentation subdirectory read by the doc parser.</summary>
    public string? DocReadDirectory => DocDirectory is null ? null : Path.Combine(DocDirectory, Config.DocSubdir);
}

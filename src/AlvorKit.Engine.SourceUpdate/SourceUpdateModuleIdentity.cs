namespace AlvorKit;

/// <summary>Immutable launch identity and current generation of one editable loaded module.</summary>
public sealed record SourceUpdateModuleIdentity(
    string AssemblyName,
    string ModuleMvid,
    string AssemblyPath,
    string AssemblySha256,
    string PdbPath,
    string PdbSha256,
    string ProjectIdentityHash,
    int Generation,
    string? SourceHash,
    bool RestartRequired);

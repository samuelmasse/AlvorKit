namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Immutable artifact identity written by an editable AlvorSense launch.</summary>
public sealed record SourceUpdateEditableLaunchManifest(
    int SchemaVersion,
    string ProjectPath,
    string AssemblyPath,
    string PdbPath,
    string AssemblySha256,
    string PdbSha256,
    string ModuleMvid,
    string ProjectIdentityHash);

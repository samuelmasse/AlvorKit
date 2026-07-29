namespace AlvorKit.Script.AlvorSense;

/// <summary>Exact Debug artifact identity passed to an explicitly composed Source Update target.</summary>
internal sealed record AlvorSenseEditableLaunchManifest(
    int SchemaVersion,
    string ProjectPath,
    string AssemblyPath,
    string PdbPath,
    string AssemblySha256,
    string PdbSha256,
    string ModuleMvid,
    string ProjectIdentityHash,
    string SdkVersion,
    string CodeViewPath);

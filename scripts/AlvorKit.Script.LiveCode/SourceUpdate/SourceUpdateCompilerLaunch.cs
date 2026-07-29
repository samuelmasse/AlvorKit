namespace AlvorKit.Script.LiveCode;

/// <summary>Compiler-facing immutable editable launch identity.</summary>
internal sealed record SourceUpdateCompilerLaunch(
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

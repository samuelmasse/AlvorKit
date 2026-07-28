namespace AlvorKit.Script.LiveWorkspace;

/// <summary>Immutable source-file identity recorded beside a live operation.</summary>
/// <param name="Path">Absolute path to the workspace-owned source file.</param>
/// <param name="Sha256">Lowercase SHA-256 identity of the exact source bytes.</param>
/// <param name="Bytes">Exact source-file length in bytes.</param>
public sealed record LiveWorkspaceSource(
    string Path,
    string Sha256,
    long Bytes);

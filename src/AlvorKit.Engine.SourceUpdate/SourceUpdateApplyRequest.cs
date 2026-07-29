namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Exact compiler-produced delta submitted for one forward source generation.</summary>
public sealed record SourceUpdateApplyRequest(
    string ModuleMvid,
    int ExpectedGeneration,
    string UpdateId,
    string PreviousSourceHash,
    string ResultSourceHash,
    int ExpectedMethodToken,
    int[] ChangedTypeTokens,
    byte[] MetadataDelta,
    byte[] IlDelta,
    byte[] PdbDelta,
    string MetadataDeltaHash,
    string IlDeltaHash,
    string PdbDeltaHash,
    string ProjectIdentityHash);

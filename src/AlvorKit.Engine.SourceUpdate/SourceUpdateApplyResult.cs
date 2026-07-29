namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Generation acknowledgment returned after a Source Update request reaches the safe frame.</summary>
public sealed record SourceUpdateApplyResult(
    SourceUpdateApplyStatus Status,
    string UpdateId,
    string ModuleMvid,
    int Generation,
    string SourceHash,
    string MetadataDeltaHash,
    string IlDeltaHash,
    string PdbDeltaHash,
    string[] HandlerWarnings,
    bool RestartRequired,
    string? Error);

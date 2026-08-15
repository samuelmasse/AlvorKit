namespace AlvorKit;

/// <summary>Exact structured bridge result written by the CLI and optional live workspace recorder.</summary>
internal sealed record LiveCodeBridgeCliResult(
    LiveCodeBridgeExecutionStatus Status,
    string Bridge,
    int Version,
    string[] Lines,
    Dictionary<string, JsonElement> Values,
    IReadOnlyCollection<LiveCodeSavedArtifact> Artifacts,
    double RunMilliseconds,
    string? Error,
    string? ExceptionType,
    string? StackTrace);

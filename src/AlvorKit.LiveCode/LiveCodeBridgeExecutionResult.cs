namespace AlvorKit.LiveCode;

/// <summary>Structured terminal response from one predefined LiveCode bridge.</summary>
public sealed record LiveCodeBridgeExecutionResult(
    LiveCodeBridgeExecutionStatus Status,
    string Bridge,
    int Version,
    string[] Lines,
    Dictionary<string, JsonElement> Values,
    LiveCodeBridgeArtifact[] Artifacts,
    double RunMilliseconds,
    string? Error,
    string? ExceptionType,
    string? StackTrace);

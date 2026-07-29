namespace AlvorKit.Script.LiveCode;

/// <summary>Token-free identity and latest durable state for one workspace Source Update owner.</summary>
internal sealed record SourceUpdateCoordinatorManifest(
    int SchemaVersion,
    string WorkspacePath,
    string LaunchManifestPath,
    string PipeName,
    int ProcessId,
    DateTimeOffset StartedUtc,
    bool Ready,
    int Generation,
    string State,
    string? OperationId,
    string? Error);

namespace AlvorKit;

/// <summary>Describes one discoverable LiveCode endpoint owned by a running development process.</summary>
public sealed record LiveCodeSessionManifest(
    int ProtocolVersion,
    string SessionId,
    string Name,
    int ProcessId,
    int Port,
    string Token,
    DateTimeOffset StartedUtc,
    string ManifestPath,
    bool FrozenInspectionEnabled = false);

namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Runtime capability and exact allowlisted module identities for one editable process.</summary>
public sealed record SourceUpdateCapabilities(
    int ProtocolVersion,
    bool MetadataUpdaterSupported,
    string RuntimeVersion,
    int ProcessId,
    string Mode,
    bool RejitAvailable,
    string SupportedEditShape,
    int MaximumDeltaBytes,
    bool RestartRequired,
    SourceUpdateModuleIdentity[] Modules);

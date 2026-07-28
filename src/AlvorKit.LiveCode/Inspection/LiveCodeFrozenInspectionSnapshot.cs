namespace AlvorKit.LiveCode;

/// <summary>Describes the current frame heartbeat and dedicated frozen-inspection lane.</summary>
public sealed record LiveCodeFrozenInspectionSnapshot(
    bool Enabled,
    bool IsFrozen,
    long FrameNumber,
    double FrameAgeMilliseconds,
    double FreezeThresholdMilliseconds,
    bool InspectorThreadAlive,
    bool InspectionRunning,
    int InspectorManagedThreadId);

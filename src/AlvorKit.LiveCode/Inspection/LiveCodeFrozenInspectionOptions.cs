namespace AlvorKit.LiveCode;

/// <summary>Configures the dormant out-of-band command lane used only after the game-frame heartbeat stalls.</summary>
public sealed record LiveCodeFrozenInspectionOptions
{
    /// <summary>Gets the minimum frame-heartbeat age required before frozen execution is accepted.</summary>
    public TimeSpan FreezeThreshold { get; init; } = TimeSpan.FromSeconds(2);
}

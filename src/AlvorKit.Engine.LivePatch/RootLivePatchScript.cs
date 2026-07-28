namespace AlvorKit.Engine;

/// <summary>Pumps native patch completions and collectible submission cleanup at a safe frame boundary.</summary>
public sealed class RootLivePatchScript(
    LivePatchSession session,
    LivePatchLiveCodeBridge bridge,
    WindowLoop window) : Script
{
    /// <inheritdoc />
    public override void Load() => window.Dispatch += Pump;

    /// <inheritdoc />
    public override void Unload()
    {
        window.Dispatch -= Pump;
        bridge.Dispose();
        session.Dispose();
    }

    private void Pump()
    {
        _ = session.Pump();
        bridge.Pump();
    }
}

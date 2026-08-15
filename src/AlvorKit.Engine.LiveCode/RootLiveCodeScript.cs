namespace AlvorKit;

/// <summary>Owns a LiveCode host and pumps queued work at the window loop's safe pre-update dispatch point.</summary>
public sealed class RootLiveCodeScript(
    LiveCodeHost host,
    WindowLoop window) : Script
{
    /// <inheritdoc />
    public override void Load()
    {
        host.Start();
        window.Dispatch += Pump;
    }

    /// <inheritdoc />
    public override void Unload()
    {
        window.Dispatch -= Pump;
        host.Dispose();
    }

    private void Pump() => host.Pump();
}

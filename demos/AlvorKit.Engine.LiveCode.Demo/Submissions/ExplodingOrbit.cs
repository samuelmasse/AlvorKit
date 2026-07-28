using AlvorKit.Engine.LiveCode.Demo;
using AlvorKit.LivePatch;

/// <summary>
/// Deliberately fails once. The exact trampoline contains the exception,
/// deactivates itself, and subsequent frames run ColonyGarden.Update normally.
/// </summary>
public sealed class ExplodingOrbit
{
    [LivePatchHandler]
    public void Run(ColonyGarden receiver, double delta)
    {
        _ = receiver;
        _ = delta;
        throw new InvalidOperationException(
            "The demo patch deliberately crossed a solar singularity.");
    }
}

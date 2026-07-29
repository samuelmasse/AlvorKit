namespace AlvorKit.Engine.SourceUpdate.Demo;

/// <summary>
/// Ordinary service whose existing method is edited in this file; its private state and
/// captured constructor dependencies remain normal compiler-bound members.
/// </summary>
public sealed class PulseService(PulsePalette palette, PulseClock clock, float offset)
{
    private float phase = offset;
    private float energy;
    private int updates;

    /// <summary>Advances one existing instance and returns its visual result.</summary>
    public PulseReading Step(double delta)
    {
        phase += (float)delta * clock.Speed;
        energy = 0.5f + MathF.Sin(phase) * 0.5f;
        updates++;
        return new("ORIGINAL METHOD", palette.Original * (0.55f + energy * 0.45f), energy, updates);
    }
}

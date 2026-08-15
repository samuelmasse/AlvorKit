namespace AlvorKit;

/// <summary>Root-owned clock shared by every colony simulation.</summary>
[Root]
public sealed class UniverseClock
{
    /// <summary>Gets the current simulation tick.</summary>
    public long Tick { get; private set; }

    /// <summary>Gets the accumulated simulation time.</summary>
    public double Time { get; private set; }

    /// <summary>Advances the universe by one engine update.</summary>
    public void Advance(double delta)
    {
        Tick++;
        Time += delta;
    }
}

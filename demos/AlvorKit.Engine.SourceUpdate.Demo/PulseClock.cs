namespace AlvorKit.Engine.SourceUpdate.Demo;

/// <summary>Injected value used by every existing editable service instance.</summary>
[Root]
public sealed class PulseClock
{
    /// <summary>Gets the shared pulse speed.</summary>
    public float Speed => 1.4f;
}

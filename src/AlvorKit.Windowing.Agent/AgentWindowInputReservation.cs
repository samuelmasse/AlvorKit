namespace AlvorKit.Windowing;

/// <summary>Releases one exclusive synthetic-input reservation exactly once.</summary>
internal sealed class AgentWindowInputReservation : IDisposable
{
    /// <summary>Gate that owns the reservation until disposal.</summary>
    private AgentWindowInputGate? owner;

    /// <summary>Creates a reservation over the supplied input gate.</summary>
    internal AgentWindowInputReservation(AgentWindowInputGate owner) =>
        this.owner = owner;

    /// <summary>Releases the reservation once and ignores repeated disposal.</summary>
    public void Dispose() => Interlocked.Exchange(ref owner, null)?.Release();
}

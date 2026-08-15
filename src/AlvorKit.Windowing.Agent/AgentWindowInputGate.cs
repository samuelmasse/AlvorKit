namespace AlvorKit;

/// <summary>Excludes native input during one synthetic transaction and quarantines the following native poll.</summary>
internal sealed class AgentWindowInputGate
{
    /// <summary>Nonzero while one caller owns the exclusive synthetic-input reservation.</summary>
    private int reservation;

    /// <summary>True after release until the next native poll begins.</summary>
    private bool quarantineNextPoll;

    /// <summary>True while the post-release native poll is being discarded.</summary>
    private bool quarantiningPoll;

    /// <summary>Gets whether an exclusive synthetic-input reservation is active.</summary>
    internal bool IsReserved => reservation != 0;

    /// <summary>Gets whether callbacks from the current native poll may be published.</summary>
    internal bool AcceptsNativeEvents => !IsReserved && !quarantiningPoll;

    /// <summary>Acquires the only synthetic-input reservation.</summary>
    internal IDisposable Reserve()
    {
        if (Interlocked.CompareExchange(ref reservation, 1, 0) != 0)
            throw new InvalidOperationException("The window already has an exclusive puppet input reservation.");

        return new AgentWindowInputReservation(this);
    }

    /// <summary>Begins quarantining when a released reservation left a native poll pending.</summary>
    internal void BeforePoll()
    {
        if (quarantineNextPoll)
            quarantiningPoll = true;
    }

    /// <summary>Ends the one native poll quarantined after a reservation.</summary>
    internal void AfterPoll()
    {
        if (!quarantiningPoll)
            return;

        quarantiningPoll = false;
        quarantineNextPoll = false;
    }

    /// <summary>Releases the reservation and schedules one native poll for quarantine.</summary>
    internal void Release()
    {
        quarantineNextPoll = true;
        Volatile.Write(ref reservation, 0);
    }
}

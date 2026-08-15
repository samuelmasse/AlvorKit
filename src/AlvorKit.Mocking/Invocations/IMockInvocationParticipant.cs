namespace AlvorKit;

/// <summary>Exposes one session participant's invocation ledger.</summary>
internal interface IMockInvocationParticipant
{
    /// <summary>Gets the ledger contributing to a session timeline.</summary>
    MockInvocationLedger Invocations { get; }
}

namespace AlvorKit.Mocking;

/// <summary>Exposes one session participant's invocation ledger.</summary>
internal interface MockInvocationParticipant
{
    /// <summary>Gets the ledger contributing to a session timeline.</summary>
    MockInvocationLedger Invocations { get; }
}

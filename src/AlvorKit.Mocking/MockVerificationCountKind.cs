namespace AlvorKit.Mocking;

/// <summary>Identifies one supported invocation-count constraint.</summary>
internal enum MockVerificationCountKind
{
    /// <summary>The observed count must equal the expected count.</summary>
    Exactly,

    /// <summary>The observed count must be at least the expected count.</summary>
    AtLeast,

    /// <summary>The observed count must be at most the expected count.</summary>
    AtMost
}

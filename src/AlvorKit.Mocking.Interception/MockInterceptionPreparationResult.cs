using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Returns either one complete activation or actionable diagnostics.</summary>
public sealed class MockInterceptionPreparationResult
{
    /// <summary>The complete owned activation, or null on failure.</summary>
    private readonly MockInterceptionActivation? activation;

    /// <summary>The deterministic preparation, activation, or rollback failures.</summary>
    private readonly ImmutableArray<MockInterceptionPreparationDiagnostic>
        diagnostics;

    /// <summary>Creates one immutable transaction result.</summary>
    internal MockInterceptionPreparationResult(
        MockInterceptionActivation? activation,
        ImmutableArray<MockInterceptionPreparationDiagnostic> diagnostics)
    {
        this.activation = activation;
        this.diagnostics = diagnostics;
    }

    /// <summary>Gets whether every route generation activated successfully.</summary>
    public bool IsSuccessful =>
        activation is not null &&
        diagnostics.IsEmpty;

    /// <summary>Gets the complete owned activation, or null on failure.</summary>
    public MockInterceptionActivation? Activation => activation;

    /// <summary>Gets deterministic actionable failures in transaction order.</summary>
    public ImmutableArray<MockInterceptionPreparationDiagnostic> Diagnostics =>
        diagnostics;
}

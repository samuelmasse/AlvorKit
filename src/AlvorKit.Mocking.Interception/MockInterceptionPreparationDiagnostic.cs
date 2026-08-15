namespace AlvorKit;

/// <summary>Describes one public actionable operation-route preparation failure.</summary>
public sealed class MockInterceptionPreparationDiagnostic
{
    /// <summary>The stable failure category.</summary>
    private readonly MockInterceptionPreparationFailureReason reason;

    /// <summary>The exact code-first route identity.</summary>
    private readonly string routeId;

    /// <summary>The backend-specific deterministic failure detail.</summary>
    private readonly string detail;

    /// <summary>The concrete recovery action.</summary>
    private readonly string suggestedAction;

    /// <summary>Creates one immutable actionable preparation diagnostic.</summary>
    public MockInterceptionPreparationDiagnostic(
        MockInterceptionPreparationFailureReason reason,
        string routeId,
        string detail,
        string? suggestedAction = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);
        if (suggestedAction is not null)
            ArgumentException.ThrowIfNullOrWhiteSpace(suggestedAction);

        this.reason = reason;
        this.routeId = routeId;
        this.detail = detail;
        this.suggestedAction =
            suggestedAction ?? DefaultAction(reason);
    }

    /// <summary>Gets the stable failure category.</summary>
    public MockInterceptionPreparationFailureReason Reason => reason;

    /// <summary>Gets the exact code-first route identity.</summary>
    public string RouteId => routeId;

    /// <summary>Gets the backend-specific deterministic failure detail.</summary>
    public string Detail => detail;

    /// <summary>Gets the concrete recovery action.</summary>
    public string SuggestedAction => suggestedAction;

    /// <summary>Gets the complete actionable public diagnostic.</summary>
    public string Message =>
        $"Interception route '{routeId}' failed with {reason}: {detail} " +
        $"Action: {suggestedAction}";

    /// <summary>Returns the complete actionable public diagnostic.</summary>
    public override string ToString() => Message;

    /// <summary>Provides a concrete default recovery for each stable category.</summary>
    private static string DefaultAction(
        MockInterceptionPreparationFailureReason reason) =>
        reason switch
        {
            MockInterceptionPreparationFailureReason.ProfilerUnavailable =>
                "Start the process with the AlvorKit Interception profiler " +
                "enabled, then prepare the plan again.",
            MockInterceptionPreparationFailureReason.AbiMismatch =>
                "Install matching managed adapter and profiler packages, " +
                "then restart the process.",
            MockInterceptionPreparationFailureReason.ModuleAllowlistRejected =>
                "Add the caller module name to " +
                "ALVORKIT_INTERCEPTION_MODULES, then restart the process; " +
                "the profiler validates its MVID separately.",
            MockInterceptionPreparationFailureReason.StaleBody =>
                "Resolve the loaded caller body again and rebuild the plan " +
                "before activation.",
            MockInterceptionPreparationFailureReason.UnsupportedSignature =>
                "Select a supported closed exact operation signature or " +
                "remove this route from the plan.",
            MockInterceptionPreparationFailureReason.PreparationFailed =>
                "Inspect the managed preparation exception, correct the " +
                "route configuration, and retry from a pristine plan.",
            MockInterceptionPreparationFailureReason.Collision =>
                "Remove the overlapping route or explicitly compose the " +
                "conflicting claims in deterministic order.",
            MockInterceptionPreparationFailureReason.RejitFailed =>
                "Inspect the profiler request completion, correct the " +
                "reported rejection, and retry from a pristine plan.",
            MockInterceptionPreparationFailureReason.RollbackFailed =>
                "Stop using the route and restart the profiled process to " +
                "restore a known loaded baseline.",
            _ => throw new ArgumentOutOfRangeException(nameof(reason))
        };
}

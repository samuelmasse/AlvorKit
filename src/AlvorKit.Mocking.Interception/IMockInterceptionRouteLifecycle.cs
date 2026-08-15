namespace AlvorKit;

/// <summary>
/// Supplies backend-specific cold-path preparation, activation, and rollback.
/// </summary>
public interface IMockInterceptionRouteLifecycle
{
    /// <summary>
    /// Prepares one route without activation, returning null on complete
    /// success. Every attempted route is later passed to <see cref="Rollback"/>
    /// when the transaction fails.
    /// </summary>
    MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route);

    /// <summary>
    /// Activates one prepared route behind a closed publication gate,
    /// returning null only when its generation is backend-ready.
    /// </summary>
    MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route);

    /// <summary>
    /// Restores an attempted, partially active, or active route to its pristine
    /// baseline; implementations must be idempotent.
    /// </summary>
    void Rollback(MockInterceptionRoute route);
}

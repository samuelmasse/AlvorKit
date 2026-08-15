namespace AlvorKit;

/// <summary>
/// Selects calls on a non-ref struct by type, live entry value, or interception site
/// without retaining a receiver value or storage address.
/// </summary>
/// <typeparam name="T">The exact intercepted struct type.</typeparam>
public sealed class MockStructScope<T>
    where T : struct
{
    private readonly MockStructScopeDescriptor descriptor;

    /// <summary>Creates the type-wide root scope.</summary>
    internal MockStructScope()
        : this(new(typeof(T)))
    {
    }

    private MockStructScope(
        MockStructScopeDescriptor descriptor)
    {
        this.descriptor = descriptor;
    }

    /// <summary>Gets the scope's explicit value-identity-free selection mode.</summary>
    public MockStructMode Mode => descriptor.Mode;

    /// <summary>
    /// Selects calls whose predicate accepts the live receiver entry value.
    /// The predicate is reevaluated for every assigned, passed, returned, or
    /// boxed copy.
    /// </summary>
    public MockStructScope<T> Matching(
        RefPredicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(descriptor.Matching(predicate));
    }

    /// <summary>
    /// Selects one exact interception site, allowing equal values at different sites
    /// to receive different behavior.
    /// </summary>
    public MockStructScope<T> AtSite(MockCallSite site)
    {
        ArgumentNullException.ThrowIfNull(site);
        return new(descriptor.AtSite(site));
    }

    /// <summary>Captures one void operation for setup.</summary>
    public MockStructSetupClause<T> When(
        MockStructCall<T> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return new(
            MockStructApiBoundary.Setup(
                descriptor,
                operation));
    }

    /// <summary>Captures one value-returning operation for setup.</summary>
    public MockStructSetupClause<T, TResult> When<TResult>(
        MockStructCall<T, TResult> operation)
        where TResult : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(operation);
        return new(
            MockStructApiBoundary.Setup(
                descriptor,
                operation));
    }

    /// <summary>Captures one void operation for count verification.</summary>
    public MockStructVerification Verify(
        MockStructCall<T> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return new(
            MockStructApiBoundary.Verification(
                descriptor,
                operation));
    }

    /// <summary>Captures one value-returning operation for count verification.</summary>
    public MockStructVerification Verify<TResult>(
        MockStructCall<T, TResult> operation)
        where TResult : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(operation);
        return new(
            MockStructApiBoundary.Verification(
                descriptor,
                operation));
    }

    /// <summary>Gets immutable scope metadata for contract/runtime integration.</summary>
    internal MockStructScopeDescriptor Descriptor => descriptor;
}

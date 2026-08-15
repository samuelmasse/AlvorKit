namespace AlvorKit;

/// <summary>
/// Identifies one code-first operation route and guards its capture/use state.
/// </summary>
public sealed class MockInterceptionRoute
{
    /// <summary>The route is not owned by a transaction.</summary>
    private const int Inactive = 0;

    /// <summary>The route is exclusively reserved for preparation.</summary>
    private const int Reserved = 1;

    /// <summary>The route is backend-ready and awaits shared publication.</summary>
    private const int Active = 2;

    /// <summary>A failed rollback left the backend state non-pristine.</summary>
    private const int Poisoned = 3;

    /// <summary>The exact stable code-first route identity.</summary>
    private readonly string id;

    /// <summary>The atomic inactive, reserved, active, or poisoned ownership state.</summary>
    private int state;

    /// <summary>The shared complete-transaction publication gate.</summary>
    private MockInterceptionPublicationGate? publicationGate;

    /// <summary>The lifecycle-scoped exact-identity ownership lease.</summary>
    private MockInterceptionRouteIdentityLease? identityLease;

    /// <summary>Creates one initially inactive exact operation route.</summary>
    public MockInterceptionRoute(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        this.id = id;
    }

    /// <summary>Gets the exact stable code-first route identity.</summary>
    public string Id => id;

    /// <summary>Gets whether the complete route generation is active.</summary>
    public bool IsActivated =>
        Volatile.Read(ref state) == Active &&
        Volatile.Read(ref publicationGate)?.IsPublished == true;

    /// <summary>
    /// Rejects capture or route use until the complete generation is active.
    /// </summary>
    public void RequireActivated()
    {
        if (IsActivated)
            return;

        if (Volatile.Read(ref state) == Poisoned)
        {
            throw new MockException(
                $"Interception route '{id}' is unavailable because rollback " +
                "did not restore a known pristine backend state. Stop using " +
                "the route and restart the profiled process.");
        }

        throw new MockException(
            $"Interception route '{id}' is not active. Complete successful " +
            "preparation and activation before capture, setup, verification, " +
            "or intercepted route use. Dynamic proxy mocking remains " +
            "available independently.");
    }

    /// <summary>Reserves the inactive route for one exclusive transaction.</summary>
    internal bool TryReserve(
        MockInterceptionPublicationGate gate,
        MockInterceptionRouteIdentityLease lease)
    {
        if (Interlocked.CompareExchange(
                ref state,
                Reserved,
                Inactive) != Inactive)
        {
            return false;
        }

        Volatile.Write(ref identityLease, lease);
        Volatile.Write(ref publicationGate, gate);
        return true;
    }

    /// <summary>Marks the route backend-ready only for its reserving transaction.</summary>
    internal bool TryMarkBackendReady() =>
        Interlocked.CompareExchange(ref state, Active, Reserved) ==
        Reserved;

    /// <summary>Gets whether failed rollback permanently poisoned this route.</summary>
    internal bool IsPoisoned =>
        Volatile.Read(ref state) == Poisoned;

    /// <summary>Releases ownership after a successful pristine rollback.</summary>
    internal void ReleaseOwnership()
    {
        MockInterceptionRouteIdentityLease? lease =
            Interlocked.Exchange(ref identityLease, null);
        Volatile.Write(ref publicationGate, null);
        Volatile.Write(ref state, Inactive);
        lease?.Release();
    }

    /// <summary>Retains poisoned ownership after a failed rollback.</summary>
    internal void PoisonOwnership()
    {
        Volatile.Write(ref publicationGate, null);
        identityLease?.Poison();
        Volatile.Write(ref state, Poisoned);
    }
}

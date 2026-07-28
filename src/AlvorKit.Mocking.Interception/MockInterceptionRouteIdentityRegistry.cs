namespace AlvorKit.Mocking.Interception;

/// <summary>
/// Owns stable route-identity claims without retaining completed lifecycles.
/// </summary>
internal static class MockInterceptionRouteIdentityRegistry
{
    /// <summary>The weak registry partition for each backend lifecycle.</summary>
    private static readonly ConditionalWeakTable<
        IMockInterceptionRouteLifecycle,
        MockInterceptionRouteIdentityClaims> ClaimsByLifecycle = [];

    /// <summary>Gets the identity-claim set for one backend lifecycle.</summary>
    internal static MockInterceptionRouteIdentityClaims For(
        IMockInterceptionRouteLifecycle lifecycle) =>
        ClaimsByLifecycle.GetValue(
            lifecycle,
            static _ => new());
}

/// <summary>Identifies the outcome of one stable-identity claim attempt.</summary>
internal enum MockInterceptionRouteIdentityClaimResult
{
    /// <summary>The caller exclusively acquired the identity.</summary>
    Acquired,

    /// <summary>Another transaction currently owns the identity.</summary>
    Owned,

    /// <summary>A failed rollback permanently poisoned the identity.</summary>
    Poisoned
}

/// <summary>Coordinates stable route identities for one backend lifecycle.</summary>
internal sealed class MockInterceptionRouteIdentityClaims
{
    /// <summary>Serializes claim creation, release, and poisoning.</summary>
    private readonly Lock sync = new();

    /// <summary>The active or poisoned claim for each stable identity.</summary>
    private readonly Dictionary<
        string,
        MockInterceptionRouteIdentityLease> leases =
        new(StringComparer.Ordinal);

    /// <summary>Attempts to acquire one stable identity exclusively.</summary>
    internal MockInterceptionRouteIdentityClaimResult TryAcquire(
        string routeId,
        out MockInterceptionRouteIdentityLease? lease)
    {
        lock (sync)
        {
            if (leases.TryGetValue(routeId, out var existing))
            {
                lease = null;
                return existing.IsPoisoned
                    ? MockInterceptionRouteIdentityClaimResult.Poisoned
                    : MockInterceptionRouteIdentityClaimResult.Owned;
            }

            lease = new(this, routeId);
            leases.Add(routeId, lease);
            return MockInterceptionRouteIdentityClaimResult.Acquired;
        }
    }

    /// <summary>Releases one successfully restored identity claim.</summary>
    internal void Release(MockInterceptionRouteIdentityLease lease)
    {
        lock (sync)
        {
            if (lease.IsPoisoned)
                return;

            if (leases.TryGetValue(lease.RouteId, out var current) &&
                ReferenceEquals(current, lease))
            {
                leases.Remove(lease.RouteId);
            }
        }
    }

    /// <summary>Retains one failed-rollback claim as permanently poisoned.</summary>
    internal void Poison(MockInterceptionRouteIdentityLease lease)
    {
        lock (sync)
        {
            if (leases.TryGetValue(lease.RouteId, out var current) &&
                ReferenceEquals(current, lease))
            {
                lease.MarkPoisoned();
            }
        }
    }
}

/// <summary>Owns one lifecycle-scoped stable route identity.</summary>
internal sealed class MockInterceptionRouteIdentityLease
{
    /// <summary>The lifecycle-scoped registry owner.</summary>
    private readonly MockInterceptionRouteIdentityClaims owner;

    /// <summary>The exact stable route identity.</summary>
    private readonly string routeId;

    /// <summary>Zero while releasable and one after rollback poisoning.</summary>
    private int poisoned;

    /// <summary>Creates one exclusively held stable identity lease.</summary>
    internal MockInterceptionRouteIdentityLease(
        MockInterceptionRouteIdentityClaims owner,
        string routeId)
    {
        this.owner = owner;
        this.routeId = routeId;
    }

    /// <summary>Gets the exact stable route identity.</summary>
    internal string RouteId => routeId;

    /// <summary>Gets whether a failed rollback retained this claim.</summary>
    internal bool IsPoisoned => Volatile.Read(ref poisoned) != 0;

    /// <summary>Releases this claim after a successful rollback.</summary>
    internal void Release() => owner.Release(this);

    /// <summary>Retains this claim after a failed rollback.</summary>
    internal void Poison() => owner.Poison(this);

    /// <summary>Marks this retained claim as poisoned under the owner lock.</summary>
    internal void MarkPoisoned() =>
        Volatile.Write(ref poisoned, 1);
}

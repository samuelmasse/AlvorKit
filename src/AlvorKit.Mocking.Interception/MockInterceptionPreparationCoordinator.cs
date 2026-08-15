using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Prepares every route before activation and rolls partial activation back.
/// </summary>
public sealed class MockInterceptionPreparationCoordinator
{
    /// <summary>The backend-specific route lifecycle.</summary>
    private readonly IMockInterceptionRouteLifecycle lifecycle;

    /// <summary>The lifecycle-scoped stable route-identity claims.</summary>
    private readonly MockInterceptionRouteIdentityClaims identityClaims;

    /// <summary>Creates one managed, runtime-neutral transaction coordinator.</summary>
    public MockInterceptionPreparationCoordinator(
        IMockInterceptionRouteLifecycle lifecycle)
    {
        ArgumentNullException.ThrowIfNull(lifecycle);
        this.lifecycle = lifecycle;
        identityClaims =
            MockInterceptionRouteIdentityRegistry.For(lifecycle);
    }

    /// <summary>
    /// Prepares every exact route, then activates all or restores a pristine set.
    /// </summary>
    public MockInterceptionPreparationResult PrepareAndActivate(
        IEnumerable<MockInterceptionRoute> routes)
    {
        ArgumentNullException.ThrowIfNull(routes);
        var requested = routes.ToImmutableArray();
        if (requested.IsEmpty)
        {
            throw new ArgumentException(
                "At least one interception route is required.",
                nameof(routes));
        }

        var validation = Validate(requested);
        if (!validation.IsEmpty)
            return new(null, validation);

        MockInterceptionPublicationGate publicationGate = new();
        var reservation = Reserve(requested, publicationGate);
        if (reservation is not null)
            return new(null, [reservation]);

        var diagnostics =
            ImmutableArray.CreateBuilder<MockInterceptionPreparationDiagnostic>();
        foreach (var route in requested)
        {
            MockInterceptionPreparationDiagnostic? diagnostic;
            try
            {
                diagnostic = lifecycle.Prepare(route);
            }
            catch (Exception exception)
            {
                diagnostic = new(
                    MockInterceptionPreparationFailureReason
                        .PreparationFailed,
                    route.Id,
                    $"Preparation threw {exception.GetType().Name}: " +
                    exception.Message);
            }

            if (diagnostic is not null)
                diagnostics.Add(diagnostic);
        }

        if (diagnostics.Count != 0)
        {
            Rollback(publicationGate, requested, diagnostics);
            return new(null, diagnostics.ToImmutable());
        }

        foreach (var route in requested)
        {
            MockInterceptionPreparationDiagnostic? diagnostic;
            try
            {
                diagnostic = lifecycle.Activate(route);
            }
            catch (Exception exception)
            {
                diagnostic = new(
                    MockInterceptionPreparationFailureReason.RejitFailed,
                    route.Id,
                    $"Activation threw {exception.GetType().Name}: " +
                    exception.Message);
            }

            if (diagnostic is not null)
            {
                diagnostics.Add(diagnostic);
                Rollback(publicationGate, requested, diagnostics);
                return new(null, diagnostics.ToImmutable());
            }
        }

        foreach (var route in requested)
        {
            if (!route.TryMarkBackendReady())
            {
                diagnostics.Add(Collision(
                    route,
                    "the route lost its reserved ownership before this " +
                    "transaction could publish its completed activation"));
                Rollback(publicationGate, requested, diagnostics);
                return new(null, diagnostics.ToImmutable());
            }
        }

        publicationGate.Publish();
        return new(
            new(lifecycle, requested, publicationGate),
            []);
    }

    /// <summary>Rejects duplicate route identities before ownership changes.</summary>
    private static ImmutableArray<MockInterceptionPreparationDiagnostic>
        Validate(ImmutableArray<MockInterceptionRoute> routes)
    {
        var diagnostics =
            ImmutableArray.CreateBuilder<MockInterceptionPreparationDiagnostic>();
        HashSet<string> ids = new(StringComparer.Ordinal);
        foreach (var route in routes)
        {
            ArgumentNullException.ThrowIfNull(route);
            if (!ids.Add(route.Id))
            {
                diagnostics.Add(Collision(
                    route,
                    "the transaction contains the same exact route identity " +
                    "more than once"));
            }
        }

        return diagnostics.ToImmutable();
    }

    /// <summary>Reserves every route before the first backend preparation call.</summary>
    private MockInterceptionPreparationDiagnostic? Reserve(
        ImmutableArray<MockInterceptionRoute> routes,
        MockInterceptionPublicationGate publicationGate)
    {
        var reserved = 0;
        MockInterceptionPreparationDiagnostic? rejection = null;
        while (reserved < routes.Length)
        {
            var route = routes[reserved];
            MockInterceptionRouteIdentityClaimResult claim =
                identityClaims.TryAcquire(route.Id, out var lease);
            if (claim !=
                MockInterceptionRouteIdentityClaimResult.Acquired)
            {
                rejection = claim ==
                    MockInterceptionRouteIdentityClaimResult.Poisoned
                    ? Poisoned(route)
                    : Collision(
                        route,
                        "the route identity is reserved or active in " +
                        "another preparation transaction");
                break;
            }

            if (!route.TryReserve(publicationGate, lease!))
            {
                lease!.Release();
                rejection = route.IsPoisoned
                    ? Poisoned(route)
                    : Collision(
                        route,
                        "the route object is reserved or active in another " +
                        "preparation transaction");
                break;
            }

            ++reserved;
        }

        if (reserved == routes.Length)
            return null;

        for (var index = reserved - 1; index >= 0; --index)
            routes[index].ReleaseOwnership();
        return rejection;
    }

    /// <summary>Rolls every attempted prepared route back in LIFO order.</summary>
    private void Rollback(
        MockInterceptionPublicationGate publicationGate,
        ImmutableArray<MockInterceptionRoute> routes,
        ImmutableArray<MockInterceptionPreparationDiagnostic>.Builder
            diagnostics)
    {
        publicationGate.Unpublish();
        for (var index = routes.Length - 1; index >= 0; --index)
            Rollback(routes[index], diagnostics);
    }

    /// <summary>Attempts one rollback and records a public recovery diagnostic.</summary>
    private void Rollback(
        MockInterceptionRoute route,
        ImmutableArray<MockInterceptionPreparationDiagnostic>.Builder
            diagnostics)
    {
        try
        {
            lifecycle.Rollback(route);
            route.ReleaseOwnership();
        }
        catch (Exception exception)
        {
            route.PoisonOwnership();
            diagnostics.Add(new(
                MockInterceptionPreparationFailureReason.RollbackFailed,
                route.Id,
                $"Rollback threw {exception.GetType().Name}: " +
                exception.Message));
        }
    }

    /// <summary>Creates one transaction-ownership collision diagnostic.</summary>
    private static MockInterceptionPreparationDiagnostic Collision(
        MockInterceptionRoute route,
        string detail) =>
        new(
            MockInterceptionPreparationFailureReason.Collision,
            route.Id,
            detail);

    /// <summary>Creates one retained failed-rollback diagnostic.</summary>
    private static MockInterceptionPreparationDiagnostic Poisoned(
        MockInterceptionRoute route) =>
        new(
            MockInterceptionPreparationFailureReason.RollbackFailed,
            route.Id,
            "an earlier rollback did not restore a known pristine backend " +
            "state, so this lifecycle permanently retained the route claim");
}

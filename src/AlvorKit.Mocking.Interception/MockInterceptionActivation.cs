using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Owns a complete set of active routes and reverses them in LIFO order.</summary>
public sealed class MockInterceptionActivation : IDisposable
{
    /// <summary>The backend-specific route lifecycle.</summary>
    private readonly IMockInterceptionRouteLifecycle lifecycle;

    /// <summary>The routes in activation order.</summary>
    private readonly ImmutableArray<MockInterceptionRoute> routes;

    /// <summary>The shared complete-transaction publication gate.</summary>
    private readonly MockInterceptionPublicationGate publicationGate;

    /// <summary>Zero while active and one after rollback begins.</summary>
    private int disposed;

    /// <summary>Creates ownership for one fully active route set.</summary>
    internal MockInterceptionActivation(
        IMockInterceptionRouteLifecycle lifecycle,
        ImmutableArray<MockInterceptionRoute> routes,
        MockInterceptionPublicationGate publicationGate)
    {
        this.lifecycle = lifecycle;
        this.routes = routes;
        this.publicationGate = publicationGate;
    }

    /// <summary>Gets whether this complete activation still owns active routes.</summary>
    public bool IsActive =>
        Volatile.Read(ref disposed) == 0 &&
        publicationGate.IsPublished;

    /// <summary>Gets the exact routes in activation order.</summary>
    public ImmutableArray<MockInterceptionRoute> Routes => routes;

    /// <summary>Rolls every route back in reverse activation order exactly once.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
            return;

        publicationGate.Unpublish();
        List<Exception>? failures = null;
        for (var index = routes.Length - 1; index >= 0; --index)
        {
            var route = routes[index];
            try
            {
                lifecycle.Rollback(route);
                route.ReleaseOwnership();
            }
            catch (Exception exception)
            {
                route.PoisonOwnership();
                failures ??= [];
                failures.Add(exception);
            }
        }

        if (failures is not null)
        {
            throw new AggregateException(
                "One or more interception routes failed to roll back.",
                failures);
        }
    }
}

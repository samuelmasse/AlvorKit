namespace AlvorKit;

/// <summary>Pairs one coordinator gate with its exact managed trampoline.</summary>
internal sealed class ProfiledRouteBinding(
    MockInterceptionRoute route,
    IInterceptionHandlerTrampoline trampoline)
{
    /// <summary>Gets the coordinator-owned route gate.</summary>
    internal MockInterceptionRoute Route { get; } = route;

    /// <summary>Gets the exact managed trampoline.</summary>
    internal IInterceptionHandlerTrampoline Trampoline { get; } = trampoline;
}

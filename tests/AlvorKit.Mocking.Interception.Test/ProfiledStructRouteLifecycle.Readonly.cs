namespace AlvorKit;

internal sealed partial class ProfiledStructRouteLifecycle
{
    private static readonly string[] ReadonlyRouteIds =
    [
        ReadId,
    ];

    /// <summary>Gets whether the readonly caller reached active preparation.</summary>
    internal bool AllReadonlyPrepared =>
        ReadonlyRoutes().All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>
    /// Gets whether setup, call, verification, and rejection used the wrapper.
    /// </summary>
    internal bool ReadonlyWrapperEntriesAreExact =>
        routes[ReadId].HandlerInvocations == 7;

    /// <summary>Gets the readonly wrapper count for diagnostics.</summary>
    internal string ReadonlyWrapperEntryCount =>
        $"read={routes[ReadId].HandlerInvocations}";

    /// <summary>Gets whether the readonly caller was restored.</summary>
    internal bool AllReadonlyRemoved =>
        ReadonlyRoutes().All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates the route used by readonly struct behavior.</summary>
    internal static MockInterceptionRoute[] CreateReadonlyRoutes() =>
    [
        .. ReadonlyRouteIds.Select(id => new MockInterceptionRoute(id)),
    ];

    private IEnumerable<IProfiledReceiverFreeCallerRoute>
        ReadonlyRoutes() =>
        ReadonlyRouteIds.Select(id => routes[id]);
}

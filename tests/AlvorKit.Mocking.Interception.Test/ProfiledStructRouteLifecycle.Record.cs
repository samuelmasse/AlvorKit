namespace AlvorKit;

internal sealed partial class ProfiledStructRouteLifecycle
{
    private static readonly string[] RecordRouteIds =
    [
        RecordId,
    ];

    /// <summary>Gets whether the record caller reached active preparation.</summary>
    internal bool AllRecordPrepared =>
        RecordRoutes().All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>
    /// Gets whether setup, calls, and verification used only the record wrapper.
    /// </summary>
    internal bool RecordWrapperEntriesAreExact =>
        routes[RecordId].HandlerInvocations == 8;

    /// <summary>Gets the record wrapper count for diagnostics.</summary>
    internal string RecordWrapperEntryCount =>
        $"record={routes[RecordId].HandlerInvocations}";

    /// <summary>Gets whether the record caller was restored.</summary>
    internal bool AllRecordRemoved =>
        RecordRoutes().All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates the route used by record-struct behavior.</summary>
    internal static MockInterceptionRoute[] CreateRecordRoutes() =>
    [
        .. RecordRouteIds.Select(id => new MockInterceptionRoute(id)),
    ];

    private IEnumerable<IProfiledReceiverFreeCallerRoute>
        RecordRoutes() =>
        RecordRouteIds.Select(id => routes[id]);
}

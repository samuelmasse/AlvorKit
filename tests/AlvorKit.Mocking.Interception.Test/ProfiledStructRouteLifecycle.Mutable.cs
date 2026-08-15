namespace AlvorKit;

internal sealed partial class ProfiledStructRouteLifecycle
{
    private static readonly string[] MutableRouteIds =
    [
        AddId,
        FieldId,
        StaticFieldId,
        ArrayId,
    ];

    /// <summary>Gets whether every mutable caller reached active preparation.</summary>
    internal bool AllMutablePrepared =>
        MutableRoutes().All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether setup, calls, and verification entered exact wrappers.</summary>
    internal bool MutableWrapperEntriesAreExact =>
        routes[AddId].HandlerInvocations == 4 &&
        routes[FieldId].HandlerInvocations == 1 &&
        routes[StaticFieldId].HandlerInvocations == 1 &&
        routes[ArrayId].HandlerInvocations == 1;

    /// <summary>Gets the exact mutable wrapper counts for diagnostics.</summary>
    internal string MutableWrapperEntryCounts =>
        $"add={routes[AddId].HandlerInvocations}, " +
        $"field={routes[FieldId].HandlerInvocations}, " +
        $"static={routes[StaticFieldId].HandlerInvocations}, " +
        $"array={routes[ArrayId].HandlerInvocations}";

    /// <summary>Gets whether every mutable caller was restored.</summary>
    internal bool AllMutableRemoved =>
        MutableRoutes().All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates the four routes used by mutable passthrough behavior.</summary>
    internal static MockInterceptionRoute[] CreateMutableRoutes() =>
    [
        .. MutableRouteIds.Select(id => new MockInterceptionRoute(id)),
    ];

    private IEnumerable<IProfiledReceiverFreeCallerRoute>
        MutableRoutes() =>
        MutableRouteIds.Select(id => routes[id]);
}

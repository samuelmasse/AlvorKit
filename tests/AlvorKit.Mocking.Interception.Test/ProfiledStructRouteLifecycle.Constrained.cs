namespace AlvorKit;

internal sealed partial class ProfiledStructRouteLifecycle
{
    private static readonly string[] ConstrainedRouteIds =
    [
        ConstrainedId,
    ];

    /// <summary>Gets whether the constrained caller reached active preparation.</summary>
    internal bool AllConstrainedPrepared =>
        ConstrainedRoutes().All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>
    /// Gets whether the real generic caller closes to the concrete ref receiver.
    /// </summary>
    internal bool ConstrainedCallerHasExactConcreteReceiver
    {
        get
        {
            MethodInfo caller = ClosedCaller(
                typeof(ProfiledStructConstrainedCaller),
                nameof(ProfiledStructConstrainedCaller.Selected));
            Type[] arguments = caller.GetGenericArguments();
            ParameterInfo[] parameters = caller.GetParameters();
            return arguments.Length == 1 &&
                arguments[0] == typeof(ProfiledMutableStructTarget) &&
                parameters.Length == 2 &&
                parameters[0].ParameterType ==
                    typeof(ProfiledMutableStructTarget).MakeByRefType();
        }
    }

    /// <summary>
    /// Gets whether setup, call, and verification used only the constrained wrapper.
    /// </summary>
    internal bool ConstrainedWrapperEntriesAreExact =>
        routes[ConstrainedId].HandlerInvocations == 6;

    /// <summary>
    /// Gets whether passthrough setup, call, and verification were exact.
    /// </summary>
    internal bool ConstrainedPassthroughWrapperEntriesAreExact =>
        routes[ConstrainedId].HandlerInvocations == 5;

    /// <summary>Gets the constrained wrapper count for diagnostics.</summary>
    internal string ConstrainedWrapperEntryCount =>
        $"constrained={routes[ConstrainedId].HandlerInvocations}";

    /// <summary>Gets whether the constrained caller was restored.</summary>
    internal bool AllConstrainedRemoved =>
        ConstrainedRoutes().All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates the route used by constrained struct behavior.</summary>
    internal static MockInterceptionRoute[] CreateConstrainedRoutes() =>
    [
        .. ConstrainedRouteIds.Select(
            id => new MockInterceptionRoute(id)),
    ];

    private IEnumerable<IProfiledReceiverFreeCallerRoute>
        ConstrainedRoutes() =>
        ConstrainedRouteIds.Select(id => routes[id]);
}

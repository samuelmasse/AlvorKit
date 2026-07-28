namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Publishes one exact post-initializer constructor route.</summary>
internal static class ProfiledConstructorBodyRoute
{
    private static ProfiledDelegateRouteBinding<
        ProfiledConstructorBodyRemainder>? binding;

    /// <summary>Runs the active Mocking wrapper or the extracted original remainder.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void Invoke(
        ProfiledConstructorBodyTarget target,
        int value)
    {
        ProfiledDelegateRouteBinding<ProfiledConstructorBodyRemainder> current =
            Volatile.Read(ref binding) ??
            throw new InvalidOperationException(
                "The constructor remainder route is not bound.");
        if (!current.TryAcquire(out IDisposable? lease))
        {
            current.Original(target, value);
            return;
        }
        using (lease)
        {
            (current.Route.IsActivated
                ? current.Wrapper
                : current.Original)(target, value);
        }
    }

    /// <summary>Binds the extracted original and Mocking wrapper while inert.</summary>
    internal static void Bind(
        MockInterceptionRoute value,
        ProfiledConstructorBodyRemainder originalRemainder,
        ProfiledConstructorBodyRemainder interceptionWrapper)
    {
        Volatile.Write(
            ref binding,
            new(value, originalRemainder, interceptionWrapper));
    }

    /// <summary>Retires the route while retaining its original fallback.</summary>
    internal static void Clear()
    {
        ProfiledDelegateRouteBinding<ProfiledConstructorBodyRemainder>?
            retired = Volatile.Read(ref binding);
        retired?.RetireAndDrain();
    }
}

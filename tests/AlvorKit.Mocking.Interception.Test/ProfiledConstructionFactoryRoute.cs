namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Publishes one exact construction wrapper behind the coordinator gate.</summary>
internal static class ProfiledConstructionFactoryRoute
{
    private static ProfiledDelegateRouteBinding<
        ProfiledConstructionFactoryOperation>? binding;

    /// <summary>Runs the active Mocking wrapper or exact original newobj delegate.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static ProfiledConstructionFactoryTarget Invoke(int value)
    {
        ProfiledDelegateRouteBinding<ProfiledConstructionFactoryOperation>
            current = Volatile.Read(ref binding) ??
            throw new InvalidOperationException(
                "The construction route is not bound.");
        if (!current.TryAcquire(out IDisposable? lease))
            return current.Original(value);
        using (lease)
        {
            return (current.Route.IsActivated
                ? current.Wrapper
                : current.Original)(value);
        }
    }

    /// <summary>Binds original and intercepted construction paths while inert.</summary>
    internal static void Bind(
        MockInterceptionRoute value,
        ProfiledConstructionFactoryOperation originalConstruction,
        ProfiledConstructionFactoryOperation interceptionWrapper)
    {
        Volatile.Write(
            ref binding,
            new(value, originalConstruction, interceptionWrapper));
    }

    /// <summary>Retires the route while retaining its original fallback.</summary>
    internal static void Clear()
    {
        ProfiledDelegateRouteBinding<ProfiledConstructionFactoryOperation>?
            retired = Volatile.Read(ref binding);
        retired?.RetireAndDrain();
    }
}

namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Publishes one construction-validation wrapper behind the coordinator gate.</summary>
internal static class ProfiledConstructionValidationRoute
{
    private static ProfiledDelegateRouteBinding<
        ProfiledConstructionValidationOperation>? binding;

    /// <summary>Runs the active Mocking wrapper or exact original newobj delegate.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static ProfiledConstructionValidationTarget Invoke(
        int value)
    {
        ProfiledDelegateRouteBinding<ProfiledConstructionValidationOperation>
            current = Volatile.Read(ref binding) ??
            throw new InvalidOperationException(
                "The construction-validation route is not bound.");
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
        ProfiledConstructionValidationOperation originalConstruction,
        ProfiledConstructionValidationOperation interceptionWrapper)
    {
        Volatile.Write(
            ref binding,
            new(value, originalConstruction, interceptionWrapper));
    }

    /// <summary>Retires the route while retaining its original fallback.</summary>
    internal static void Clear()
    {
        ProfiledDelegateRouteBinding<ProfiledConstructionValidationOperation>?
            retired = Volatile.Read(ref binding);
        retired?.RetireAndDrain();
    }
}

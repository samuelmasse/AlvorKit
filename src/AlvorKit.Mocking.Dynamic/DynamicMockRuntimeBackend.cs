namespace AlvorKit.Mocking;

/// <summary>Implements runtime-emitted proxy and callback execution.</summary>
internal sealed class DynamicMockRuntimeBackend :
    IMockProxyCallbackBackend
{
    /// <summary>Gets the singleton selected by the optional package facade.</summary>
    internal static DynamicMockRuntimeBackend Instance { get; } = new();

    private DynamicMockRuntimeBackend()
    {
    }

    /// <inheritdoc />
    public string Name => "dynamic";

    /// <inheritdoc />
    [return: DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.NonPublicConstructors)]
    public Type ResolveMockType(Type mockedType) =>
        mockedType.IsSealed
            ? mockedType
            : Proxies.Get(mockedType);

    /// <inheritdoc />
    public void PrepareCapture(Delegate capture) =>
        MockDynamicGenericCallsite.Prepare(capture);

    /// <inheritdoc />
    public Delegate NormalizeCallback(
        Delegate callback,
        MethodInfo capturedMethod)
    {
        if (capturedMethod.IsGenericMethod &&
            !MockInterceptionMethodRegistry.Contains(capturedMethod))
        {
            return NormalizeProxyGenericCallback(
                callback,
                capturedMethod);
        }

        return NormalizeExactCallback(
            callback,
            capturedMethod);
    }

    /// <inheritdoc />
    public Delegate NormalizeConstructorCallback(
        Delegate callback,
        MethodInfo logicalMethod) =>
        MockConstructorCallbackAdapter.Normalize(
            callback,
            logicalMethod);

    private static Delegate NormalizeProxyGenericCallback(
        Delegate callback,
        MethodInfo method)
    {
        if (method.DeclaringType is null ||
            !typeof(IMock).IsAssignableFrom(method.DeclaringType))
        {
            throw new MockException(
                $"Unowned generic method '{method.Name}' cannot retain " +
                "a typed callback because its runtime body may be shared.");
        }

        Type? delegateType =
            MockTypedCallbackDelegateShape.Create(method) ?? throw new MockException(
                $"Proxy-owned generic method '{method.Name}' requires an " +
                "exact reference-shaped or wide callback delegate that is not " +
                "yet representable by the direct standard delegate path.");
        Delegate normalized =
            NormalizeExactCallback(callback, method);
        if (normalized.GetType() != delegateType)
        {
            throw new MockException(
                $"The callback for proxy-owned generic method '{method.Name}' " +
                "did not normalize to its closed construction.");
        }

        return normalized;
    }

    private static Delegate NormalizeExactCallback(
        Delegate callback,
        MethodInfo method)
    {
        Type stableType =
            MockTypedCallbackDelegateCache.GetOrCreate(method);
        MockTypedCallbackContract.ValidateInvoke(
            stableType.GetMethod(nameof(Action.Invoke))!,
            method);

        try
        {
            return Delegate.CreateDelegate(
                stableType,
                callback.Target,
                callback.Method,
                true)!;
        }
        catch (ArgumentException)
        {
            throw new MockException(
                "The callback could not be normalized to the stable exact " +
                "delegate shape.");
        }
    }
}

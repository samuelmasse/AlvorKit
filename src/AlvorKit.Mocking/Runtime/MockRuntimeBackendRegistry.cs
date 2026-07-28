namespace AlvorKit.Mocking;

/// <summary>
/// Owns independent process-wide proxy/callback and operation-interception
/// capability selection without retaining mock or test state.
/// </summary>
internal static class MockRuntimeBackendRegistry
{
    private static readonly Lock Sync = new();
    private static IMockProxyCallbackBackend? proxy;
    private static IMockOperationBackend? operation;

    /// <summary>Gets the selected proxy/callback backend.</summary>
    internal static IMockProxyCallbackBackend Proxy =>
        Volatile.Read(ref proxy) ??
        throw MissingProxyBackend();

    /// <summary>Gets the explicit operation provider when one is selected.</summary>
    internal static IMockOperationBackend? ExplicitOperation =>
        Volatile.Read(ref operation);

    /// <summary>Gets the selected operation-interception provider.</summary>
    internal static IMockOperationBackend Operation =>
        ExplicitOperation ??
        throw MissingOperationBackend();

    /// <summary>
    /// Selects one proxy/callback backend, accepting repeat installation of
    /// the same singleton and rejecting a conflicting proxy runtime.
    /// </summary>
    internal static void RegisterProxy(
        IMockProxyCallbackBackend backend)
    {
        ArgumentNullException.ThrowIfNull(backend);

        lock (Sync)
        {
            if (proxy is null)
            {
                Volatile.Write(ref proxy, backend);
                return;
            }

            if (ReferenceEquals(proxy, backend))
                return;

            throw new MockException(
                $"Mocking proxy/callback backend '{proxy.Name}' is already " +
                $"enabled; cannot also enable '{backend.Name}' in the same " +
                "process.");
        }
    }

    /// <summary>
    /// Selects one operation-interception provider independently of the proxy
    /// backend.
    /// </summary>
    internal static void RegisterOperation(
        IMockOperationBackend backend)
    {
        ArgumentNullException.ThrowIfNull(backend);

        lock (Sync)
        {
            if (operation is null)
            {
                Volatile.Write(ref operation, backend);
                return;
            }

            if (ReferenceEquals(operation, backend))
                return;

            throw new MockException(
                $"Mocking operation-interception provider " +
                $"'{operation.Name}' is already enabled; cannot also enable " +
                $"'{backend.Name}' in the same process.");
        }
    }

    /// <summary>Creates the actionable proxy selection failure.</summary>
    internal static MockException MissingProxyBackend() =>
        new(
            "No proxy/callback mocking backend is enabled. Reference " +
            "AlvorKit.Mocking.Dynamic and call MockDynamic.Enable() for JIT " +
            "execution.");

    /// <summary>Creates the actionable operation-provider failure.</summary>
    internal static MockException MissingOperationBackend() =>
        new(
            "No operation-interception provider is enabled. Enable the " +
            "Interception provider for concrete and receiver-free operations " +
            "by referencing AlvorKit.Mocking.Interception and calling " +
            "MockInterception.Enable().");
}

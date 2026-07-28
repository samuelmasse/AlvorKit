namespace AlvorKit.Mocking.Dynamic;

/// <summary>Selects the optional runtime-emitted proxy and callback backend.</summary>
public static class MockDynamic
{
    /// <summary>
    /// Enables dynamic proxy/callback execution. Repeated calls are
    /// idempotent, and an operation-interception provider may coexist with
    /// this proxy capability.
    /// </summary>
    public static void Enable() =>
        MockRuntimeBackendRegistry.RegisterProxy(
            DynamicMockRuntimeBackend.Instance);
}

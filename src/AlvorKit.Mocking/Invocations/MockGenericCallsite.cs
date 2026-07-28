namespace AlvorKit.Mocking;

/// <summary>Routes generic capture preparation through the selected backend.</summary>
internal static class MockGenericCallsite
{
    /// <summary>Prepares backend-specific generic call-site metadata.</summary>
    internal static void Prepare(Delegate capture) =>
        MockRuntimeBackendRegistry.Proxy.PrepareCapture(capture);
}

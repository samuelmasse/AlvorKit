namespace AlvorKit;

/// <summary>Provides stable history labels for owned instance-dispatch backends.</summary>
internal static class MockBackendLabel
{
    /// <summary>Identifies calls dispatched by a generated proxy override.</summary>
    internal const string ProxyInstance = "dynamic-instance";

    /// <summary>Identifies calls dispatched through instance interception.</summary>
    internal const string InterceptionInstance = "interception-instance";

    /// <summary>Identifies calls dispatched through receiver-free interception.</summary>
    internal const string InterceptionReceiverFree =
        "interception-receiver-free";

    /// <summary>Returns the stable history label for an instance backend.</summary>
    internal static string For(
        MockBackendKind backend,
        MockOperationKind operation) =>
        (backend, operation) switch
        {
            (MockBackendKind.Proxy, MockOperationKind.InstanceMethod) =>
                ProxyInstance,
            (MockBackendKind.Interception, MockOperationKind.InstanceMethod) =>
                InterceptionInstance,
            (MockBackendKind.Interception, _) =>
                InterceptionReceiverFree,
            _ => throw new ArgumentOutOfRangeException(
                nameof(backend)),
        };
}

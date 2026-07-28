namespace AlvorKit.Mocking;

internal static partial class Capture
{
    /// <summary>Runs one value-returning setup or verification capture operation.</summary>
    internal static MockCapturedInvocation Run<T>(
        CaptureOperation operation,
        Func<T> invoke)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(invoke);
        MockGenericCallsite.Prepare(invoke);

        return RunPrepared(operation, null, () => invoke());
    }

    /// <summary>
    /// Runs one value-returning capture for an expected receiver-free kind.
    /// </summary>
    internal static MockCapturedInvocation Run<T>(
        CaptureOperation operation,
        MockInvocationOperationKind expectedOperationKind,
        Func<T> invoke)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(invoke);
        MockGenericCallsite.Prepare(invoke);
        return RunPrepared(
            operation,
            expectedOperationKind,
            () => invoke());
    }
}

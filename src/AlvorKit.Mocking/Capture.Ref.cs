namespace AlvorKit;

internal static partial class Capture
{
    /// <summary>Runs one mutable managed-reference capture operation.</summary>
    internal static MockCapturedInvocation RunRef<T>(
        CaptureOperation operation,
        MockRefCall<T> invoke)
    {
        ArgumentNullException.ThrowIfNull(invoke);
        MockGenericCallsite.Prepare(invoke);
        return RunPrepared(operation, null, () => invoke());
    }

    /// <summary>Runs one read-only managed-reference capture operation.</summary>
    internal static MockCapturedInvocation RunRefReadonly<T>(
        CaptureOperation operation,
        MockRefReadonlyCall<T> invoke)
    {
        ArgumentNullException.ThrowIfNull(invoke);
        MockGenericCallsite.Prepare(invoke);
        return RunPrepared(operation, null, () => invoke());
    }
}

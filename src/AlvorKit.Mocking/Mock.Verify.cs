namespace AlvorKit;

public static partial class Mock
{
    /// <summary>Captures a value-returning mocked call for count verification.</summary>
    public static MockVerification Verify<T>(Func<T> func)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(func);

        var captured = Capture.Run(
            CaptureOperation.Verification,
            func);
        if (captured.Mocked.ReceiverFree is not null)
        {
            return new(
                MockReceiverFreeApiBoundary.Verification(captured));
        }

        return new(captured);
    }

    /// <summary>Captures a void mocked call for count verification.</summary>
    public static MockVerification Verify(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var captured = Capture.Run(
            CaptureOperation.Verification,
            action);
        if (captured.Mocked.ReceiverFree is not null)
        {
            return new(
                MockReceiverFreeApiBoundary.Verification(captured));
        }

        return new(captured);
    }

    /// <summary>Verifies that one mock has no remaining unverified invocations.</summary>
    public static void VerifyNoOtherCalls(object mock)
    {
        ArgumentNullException.ThrowIfNull(mock);

        var mocked = GetMocked(mock) ?? throw new MockException(
                MockDiagnostics.NonMockTarget(
                    "verify invocations",
                    mock));
        var message = MockDiagnostics.NoOtherCalls(
            mocked,
            mocked.Invocations.Snapshot().Invocations);
        if (message is not null)
            throw new MockException(message);
    }
}

namespace AlvorKit.Mocking;

public static partial class Mock
{
    /// <summary>Starts a new invocation-history epoch while retaining configured behavior.</summary>
    public static void ClearInvocations(object mock)
    {
        ArgumentNullException.ThrowIfNull(mock);

        var mocked = GetMocked(mock) ?? throw new MockException(
                MockDiagnostics.NonMockTarget(
                    "clear invocations",
                    mock));
        mocked.Invocations.ClearEpoch();
    }
}

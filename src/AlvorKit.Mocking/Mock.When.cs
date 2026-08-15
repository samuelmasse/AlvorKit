namespace AlvorKit;

public static partial class Mock
{
    /// <summary>Captures a mutable managed-reference return for stable alias configuration.</summary>
    public static MockRefSetupClause<T> WhenRef<T>(
        MockRefCall<T> call)
    {
        ArgumentNullException.ThrowIfNull(call);

        var captured = Capture.RunRef(
            CaptureOperation.Setup,
            call);
        return new(
            captured.Mocked,
            captured.Method,
            captured.CarrierArguments);
    }

    /// <summary>Captures a read-only managed-reference return for stable alias configuration.</summary>
    public static MockRefReadonlySetupClause<T> WhenRefReadonly<T>(
        MockRefReadonlyCall<T> call)
    {
        ArgumentNullException.ThrowIfNull(call);

        var captured = Capture.RunRefReadonly(
            CaptureOperation.Setup,
            call);
        return new(
            captured.Mocked,
            captured.Method,
            captured.CarrierArguments);
    }

    /// <summary>Captures a mocked method or property getter and returns a clause used to configure its return value.</summary>
    public static MockSetupClause<T> When<T>(Func<T> func)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(func);

        var captured = Capture.Run(
            CaptureOperation.Setup,
            func);
        if (captured.Mocked.ReceiverFree is not null)
        {
            return new(
                MockReceiverFreeApiBoundary.Setup(captured));
        }

        return new(
            captured.Mocked,
            captured.Method,
            captured.CarrierArguments);
    }

    /// <summary>Captures a mocked void method or property setter and returns a clause used to configure reference outputs.</summary>
    public static MockSetupClause When(Action func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var captured = Capture.Run(
            CaptureOperation.Setup,
            func);
        if (captured.Mocked.ReceiverFree is not null)
        {
            return new(
                MockReceiverFreeApiBoundary.Setup(captured));
        }

        return new(
            captured.Mocked,
            captured.Method,
            captured.CarrierArguments);
    }
}

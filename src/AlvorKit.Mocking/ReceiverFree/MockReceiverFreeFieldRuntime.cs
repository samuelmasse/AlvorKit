namespace AlvorKit;

/// <summary>
/// Executes exact field observers and transformers without boxing live values.
/// </summary>
internal static class MockReceiverFreeFieldRuntime
{
    /// <summary>Applies a selected write behavior before the original store.</summary>
    internal static void ApplyWrite<T>(
        MockDispatchContinuation continuation,
        scoped ref T value)
        where T : allows ref struct =>
        Apply(continuation, ref value);

    /// <summary>Applies a selected read behavior after the original load.</summary>
    internal static void ApplyRead<T>(
        MockDispatchContinuation continuation,
        scoped ref T value)
        where T : allows ref struct =>
        Apply(continuation, ref value);

    private static void Apply<T>(
        MockDispatchContinuation continuation,
        scoped ref T value)
        where T : allows ref struct
    {
        if (!continuation.IsReceiverFreeFieldBehavior)
            return;

        try
        {
            if (continuation.ReceiverFreeFieldBehaviorKind ==
                MockReceiverFreeBehaviorKind.Observe)
            {
                var observer =
                    (MockValueObserver<T>)
                    continuation.ReceiverFreeFieldCallback;
                observer(in value);
                return;
            }

            var transform =
                (MockValueTransform<T>)
                continuation.ReceiverFreeFieldCallback;
            value = transform(in value);
        }
        catch (Exception exception)
        {
            continuation.CompleteReceiverFreeBehaviorThrown(
                exception);
            throw;
        }
    }
}

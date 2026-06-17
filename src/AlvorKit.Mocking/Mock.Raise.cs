namespace AlvorKit.Mocking;

public static partial class Mock
{
    /// <summary>Raises a mocked event captured through an add or remove expression.</summary>
    public static void Raise(Action action, params object[] args)
    {
        Delegate? handler = null;

        try
        {
            Capture.Start();
            action.Invoke();

            if (Capture.Context.Instance is null || Capture.Context.Method is null || Capture.Context.Args is null)
                throw new MockException("Failed to capture an event invocation. Ensure the event exists on the mock.");

            var mocked = GetMocked(Capture.Context.Instance)!;
            var ev = Events.Get(mocked, Capture.Context.Method) ?? throw new MockException(
                $"No matching event found for method '{Capture.Context.Method.Name}' on type '{mocked.Type.Type.FullName}'.");

            if (Capture.Context.Args[0] != null)
                throw new MockException("Invoking event should be done with += null of -= null, not with a real method");

            if (mocked.HasEventHandlers)
                mocked.EventHandlers.TryGetValue(ev, out handler);
        }
        finally
        {
            Capture.End();
        }

        handler?.DynamicInvoke(args);
    }
}

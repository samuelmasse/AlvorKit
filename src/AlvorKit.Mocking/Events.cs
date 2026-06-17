namespace AlvorKit.Mocking;

/// <summary>Maps intercepted event accessor calls to mocked event handler storage.</summary>
internal static class Events
{
    /// <summary>Finds the event represented by an intercepted add or remove accessor.</summary>
    internal static EventInfo? Get(Mocked mocked, MethodInfo method)
    {
        if (mocked.Type.Events.TryGetValue(method, out var val))
            return val;

        lock (mocked)
        {
            string key = method.Name;

            if (method.Name.StartsWith("add_"))
                key = method.Name[4..];
            else if (method.Name.StartsWith("remove_"))
                key = method.Name[7..];

            var ev = mocked.Type.Type.GetEvent(key);
            if (ev != null)
            {
                mocked.Type.Events.TryAdd(method, ev);
                return ev;
            }
            else
            {
                mocked.Type.Events.TryAdd(method, null);
                return null;
            }
        }
    }

    /// <summary>Applies add and remove event accessor calls to the mock's delegate table.</summary>
    internal static void HandleAddAndRemove(Mocked mocked, MethodInfo method, EventInfo ev, Delegate handler)
    {
        lock (mocked)
        {
            if (method.Name.StartsWith("add_"))
            {
                if (mocked.EventHandlers.TryGetValue(ev, out var value))
                    mocked.EventHandlers[ev] = Delegate.Combine(value, handler);
                else mocked.EventHandlers.TryAdd(ev, handler);
            }
            else if (mocked.EventHandlers.TryGetValue(ev, out var value))
            {
                var newVal = Delegate.Remove(value, handler);
                if (newVal == null)
                    mocked.EventHandlers.TryRemove(ev, out var _);
                else mocked.EventHandlers[ev] = newVal;
            }
        }
    }
}

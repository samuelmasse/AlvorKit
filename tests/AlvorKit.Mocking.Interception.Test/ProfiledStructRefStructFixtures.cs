namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Tracks exact original bodies for the struct/ref-struct row.</summary>
internal static class ProfiledStructRefStructOriginalCounters
{
    private static int observe;
    private static int window;

    internal static int Observe => Volatile.Read(ref observe);
    internal static int Window => Volatile.Read(ref window);

    internal static void RecordObserve() =>
        Interlocked.Increment(ref observe);

    internal static void RecordWindow() =>
        Interlocked.Increment(ref window);

    internal static void Reset()
    {
        Volatile.Write(ref observe, 0);
        Volatile.Write(ref window, 0);
    }
}

/// <summary>Provides live caller storage around ref-struct operations.</summary>
public struct ProfiledStructRefStructTarget(int value)
{
    public int Value = value;

    public int Observe(Span<int> values)
    {
        ProfiledStructRefStructOriginalCounters.RecordObserve();
        Value += values[0];
        return Value;
    }

    public readonly ProfiledStructWindow Window(int[] owner)
    {
        ProfiledStructRefStructOriginalCounters.RecordWindow();
        return new(owner);
    }
}

/// <summary>Owns storage returned by an exact stack-only factory.</summary>
internal sealed class ProfiledStructBehaviorWindowOwner(int[] values)
{
    internal int Calls { get; private set; }

    internal ProfiledStructWindow Create()
    {
        Calls++;
        return new(values);
    }

    internal void Set(int index, int value) =>
        values[index] = value;
}

public delegate int ProfiledStructSpanOperation(
    ref ProfiledStructRefStructTarget target,
    Span<int> values);

public delegate ProfiledStructWindow ProfiledStructBorrowedWindowOperation(
    ref ProfiledStructRefStructTarget target,
    int[] owner);

/// <summary>Preserves untouched struct/ref-struct operations.</summary>
internal static class ProfiledStructRefStructOriginal
{
    internal static int Observe(
        ref ProfiledStructRefStructTarget target,
        Span<int> values) =>
        target.Observe(values);

    internal static ProfiledStructWindow Window(
        ref ProfiledStructRefStructTarget target,
        int[] owner) =>
        target.Window(owner);
}

/// <summary>Counts and enters the exact span-input wrapper.</summary>
internal sealed class ProfiledStructSpanHandler(
    ProfiledStructSpanOperation wrapper) :
    ProfiledReceiverFreeHandler
{
    public int Invoke(
        ref ProfiledStructRefStructTarget target,
        Span<int> values)
    {
        Record();
        return wrapper(ref target, values);
    }
}

/// <summary>Counts and enters the exact borrowed-window wrapper.</summary>
internal sealed class ProfiledStructBorrowedWindowHandler(
    ProfiledStructBorrowedWindowOperation wrapper) :
    ProfiledReceiverFreeHandler
{
    public ProfiledStructWindow Invoke(
        ref ProfiledStructRefStructTarget target,
        int[] owner)
    {
        Record();
        return wrapper(ref target, owner);
    }
}

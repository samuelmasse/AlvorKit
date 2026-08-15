namespace AlvorKit;

/// <summary>Tracks exact original struct-method executions.</summary>
internal static class ProfiledStructOriginalCounters
{
    private static int add;
    private static int constrained;
    private static int read;
    private static int recordRead;
    private static int window;

    internal static int Add => Volatile.Read(ref add);
    internal static int Constrained => Volatile.Read(ref constrained);
    internal static int Read => Volatile.Read(ref read);
    internal static int RecordRead => Volatile.Read(ref recordRead);
    internal static int Window => Volatile.Read(ref window);

    internal static void RecordAdd() => Interlocked.Increment(ref add);
    internal static void RecordConstrained() =>
        Interlocked.Increment(ref constrained);
    internal static void IncrementRead() => Interlocked.Increment(ref read);
    internal static void IncrementRecordRead() =>
        Interlocked.Increment(ref recordRead);
    internal static void RecordWindow() =>
        Interlocked.Increment(ref window);

    internal static void Reset()
    {
        Volatile.Write(ref add, 0);
        Volatile.Write(ref constrained, 0);
        Volatile.Write(ref read, 0);
        Volatile.Write(ref recordRead, 0);
        Volatile.Write(ref window, 0);
    }
}

/// <summary>Provides mutable unmanaged storage for profiled struct calls.</summary>
public struct ProfiledMutableStructTarget(int value) :
    IProfiledStructMetric
{
    public int Value = value;

    public int Add(int amount)
    {
        ProfiledStructOriginalCounters.RecordAdd();
        Value += amount;
        return Value;
    }

    public readonly ProfiledStructWindow Window(int[] owner)
    {
        ProfiledStructOriginalCounters.RecordWindow();
        return new(owner);
    }

    public int Measure(int amount)
    {
        ProfiledStructOriginalCounters.RecordConstrained();
        Value += amount;
        return Value;
    }
}

/// <summary>Provides a readonly value receiver.</summary>
public readonly struct ProfiledReadonlyStructTarget(int value)
{
    public int Value { get; } = value;

    public int Read(int amount)
    {
        ProfiledStructOriginalCounters.IncrementRead();
        return Value + amount;
    }
}

/// <summary>Provides a readonly record value receiver.</summary>
public readonly record struct ProfiledRecordStructTarget(int Value)
{
    public int Read(int amount)
    {
        ProfiledStructOriginalCounters.IncrementRecordRead();
        return Value + amount;
    }
}

/// <summary>Defines one constrained struct operation.</summary>
public interface IProfiledStructMetric
{
    int Measure(int amount);
}

/// <summary>Returns a borrowed view over caller-owned array storage.</summary>
public readonly ref struct ProfiledStructWindow(int[] owner)
{
    public ReadOnlySpan<int> Values => owner;
}

/// <summary>Owns mutable instance and static struct fields.</summary>
internal sealed class ProfiledStructStorage(int value)
{
    internal ProfiledMutableStructTarget Target = new(value);
    internal static ProfiledMutableStructTarget StaticTarget;
}

public delegate int ProfiledStructInt32Operation<T>(
    ref T target,
    int amount)
    where T : struct;

public delegate int ProfiledStructReadOnlyInt32Operation<T>(
    in T target,
    int amount)
    where T : struct;

public delegate ProfiledStructWindow ProfiledStructWindowOperation(
    ref ProfiledMutableStructTarget target,
    int[] owner);

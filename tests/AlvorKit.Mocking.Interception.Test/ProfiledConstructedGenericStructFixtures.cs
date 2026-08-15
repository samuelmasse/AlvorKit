namespace AlvorKit;

/// <summary>Tracks original execution independently for every closed receiver type.</summary>
internal static class ProfiledConstructedGenericStructOriginalCounter<T>
    where T : notnull
{
    private static int calls;

    /// <summary>Gets the number of original bodies entered for this construction.</summary>
    internal static int Calls => Volatile.Read(ref calls);

    /// <summary>Records one original body entry.</summary>
    internal static void Record() => Interlocked.Increment(ref calls);

    /// <summary>Resets this construction's counter.</summary>
    internal static void Reset() => Volatile.Write(ref calls, 0);
}

/// <summary>Provides caller-owned storage on one closed generic value receiver.</summary>
public struct ProfiledConstructedGenericStructTarget<T>(T value)
    where T : notnull
{
    /// <summary>Gets the value in the caller-owned receiver storage.</summary>
    public T Value = value;

    /// <summary>Mutates the live receiver and returns the replacement value.</summary>
    public T Echo(T replacement)
    {
        ProfiledConstructedGenericStructOriginalCounter<T>.Record();
        Value = replacement;
        return Value;
    }
}

/// <summary>Describes one exact closed generic value-receiver operation.</summary>
public delegate T ProfiledConstructedGenericStructOperation<T>(
    ref ProfiledConstructedGenericStructTarget<T> target,
    T value)
    where T : notnull;

/// <summary>Preserves the original operation behind one exact typed delegate.</summary>
internal static class ProfiledConstructedGenericStructOriginal
{
    /// <summary>Calls the original body over the same managed reference.</summary>
    internal static T Echo<T>(
        ref ProfiledConstructedGenericStructTarget<T> target,
        T value)
        where T : notnull =>
        target.Echo(value);
}

/// <summary>Counts entries into one closed production Mocking wrapper.</summary>
internal sealed class ProfiledConstructedGenericStructHandler<T>(
    ProfiledConstructedGenericStructOperation<T> wrapper) :
    ProfiledReceiverFreeHandler
    where T : notnull
{
    /// <summary>Invokes the wrapper without copying or boxing its receiver.</summary>
    public T Invoke(
        ref ProfiledConstructedGenericStructTarget<T> target,
        T value)
    {
        Record();
        return wrapper(ref target, value);
    }
}

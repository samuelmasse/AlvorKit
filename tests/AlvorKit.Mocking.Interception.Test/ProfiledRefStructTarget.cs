namespace AlvorKit;

/// <summary>Represents one borrowed value window used by profiled concrete calls.</summary>
public readonly ref struct ProfiledWindow(ReadOnlySpan<int> values)
{
    /// <summary>Gets the live borrowed values.</summary>
    public ReadOnlySpan<int> Values { get; } = values;
}

/// <summary>Owns storage returned through an exact ref-struct factory.</summary>
internal sealed class ProfiledWindowOwner(int[] values)
{
    /// <summary>Gets the number of factory calls.</summary>
    internal int Calls { get; private set; }

    /// <summary>Creates a borrowed view over the owned storage.</summary>
    internal ProfiledWindow Create()
    {
        Calls++;
        return new(values);
    }
}

/// <summary>Sealed target with ref-struct input and return operations.</summary>
public sealed class ProfiledRefStructTarget
{
    private readonly int[] values = [13, 21];

    /// <summary>Gets the number of original ref-struct bodies entered.</summary>
    public int OriginalCalls { get; private set; }

    /// <summary>Observes a borrowed ref-struct input.</summary>
    public int Observe(ProfiledWindow window)
    {
        OriginalCalls++;
        return window.Values.Length;
    }

    /// <summary>Returns a borrowed window over original storage.</summary>
    public ProfiledWindow Window()
    {
        OriginalCalls++;
        return new(values);
    }
}

/// <summary>Exact delegate for the ref-struct input operation.</summary>
public delegate int ProfiledObserveOperation(
    ProfiledRefStructTarget target,
    ProfiledWindow window);

/// <summary>Exact delegate for the ref-struct return operation.</summary>
public delegate ProfiledWindow ProfiledWindowOperation(
    ProfiledRefStructTarget target);

/// <summary>Preserves untouched ref-struct operations for fallback.</summary>
internal static class ProfiledRefStructOriginal
{
    /// <summary>Invokes the untouched ref-struct input operation.</summary>
    internal static int Observe(
        ProfiledRefStructTarget target,
        ProfiledWindow window) =>
        target.Observe(window);

    /// <summary>Invokes the untouched ref-struct return operation.</summary>
    internal static ProfiledWindow Window(
        ProfiledRefStructTarget target) =>
        target.Window();
}

/// <summary>Exposes the ref-struct input wrapper as an exact handler.</summary>
public sealed class ProfiledObserveHandler(ProfiledObserveOperation wrapper)
{
    /// <summary>Invokes the bound ref-struct input wrapper.</summary>
    public int Invoke(
        ProfiledRefStructTarget target,
        ProfiledWindow window) =>
        wrapper(target, window);
}

/// <summary>Exposes the ref-struct return wrapper as an exact handler.</summary>
public sealed class ProfiledWindowHandler(ProfiledWindowOperation wrapper)
{
    /// <summary>Invokes the bound ref-struct return wrapper.</summary>
    public ProfiledWindow Invoke(ProfiledRefStructTarget target) =>
        wrapper(target);
}

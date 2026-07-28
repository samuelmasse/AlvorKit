namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Provides nonvirtual members on a closed concrete generic type.</summary>
public sealed class ProfiledGenericTarget<T>
{
    private readonly T original;

    /// <summary>Creates an original-behavior target.</summary>
    internal ProfiledGenericTarget(T original) =>
        this.original = original;

    /// <summary>Gets the number of original calls.</summary>
    public int Calls { get; private set; }

    /// <summary>Returns the supplied value through the closed generic signature.</summary>
    public T Echo(T value)
    {
        Calls++;
        return value;
    }

    /// <summary>Gets an original value through the closed generic property.</summary>
    public T Value
    {
        get
        {
            Calls++;
            return original;
        }
    }
}

/// <summary>Provides a concrete generic method with independently configured constructions.</summary>
public sealed class ProfiledConstructedGenericTarget
{
    /// <summary>Gets the number of original method bodies entered.</summary>
    public int OriginalCalls { get; private set; }

    /// <summary>Returns the supplied value through one constructed generic signature.</summary>
    public T Echo<T>(T value)
    {
        OriginalCalls++;
        return value;
    }
}

/// <summary>Exact delegate for one closed generic target's echo operation.</summary>
public delegate T ProfiledClosedGenericEchoOperation<T>(
    ProfiledGenericTarget<T> target,
    T value);

/// <summary>Exact delegate for one closed generic target's property getter.</summary>
public delegate T ProfiledClosedGenericValueOperation<T>(
    ProfiledGenericTarget<T> target);

/// <summary>Exact delegate for one construction of a concrete generic method.</summary>
public delegate T ProfiledConstructedGenericEchoOperation<T>(
    ProfiledConstructedGenericTarget target,
    T value);

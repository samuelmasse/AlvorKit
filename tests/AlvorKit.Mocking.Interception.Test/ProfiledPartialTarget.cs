namespace AlvorKit;

/// <summary>Sealed target for the concrete configured-and-original partial scenario.</summary>
public sealed class ProfiledPartialTarget(Exception? failure = null)
{
    private readonly Exception failure = failure ??
        new InvalidOperationException("original concrete failure");

    /// <summary>Gets the number of original concrete method bodies entered.</summary>
    public int OriginalCalls { get; private set; }

    /// <summary>Adds two values in the original implementation.</summary>
    public int Add(int left, int right)
    {
        OriginalCalls++;
        return left + right;
    }

    /// <summary>Returns a distinct unconfigured neighboring result.</summary>
    public int Neighbor(int value)
    {
        OriginalCalls++;
        return value + 40;
    }

    /// <summary>Throws the exact exception supplied at construction.</summary>
    public void ThrowOriginal()
    {
        OriginalCalls++;
        throw failure;
    }

    /// <summary>Mutates reference and output arguments in the original implementation.</summary>
    public int Mutate(ref int value, out int doubled)
    {
        OriginalCalls++;
        value += 3;
        doubled = value * 2;
        return value;
    }
}

/// <summary>Exact delegate for the concrete addition operation.</summary>
public delegate int ProfiledAddOperation(
    ProfiledPartialTarget target,
    int left,
    int right);

/// <summary>Exact delegate for the concrete neighboring operation.</summary>
public delegate int ProfiledNeighborOperation(
    ProfiledPartialTarget target,
    int value);

/// <summary>Exact delegate for the concrete throwing operation.</summary>
public delegate void ProfiledThrowOperation(ProfiledPartialTarget target);

/// <summary>Exact delegate for the concrete ref/out operation.</summary>
public delegate int ProfiledMutateOperation(
    ProfiledPartialTarget target,
    ref int value,
    out int doubled);

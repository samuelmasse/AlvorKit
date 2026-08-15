namespace AlvorKit;

/// <summary>Sealed target for concurrent receiver-isolation behavior.</summary>
public sealed class ProfiledInstanceIsolationTarget
{
    /// <summary>Gets the number of original method bodies entered.</summary>
    public int OriginalCalls { get; private set; }

    /// <summary>Adds two values in the original implementation.</summary>
    public int Add(int left, int right)
    {
        OriginalCalls++;
        return left + right;
    }
}

/// <summary>Exact delegate for the receiver-isolation addition.</summary>
public delegate int ProfiledInstanceIsolationOperation(
    ProfiledInstanceIsolationTarget target,
    int left,
    int right);

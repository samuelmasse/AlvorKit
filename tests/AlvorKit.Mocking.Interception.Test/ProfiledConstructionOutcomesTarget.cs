namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Provides observable original construction for outcome routing.</summary>
public sealed class ProfiledConstructionOutcomesTarget
{
    /// <summary>Creates one target and records original construction.</summary>
    public ProfiledConstructionOutcomesTarget(int value)
    {
        ConstructorCalls++;
        Value = value;
    }

    /// <summary>Counts original constructor executions.</summary>
    public static int ConstructorCalls { get; private set; }

    /// <summary>Gets constructor-initialized state.</summary>
    public int Value { get; }

    /// <summary>Clears original-construction evidence.</summary>
    public static void Reset() => ConstructorCalls = 0;
}

/// <summary>Exact delegate for the construction-outcomes operation.</summary>
public delegate ProfiledConstructionOutcomesTarget
    ProfiledConstructionOutcomesOperation(int value);

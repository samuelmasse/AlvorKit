namespace AlvorKit;

/// <summary>Provides observable construction for factory-result validation.</summary>
public sealed class ProfiledConstructionValidationTarget
{
    /// <summary>Creates one target and records original construction.</summary>
    public ProfiledConstructionValidationTarget(int value)
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

/// <summary>Exact delegate for construction-result validation.</summary>
public delegate ProfiledConstructionValidationTarget
    ProfiledConstructionValidationOperation(int value);

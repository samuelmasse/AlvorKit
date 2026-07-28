namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Provides observable allocation state for factory substitution.</summary>
public sealed class ProfiledConstructionFactoryTarget
{
    /// <summary>Creates one target and records original construction.</summary>
    public ProfiledConstructionFactoryTarget(int value)
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

/// <summary>Exact delegate for the selected construction operation.</summary>
public delegate ProfiledConstructionFactoryTarget
    ProfiledConstructionFactoryOperation(int value);

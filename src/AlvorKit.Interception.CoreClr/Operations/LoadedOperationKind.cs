namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Identifies an exact loaded IL operation supported by caller planning.</summary>
public enum LoadedOperationKind
{
    /// <summary>A direct or virtual call through an ordinary reference receiver.</summary>
    InstanceCall,

    /// <summary>A value-type instance call through a live managed receiver.</summary>
    StructMethod,

    /// <summary>A direct call to a static method.</summary>
    StaticCall,

    /// <summary>An object allocation and constructor invocation.</summary>
    ObjectConstruction,

    /// <summary>A read from a static field.</summary>
    StaticFieldRead,

    /// <summary>A write to a static field.</summary>
    StaticFieldWrite,

    /// <summary>A read through an ordinary reference field receiver.</summary>
    InstanceFieldRead,

    /// <summary>A write through an ordinary reference field receiver.</summary>
    InstanceFieldWrite
}

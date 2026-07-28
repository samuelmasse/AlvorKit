namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Classifies a resolved declaring or constrained type for stack-shape recognition.</summary>
public enum LoadedTypeShape
{
    /// <summary>An ordinary class or other reference type.</summary>
    ReferenceType,

    /// <summary>An interface whose unconstrained receiver is an object reference.</summary>
    Interface,

    /// <summary>A concrete non-nullable value type.</summary>
    ValueType,

    /// <summary>An open generic type parameter without a concrete runtime shape.</summary>
    GenericParameter
}

namespace AlvorKit;

/// <summary>Describes managed-reference returns representable by the exact dispatch ABI.</summary>
internal static class MockManagedReferenceAbi
{
    /// <summary>Returns whether a managed-reference return has a legal generic element type.</summary>
    internal static bool IsSupported(Type returnType)
    {
        if (!returnType.IsByRef)
            return false;

        Type elementType = returnType.GetElementType()!;
        return !elementType.IsByRefLike
            && !elementType.IsPointer
            && !elementType.IsFunctionPointer
            && !elementType.ContainsGenericParameters;
    }

    /// <summary>Creates the internal exact alias-factory delegate type.</summary>
    internal static Type FactoryType(Type returnType) =>
        typeof(MockManagedReferenceFactory<>).MakeGenericType(
            returnType.GetElementType()!);

    /// <summary>Creates the by-reference prefix injection type.</summary>
    internal static Type InjectionType(Type returnType) =>
        FactoryType(returnType).MakeByRefType();
}

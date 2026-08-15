namespace AlvorKit;

/// <summary>
/// Classifies how a method result crosses the CLR call boundary.
/// </summary>
internal enum MockReturnKind
{
    Void,
    Value,
    ManagedReference,
    ReadOnlyManagedReference,
    Pointer,
    FunctionPointer,
    RefStructValue,
}

namespace AlvorKit.Mocking;

/// <summary>
/// Classifies how one declared parameter crosses the CLR call boundary.
/// </summary>
internal enum MockPassingKind
{
    Value,
    ManagedReference,
    Pointer,
    FunctionPointer,
    RefStructValue,
}

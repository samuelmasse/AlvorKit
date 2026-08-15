namespace AlvorKit;

/// <summary>
/// Identifies the interception semantics represented by generated dispatch code.
/// </summary>
internal enum MockOperationKind
{
    InstanceMethod,
    StaticMethod,
    Construction,
    ConstructorBody,
    FieldRead,
    FieldWrite,
    StructMethod,
}

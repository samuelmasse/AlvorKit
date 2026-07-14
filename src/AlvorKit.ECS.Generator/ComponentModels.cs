namespace AlvorKit.ECS.Generator;

internal sealed record PropertyModel(
    string Name,
    string ValueType,
    string NullableType,
    bool AddToString,
    bool LazyInitialize,
    bool Archetypal,
    bool IsDelegate,
    string? Comment,
    string GetAccess,
    string SetAccess);

internal sealed record InterfaceModel(
    string Namespace,
    string InterfaceName,
    string ClassName,
    PropertyModel[] Properties,
    bool SkipBuilder,
    string Access);

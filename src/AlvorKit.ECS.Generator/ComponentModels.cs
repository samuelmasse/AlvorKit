namespace AlvorKit.ECS.Generator;

/// <summary>Describes one component property declared on a component interface.</summary>
/// <param name="Name">The source property name and generated component marker name.</param>
/// <param name="ValueType">The non-nullable-friendly type name used for component metadata.</param>
/// <param name="NullableType">The nullable-aware type name used in generated accessors.</param>
/// <param name="AddToString">Whether the component marker should carry <c>ComponentToStringAttribute</c>.</param>
/// <param name="LazyInitialize">Whether the mutating getter should create a missing component value.</param>
/// <param name="IsDelegate">Whether the generated accessor name needs the <c>Delegate</c> suffix.</param>
/// <param name="Comment">Optional XML documentation copied from the source property.</param>
/// <param name="GetAccess">The generated get-side accessibility.</param>
/// <param name="SetAccess">The generated set-side accessibility.</param>
internal sealed record PropertyModel(
    string Name,
    string ValueType,
    string NullableType,
    bool AddToString,
    bool LazyInitialize,
    bool IsDelegate,
    string? Comment,
    string GetAccess,
    string SetAccess);

/// <summary>Describes one component interface and the source it should generate.</summary>
/// <param name="Namespace">The namespace where generated source should be emitted.</param>
/// <param name="InterfaceName">The source interface name.</param>
/// <param name="ClassName">The generated component group class name.</param>
/// <param name="Properties">The component properties declared by the interface.</param>
/// <param name="SkipBuilder">Whether builder-style mutator extensions should be omitted.</param>
/// <param name="Access">The generated top-level type accessibility.</param>
internal sealed record InterfaceModel(
    string Namespace,
    string InterfaceName,
    string ClassName,
    PropertyModel[] Properties,
    bool SkipBuilder,
    string Access);

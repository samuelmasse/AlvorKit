namespace AlvorKit.Script.Bindgen;

public record BindingEnumMember(string ManagedName, long Value, string? Documentation);

public record BindingEnum(
    string NativeName,
    string ManagedName,
    string UnderlyingType,
    bool IsFlags,
    List<BindingEnumMember> Members,
    string? Documentation);

public record BindingField(string ManagedName, string ManagedType, int Offset, string? Documentation);

public record BindingStruct(
    string NativeName,
    string ManagedName,
    bool IsUnion,
    int Size,
    List<BindingField> Fields,
    string? Documentation);

public record BindingParameter(
    string ManagedName,
    string ManagedType,
    string Modifier,
    bool RequiresUtf8StringMarshalling,
    string? BoolMarshaller,
    bool IsUntypedPointer = false,
    bool IsConstPointee = false,
    bool IsSizeT = false);

public record BindingFunction(
    string NativeName,
    string ManagedName,
    string ReturnType,
    string? ReturnBoolMarshaller,
    List<BindingParameter> Parameters,
    XmlDocComment? Documentation);

public record BindingConstant(string ManagedName, long Value);

public record InlineBufferDefinition(string ManagedName, string ElementType, int Count);

public record BindingModel(
    List<BindingEnum> Enums,
    List<BindingStruct> Structs,
    List<BindingFunction> Functions,
    List<BindingConstant> Constants,
    List<InlineBufferDefinition> InlineBuffers,
    List<string> SkippedFunctions,
    List<string> SizeofTypes);

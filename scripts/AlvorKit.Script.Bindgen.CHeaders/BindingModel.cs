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

public record BindingHandle(string NativeName, string ManagedName);

public record BindingStruct(
    string NativeName,
    string ManagedName,
    bool IsUnion,
    int Size,
    List<BindingField> Fields,
    List<InlineBufferDefinition> NestedBuffers,
    string? Documentation);

/// <summary>
/// Parameter shape after C has been reduced to a safe public type and a raw interop type. The two
/// differ when the public API is nicer than the P/Invoke boundary, such as bool-over-int or a string
/// convenience overload over a raw <c>const char*</c>.
/// </summary>
public record BindingParameter(
    string ManagedName,
    string ManagedType,
    string InteropType,
    string Modifier,
    bool HasStringConvenience,
    bool IsUntypedPointer = false,
    bool IsConstPointee = false,
    bool IsSizeT = false,
    string? CallbackType = null);

public record BindingDelegate(string ManagedName, string ReturnType, List<BindingParameter> Parameters);

public record BindingFunction(
    string NativeName,
    string ManagedName,
    string ReturnType,
    string ReturnInteropType,
    List<BindingParameter> Parameters,
    XmlDocComment? Documentation,
    bool ReturnsCString = false);

public record BindingConstant(string ManagedName, long Value);

public record InlineBufferDefinition(string ManagedName, string ElementType, int Count);

public record BindingModel(
    List<BindingEnum> Enums,
    List<BindingStruct> Structs,
    List<BindingHandle> Handles,
    List<BindingDelegate> Delegates,
    List<BindingFunction> Functions,
    List<BindingConstant> Constants,
    List<string> SkippedFunctions,
    List<string> SizeofTypes);

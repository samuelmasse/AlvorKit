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
/// A function parameter. <see cref="ManagedType"/> is the type on the public contract; <see cref="InteropType"/>
/// is the blittable type at the native P/Invoke boundary (they differ for a bool, whose interop form is the
/// underlying int). <see cref="HasStringConvenience"/> marks a <c>const char*</c> parameter: its raw form is
/// <c>nint</c> and a <c>string</c> convenience overload is derived. No P/Invoke marshalling: the native class
/// stays raw and conversions happen in the backend (bool) or convenience overloads (string).
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

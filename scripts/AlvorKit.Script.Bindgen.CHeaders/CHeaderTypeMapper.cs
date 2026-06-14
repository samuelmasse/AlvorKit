using ClangSharp.Interop;
using ClangType = ClangSharp.Type;

namespace AlvorKit.Script.Bindgen;

/// <summary>Maps Clang types into public managed and raw interop type spellings.</summary>
internal sealed class CHeaderTypeMapper(
    BindgenConfig config,
    CHeaderParseState state,
    Func<string, BindingStruct?> resolveStruct)
{
    /// <summary>Maps a native type to a managed C# type name, or null when unsupported.</summary>
    public string? MapNativeType(CXType type, bool isParam = false, bool isReturn = false, bool boolAsRaw = false)
    {
        var spelling = CHeaderNameMapper.CleanTypeSpelling(type);
        if (!boolAsRaw && IsConfiguredBool(spelling, isParam, isReturn))
            return "bool";
        if (state.EnumByNativeName.TryGetValue(spelling, out var enumType))
            return enumType.ManagedName;
        if (spelling == "size_t")
            return "nuint";

        var canonical = type.CanonicalType;
        var primitive = MapPrimitive(canonical.kind);
        if (primitive is not null)
            return primitive;
        if (canonical.kind == CXTypeKind.CXType_Pointer)
            return MapPointer(type, canonical.PointeeType, isParam);
        if (isParam && canonical.kind is CXTypeKind.CXType_ConstantArray or CXTypeKind.CXType_IncompleteArray)
            return "nint";
        return canonical.kind == CXTypeKind.CXType_Record ? resolveStruct(spelling)?.ManagedName : null;
    }

    /// <summary>Maps an enum underlying integer type into a C# integral type.</summary>
    public static string MapIntegerType(ClangType underlyingType) => underlyingType.CanonicalType.Handle.kind switch
    {
        CXTypeKind.CXType_UChar or CXTypeKind.CXType_Char_U => "byte",
        CXTypeKind.CXType_SChar or CXTypeKind.CXType_Char_S => "sbyte",
        CXTypeKind.CXType_UShort => "ushort",
        CXTypeKind.CXType_Short => "short",
        CXTypeKind.CXType_UInt => "uint",
        CXTypeKind.CXType_ULongLong => "ulong",
        CXTypeKind.CXType_LongLong => "long",
        _ => "int"
    };

    /// <summary>Returns true when configuration treats the type as boolean.</summary>
    private bool IsConfiguredBool(string spelling, bool isParam, bool isReturn) =>
        spelling == $"{config.Prefix}bool" || ((isParam || isReturn) && config.BoolTypes.Contains(spelling));

    /// <summary>Maps primitive Clang kinds to managed C# type names.</summary>
    private static string? MapPrimitive(CXTypeKind kind) => kind switch
    {
        CXTypeKind.CXType_Void => "void",
        CXTypeKind.CXType_Bool => "bool",
        CXTypeKind.CXType_UChar or CXTypeKind.CXType_Char_U => "byte",
        CXTypeKind.CXType_SChar or CXTypeKind.CXType_Char_S => "sbyte",
        CXTypeKind.CXType_UShort => "ushort",
        CXTypeKind.CXType_Short => "short",
        CXTypeKind.CXType_UInt => "uint",
        CXTypeKind.CXType_Int => "int",
        CXTypeKind.CXType_ULong => "CULong",
        CXTypeKind.CXType_Long => "CLong",
        CXTypeKind.CXType_ULongLong => "ulong",
        CXTypeKind.CXType_LongLong => "long",
        CXTypeKind.CXType_Float => "float",
        CXTypeKind.CXType_Double => "double",
        _ => null
    };

    /// <summary>Maps native pointer types to strings, handles, callbacks, or raw pointers.</summary>
    private string MapPointer(CXType original, CXType pointee, bool isParam)
    {
        if (pointee.CanonicalType.kind is CXTypeKind.CXType_FunctionProto or CXTypeKind.CXType_FunctionNoProto)
            return "nint";
        if (isParam && IsConstCharPointer(pointee))
            return "string";
        if (pointee.CanonicalType.kind == CXTypeKind.CXType_Record
            && OpaqueHandle(CHeaderNameMapper.CleanTypeSpelling(pointee)) is { } handle)
            return handle;
        return "nint";
    }

    /// <summary>Returns true for const char pointers accepted by string convenience overloads.</summary>
    private static bool IsConstCharPointer(CXType pointee) =>
        pointee.kind is CXTypeKind.CXType_Char_S or CXTypeKind.CXType_Char_U && pointee.IsConstQualified;

    /// <summary>Returns an opt-in opaque handle for an unknown record pointer.</summary>
    private string? OpaqueHandle(string nativeName)
    {
        if (state.RecordByNativeName.ContainsKey(nativeName) || !config.TypeRenames.TryGetValue(nativeName, out var managed))
            return null;
        state.HandlesByNativeName[nativeName] = managed;
        return managed;
    }
}

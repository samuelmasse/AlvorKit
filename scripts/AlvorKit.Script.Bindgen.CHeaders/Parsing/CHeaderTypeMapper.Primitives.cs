using ClangType = ClangSharp.Type;

namespace AlvorKit.Script.Bindgen;

/// <summary>Maps primitive native C types for <see cref="CHeaderTypeMapper"/>.</summary>
internal sealed partial class CHeaderTypeMapper
{
    /// <summary>Maps an enum underlying integer type into a C# integral type.</summary>
    public static string MapIntegerType(ClangType underlyingType) => MapIntegerKind(underlyingType.CanonicalType.Handle.kind);

    /// <summary>Maps a Clang integer kind into the corresponding C# enum underlying type.</summary>
    internal static string MapIntegerKind(CXTypeKind kind) => kind switch
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

    /// <summary>Returns true for const char pointers accepted by string convenience overloads.</summary>
    private static bool IsConstCharPointer(CXType pointee) =>
        pointee.kind is CXTypeKind.CXType_Char_S or CXTypeKind.CXType_Char_U && pointee.IsConstQualified;

    /// <summary>Maps non-character primitive pointer returns to raw C# pointer types.</summary>
    private static string? PrimitivePointer(CXTypeKind kind) => kind switch
    {
        CXTypeKind.CXType_Bool => "bool*",
        CXTypeKind.CXType_UShort => "ushort*",
        CXTypeKind.CXType_Short => "short*",
        CXTypeKind.CXType_UInt => "uint*",
        CXTypeKind.CXType_Int => "int*",
        CXTypeKind.CXType_ULong => "CULong*",
        CXTypeKind.CXType_Long => "CLong*",
        CXTypeKind.CXType_ULongLong => "ulong*",
        CXTypeKind.CXType_LongLong => "long*",
        CXTypeKind.CXType_Float => "float*",
        CXTypeKind.CXType_Double => "double*",
        _ => null
    };
}

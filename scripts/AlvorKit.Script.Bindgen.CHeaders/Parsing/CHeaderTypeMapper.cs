using ClangSharp.Interop;

namespace AlvorKit.Script.Bindgen;

/// <summary>Maps Clang types into public managed and raw interop type spellings.</summary>
internal sealed partial class CHeaderTypeMapper(
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
        if (config.TypeAliases.TryGetValue(spelling, out var alias))
            return alias;
        if (state.EnumByNativeName.TryGetValue(spelling, out var enumType))
            return enumType.ManagedName;
        if (spelling == "size_t")
            return "nuint";

        var canonical = type.CanonicalType;
        var primitive = MapPrimitive(canonical.kind);
        if (primitive is not null)
            return primitive;
        if (canonical.kind == CXTypeKind.CXType_Pointer)
            return MapPointer(canonical.PointeeType, isParam, isReturn);
        if (isParam && canonical.kind is CXTypeKind.CXType_ConstantArray or CXTypeKind.CXType_IncompleteArray)
            return "nint";
        return canonical.kind == CXTypeKind.CXType_Record ? resolveStruct(spelling)?.ManagedName : null;
    }

    /// <summary>Maps a native type to the raw C# type used at the P/Invoke boundary.</summary>
    public string? MapInteropType(CXType type, bool isParam = false, bool isReturn = false, bool boolAsRaw = false)
    {
        var spelling = CHeaderNameMapper.CleanTypeSpelling(type);
        if (config.InteropTypeAliases.TryGetValue(spelling, out var alias))
        {
            if (type.CanonicalType.kind == CXTypeKind.CXType_Record && resolveStruct(spelling) is null)
                return null;
            return alias;
        }

        return MapNativeType(type, isParam, isReturn, boolAsRaw);
    }

    /// <summary>Returns true when configuration treats the type as boolean.</summary>
    private bool IsConfiguredBool(string spelling, bool isParam, bool isReturn) =>
        spelling == $"{config.Prefix}bool" || ((isParam || isReturn) && config.BoolTypes.Contains(spelling));

    /// <summary>Maps native pointer types to strings, handles, callbacks, or raw pointers.</summary>
    private string MapPointer(CXType pointee, bool isParam, bool isReturn)
    {
        if (pointee.CanonicalType.kind is CXTypeKind.CXType_FunctionProto or CXTypeKind.CXType_FunctionNoProto)
            return "nint";
        if (isParam && IsConstCharPointer(pointee))
            return "string";
        var pointeeName = CHeaderNameMapper.CleanTypeSpelling(pointee);
        if (ConfiguredOpaqueHandle(pointeeName) is { } configuredHandle)
            return configuredHandle;
        if (pointee.CanonicalType.kind == CXTypeKind.CXType_Record && RecordPointer(pointeeName) is { } recordPointer)
            return recordPointer;
        if (pointee.CanonicalType.kind == CXTypeKind.CXType_Record && RenamedOpaqueHandle(pointeeName) is { } renamedHandle)
            return renamedHandle;
        if (isReturn && PrimitivePointer(pointee.CanonicalType.kind) is { } primitivePointer)
            return primitivePointer;
        return "nint";
    }

    /// <summary>Returns an explicitly configured opaque handle for a pointer type.</summary>
    private string? ConfiguredOpaqueHandle(string nativeName)
    {
        if (!config.OpaqueTypes.TryGetValue(nativeName, out var managed))
            return null;
        state.HandlesByNativeName[nativeName] = managed;
        return managed;
    }

    /// <summary>Returns a raw pointer type for a visible native record.</summary>
    private string? RecordPointer(string nativeName)
    {
        if (RecordName(nativeName) is not { } recordName)
            return null;
        return resolveStruct(recordName) is { } record ? record.ManagedName + "*" : null;
    }

    /// <summary>Returns the public name that should emit the visible record.</summary>
    private string? RecordName(string nativeName)
    {
        if (!state.RecordByNativeName.TryGetValue(nativeName, out var record))
            return null;
        if (config.TransparentStructs.Contains(nativeName) || state.StructByNativeName.ContainsKey(nativeName))
            return nativeName;

        var spelling = record.Handle.Spelling.ToString();
        var candidates = state.PublicRecordNames.Concat(config.TransparentStructs).Concat(state.StructByNativeName.Keys);
        return candidates.FirstOrDefault(candidate =>
            state.RecordByNativeName.TryGetValue(candidate, out var transparent)
            && transparent.Handle.Spelling.ToString() == spelling) ?? nativeName;
    }

    /// <summary>Returns a legacy opaque handle for a renamed unknown record pointer.</summary>
    private string? RenamedOpaqueHandle(string nativeName)
    {
        if (state.RecordByNativeName.ContainsKey(nativeName) || !config.TypeRenames.TryGetValue(nativeName, out var managed))
            return null;
        state.HandlesByNativeName[nativeName] = managed;
        return managed;
    }
}

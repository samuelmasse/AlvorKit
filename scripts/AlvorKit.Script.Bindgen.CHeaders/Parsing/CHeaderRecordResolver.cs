namespace AlvorKit.Script.Bindgen;

/// <summary>Resolves native records into managed struct descriptions on demand.</summary>
internal sealed class CHeaderRecordResolver(
    BindgenConfig config,
    CHeaderParseState state,
    CHeaderNameMapper names,
    CHeaderTypeMapper types)
{
    /// <summary>Returns an existing or newly built struct for a native record name.</summary>
    public BindingStruct? ResolveStruct(string nativeName)
    {
        if (state.StructByNativeName.TryGetValue(nativeName, out var existing))
            return existing;
        if (state.FailedStructs.Contains(nativeName) || !state.RecordByNativeName.TryGetValue(nativeName, out var record))
            return null;
        return BuildStruct(record, nativeName);
    }

    /// <summary>Builds a managed struct and records failure for unsupported fields.</summary>
    private BindingStruct? BuildStruct(RecordDecl record, string nativeName)
    {
        var isUnion = record.Handle.Kind == CXCursorKind.CXCursor_UnionDecl;
        var managedName = config.InteropTypeAliases.GetValueOrDefault(nativeName) ?? names.TypeName(nativeName);
        var built = new BindingStruct(nativeName, managedName, isUnion, (int)record.TypeForDecl.Handle.SizeOf, [], [],
            XmlDocComment.Parse(record.Handle.RawCommentText.ToString())?.Summary);
        state.StructByNativeName[nativeName] = built;

        var anonymousFieldIndex = 0;
        foreach (var field in record.Fields)
        {
            var fieldNativeName = IsAnonymousRecordField(field) ? $"anonymous{anonymousFieldIndex++}" : field.Name;
            var fieldManagedName = fieldNativeName.Length == 0
                ? ""
                : CSharpName.FromNativeIdentifier(fieldNativeName, config.Prefix, config.DigitNamePrefix);
            var managedType = fieldNativeName.Length == 0 ? null : MapStructFieldType(field, fieldNativeName, fieldManagedName, built);
            if (managedType is null)
            {
                if (isUnion)
                    continue;
                state.StructByNativeName.Remove(nativeName);
                state.FailedStructs.Add(nativeName);
                return null;
            }

            built.Fields.Add(new(
                fieldManagedName,
                managedType,
                (int)(field.Handle.OffsetOfField / 8),
                XmlDocComment.Member(field.Handle.RawCommentText.ToString()),
                fieldNativeName));
        }

        CHeaderLayoutValidator.ValidateNaturalRecordLayout(state, record, "primary");
        return built;
    }

    /// <summary>Maps a native field to a managed field type.</summary>
    private string? MapStructFieldType(FieldDecl field, string fieldNativeName, string fieldManagedName, BindingStruct owner)
    {
        var canonical = field.Type.Handle.CanonicalType;
        if (canonical.kind == CXTypeKind.CXType_ConstantArray)
            return MapInlineArray(canonical, fieldManagedName, fieldNativeName, owner);
        if (canonical.kind == CXTypeKind.CXType_Record)
            return MapRecordField(field, owner.NativeName, fieldNativeName);
        if (state.DelegatesByNativeName.ContainsKey(CHeaderNameMapper.CleanTypeSpelling(field.Type.Handle)))
            state.UsedCallbackTypedefs.Add(CHeaderNameMapper.CleanTypeSpelling(field.Type.Handle));
        return types.MapNativeType(field.Type.Handle);
    }

    /// <summary>Maps a named or anonymous nested record field.</summary>
    private string? MapRecordField(FieldDecl field, string parentNativeName, string fieldNativeName)
    {
        var nativeTypeName = CHeaderNameMapper.CleanTypeSpelling(field.Type.Handle);
        if (!IsAnonymousName(nativeTypeName) && state.RecordByNativeName.ContainsKey(nativeTypeName))
            return ResolveStruct(nativeTypeName)?.ManagedName;
        if (field.Type.CanonicalType is not RecordType { Decl.Definition: RecordDecl definition })
            return null;

        var synthesizedName = $"{parentNativeName}_{fieldNativeName}";
        if (state.StructByNativeName.TryGetValue(synthesizedName, out var existing))
            return existing.ManagedName;
        if (state.FailedStructs.Contains(synthesizedName))
            return null;
        return BuildStruct(definition, synthesizedName)?.ManagedName;
    }

    /// <summary>Maps a fixed-size array field to a nested inline-buffer type.</summary>
    private string? MapInlineArray(CXType arrayType, string fieldManagedName, string fieldNativeName, BindingStruct owner)
    {
        var count = (int)arrayType.ArraySize;
        var element = arrayType.ElementType;
        var elementType = element.CanonicalType.kind switch
        {
            CXTypeKind.CXType_ConstantArray => MapInlineArray(element.CanonicalType, fieldManagedName + "Row", fieldNativeName, owner),
            CXTypeKind.CXType_Record => ResolveStruct(CHeaderNameMapper.CleanTypeSpelling(element))?.ManagedName,
            _ => types.MapNativeType(element)
        };
        if (elementType is null)
            return null;

        var bufferName = fieldManagedName + "Buffer";
        owner.NestedBuffers.Add(new(bufferName, fieldNativeName, elementType, count));
        return bufferName;
    }

    /// <summary>Returns true for Clang's synthetic field names for anonymous struct and union fields.</summary>
    private static bool IsAnonymousRecordField(FieldDecl field) =>
        field.Type.Handle.CanonicalType.kind == CXTypeKind.CXType_Record && IsAnonymousName(field.Name);

    /// <summary>Returns true for anonymous Clang spellings that are unsuitable as generated C# identifiers.</summary>
    private static bool IsAnonymousName(string name) =>
        name.Length == 0 || name.Contains("(anonymous at ", StringComparison.Ordinal);
}

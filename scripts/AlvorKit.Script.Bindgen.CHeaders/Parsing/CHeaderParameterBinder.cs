namespace AlvorKit.Script.Bindgen;

/// <summary>Binds native function parameters into managed and interop parameter shapes.</summary>
internal sealed class CHeaderParameterBinder(BindgenConfig config, CHeaderParseState state, CHeaderTypeMapper types)
{
    /// <summary>Returns a binding for one native parameter, or records why the function is skipped.</summary>
    public BindingParameter? TryBind(FunctionDecl function, ParmVarDecl parameter, int index)
    {
        var modifier = ParameterModifier(function.Name, parameter.Name);
        var typeHandle = modifier.Length > 0 ? parameter.Type.Handle.PointeeType : parameter.Type.Handle;
        var niceType = types.MapNativeType(typeHandle, isParam: modifier.Length == 0);
        if (niceType is null)
        {
            state.SkippedFunctions.Add($"{function.Name} (param {parameter.Name}: {parameter.Type.AsString})");
            return null;
        }

        var isString = niceType == "string";
        var hasStringConvenience = isString && !config.StringSkip.ContainsKey(function.Name);
        var isBool = niceType == "bool" || (modifier.Length == 0 && config.BoolParams.GetValueOrDefault(function.Name, []).Contains(parameter.Name));
        var managedType = isString ? "nint" : isBool ? "bool" : niceType;
        var interopType = isString ? "nint" : isBool
            ? types.MapInteropType(typeHandle, isParam: modifier.Length == 0, boolAsRaw: true)!
            : types.MapInteropType(typeHandle, isParam: modifier.Length == 0);
        if (interopType is null)
        {
            state.SkippedFunctions.Add($"{function.Name} (interop param {parameter.Name}: {parameter.Type.AsString})");
            return null;
        }

        var nativeName = parameter.Name.Length > 0 ? parameter.Name : $"arg{index}";
        var canonical = parameter.Type.Handle.CanonicalType;
        var callbackType = CallbackType(parameter);

        return new(
            CSharpName.Parameter(nativeName),
            managedType,
            interopType,
            modifier,
            HasStringConvenience: hasStringConvenience,
            IsUntypedPointer: IsUntypedPointer(modifier, isString, managedType, canonical),
            IsConstPointee: IsConstPointee(canonical),
            IsSizeT: modifier.Length == 0 && CHeaderNameMapper.CleanTypeSpelling(parameter.Type.Handle) == "size_t",
            CallbackType: callbackType);
    }

    /// <summary>Returns the configured C# modifier for a native pointer parameter.</summary>
    private string ParameterModifier(string functionName, string parameterName)
    {
        if (config.OutParams.GetValueOrDefault(functionName, []).Contains(parameterName))
            return "out";
        if (config.InParams.GetValueOrDefault(functionName, []).Contains(parameterName))
            return "in";
        return "";
    }

    /// <summary>Returns a managed callback type and marks the typedef as used when present.</summary>
    private string? CallbackType(ParmVarDecl parameter)
    {
        var callbackTypedef = CHeaderNameMapper.CleanTypeSpelling(parameter.Type.Handle);
        var callbackType = state.DelegatesByNativeName.GetValueOrDefault(callbackTypedef)?.ManagedName;
        if (callbackType is not null)
            state.UsedCallbackTypedefs.Add(callbackTypedef);
        return callbackType;
    }

    /// <summary>Returns true when a parameter can be represented by span extension overloads.</summary>
    private static bool IsUntypedPointer(string modifier, bool isString, string managedType, CXType canonical) =>
        modifier.Length == 0
        && !isString
        && managedType == "nint"
        && canonical.kind == CXTypeKind.CXType_Pointer
        && canonical.PointeeType.CanonicalType.kind
            is CXTypeKind.CXType_Void
            or CXTypeKind.CXType_Char_S or CXTypeKind.CXType_Char_U
            or CXTypeKind.CXType_SChar or CXTypeKind.CXType_UChar;

    /// <summary>Returns true when a pointer parameter points to const data.</summary>
    private static bool IsConstPointee(CXType canonical) =>
        canonical.kind == CXTypeKind.CXType_Pointer && canonical.PointeeType.IsConstQualified;
}

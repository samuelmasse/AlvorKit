namespace AlvorKit.Script.Bindgen;

/// <summary>Discovers function-pointer typedefs used by generated callback APIs.</summary>
internal static class CHeaderCallbackDiscovery
{
    /// <summary>Adds all supported callback typedefs from the selected declarations.</summary>
    public static void Discover(
        BindgenConfig config,
        CHeaderParseState state,
        CHeaderNameMapper names,
        CHeaderTypeMapper types,
        List<Decl> declarations)
    {
        foreach (var typedef in declarations.OfType<TypedefDecl>())
            if (TryReadCallback(config, names, types, typedef) is { } callback)
                state.DelegatesByNativeName[typedef.Name] = callback;
    }

    /// <summary>Returns a delegate model for a function-pointer typedef when all types are supported.</summary>
    private static BindingDelegate? TryReadCallback(
        BindgenConfig config,
        CHeaderNameMapper names,
        CHeaderTypeMapper types,
        TypedefDecl typedef)
    {
        var canonical = typedef.UnderlyingType.Handle.CanonicalType;
        if (canonical.kind != CXTypeKind.CXType_Pointer || canonical.PointeeType.kind != CXTypeKind.CXType_FunctionProto)
            return null;
        var proto = canonical.PointeeType;
        if (types.MapNativeType(proto.ResultType, isReturn: true) is not { } returnType)
            return null;

        var parameterNames = ParameterNames(typedef);
        var parameters = Enumerable.Range(0, proto.NumArgTypes)
            .Select(index => MapCallbackParameter(config, types, proto.GetArgType((uint)index), parameterNames, index))
            .ToList();
        return parameters.Any(parameter => parameter is null)
            ? null
            : new(names.DelegateName(typedef.Name), returnType, [.. parameters.OfType<BindingParameter>()]);
    }

    /// <summary>Maps one callback parameter while keeping callback strings raw.</summary>
    private static BindingParameter? MapCallbackParameter(
        BindgenConfig config,
        CHeaderTypeMapper types,
        CXType argType,
        List<string> names,
        int index)
    {
        if (types.MapNativeType(argType) is not { } managed)
            return null;
        var nativeName = index < names.Count && names[index].Length > 0 ? names[index] : $"arg{index}";
        var typed = config.EnumOverloads?.ByParamName.GetValueOrDefault(nativeName) ?? managed;
        return new(CSharpName.Parameter(nativeName), typed, typed, "", HasStringConvenience: false);
    }

    /// <summary>Reads declared callback parameter names from Clang cursor children.</summary>
    private static List<string> ParameterNames(TypedefDecl typedef) =>
        [.. typedef.CursorChildren.OfType<ParmVarDecl>().Select(parameter => parameter.Name)];
}

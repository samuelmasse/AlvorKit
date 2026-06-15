using ClangSharp;
using ClangSharp.Interop;

namespace AlvorKit.Script.Bindgen;

/// <summary>Discovers exported native functions selected for the generated managed API.</summary>
internal sealed class CHeaderFunctionDiscovery(
    BindgenConfig config,
    CHeaderParseState state,
    CHeaderTypeMapper types)
{
    /// <summary>Adds supported functions from the selected declarations.</summary>
    public void Discover(List<Decl> declarations)
    {
        string[] prefixes = [config.Prefix, .. config.ExtraPrefixes];
        var parameters = new CHeaderParameterBinder(config, state, types);
        foreach (var function in declarations.OfType<FunctionDecl>())
        {
            var matchingPrefix = prefixes.FirstOrDefault(function.Name.StartsWith);
            if (matchingPrefix is null || ShouldSkip(function))
                continue;

            var boundFunction = TryBindFunction(function, matchingPrefix, parameters);
            if (boundFunction is not null)
            {
                state.Functions.Add(boundFunction);
                TrackSizeofCandidate(function);
            }
        }
    }

    /// <summary>Returns true when configuration or C language shape excludes a function.</summary>
    private bool ShouldSkip(FunctionDecl function)
    {
        if (config.Skip.TryGetValue(function.Name, out var skipReason))
        {
            state.SkippedFunctions.Add($"{function.Name} ({skipReason})");
            return true;
        }
        if (function.IsVariadic)
        {
            state.SkippedFunctions.Add($"{function.Name} (variadic)");
            return true;
        }
        return function.StorageClass == CX_StorageClass.CX_SC_Static;
    }

    /// <summary>Binds one native function when its return and parameters are supported.</summary>
    private BindingFunction? TryBindFunction(FunctionDecl function, string matchingPrefix, CHeaderParameterBinder parameters)
    {
        var returnType = types.MapNativeType(function.ReturnType.Handle, isReturn: true);
        if (returnType is null)
        {
            state.SkippedFunctions.Add($"{function.Name} (return type: {function.ReturnType.AsString})");
            return null;
        }

        var boundParameters = function.Parameters
            .Select((parameter, index) => parameters.TryBind(function, parameter, index))
            .ToList();
        if (boundParameters.Any(parameter => parameter is null))
            return null;

        var isBoolReturn = returnType == "bool" || config.BoolReturns.Contains(function.Name);
        var returnInteropType = isBoolReturn
            ? types.MapNativeType(function.ReturnType.Handle, isReturn: true, boolAsRaw: true)!
            : returnType;
        var managedName = config.FunctionRenames.GetValueOrDefault(function.Name)
            ?? CSharpName.FromNativeIdentifier(function.Name, matchingPrefix, config.DigitNamePrefix);
        return new(
            function.Name,
            managedName,
            isBoolReturn ? "bool" : returnType,
            returnInteropType,
            [.. boundParameters.OfType<BindingParameter>()],
            XmlDocComment.Parse(function.Handle.RawCommentText.ToString()),
            ReturnsCString: returnType == "nint" && ReturnsCString(function.ReturnType.Handle));
    }

    /// <summary>Returns true when the raw pointer return can drive C-string convenience overloads.</summary>
    private static bool ReturnsCString(CXType returnType)
    {
        var canonical = returnType.CanonicalType;
        if (canonical.kind != CXTypeKind.CXType_Pointer)
            return false;
        var pointee = canonical.PointeeType;
        return pointee.kind is CXTypeKind.CXType_Char_S or CXTypeKind.CXType_Char_U && pointee.IsConstQualified;
    }

    /// <summary>Tracks struct init functions whose allocation size must be queried at runtime.</summary>
    private void TrackSizeofCandidate(FunctionDecl function)
    {
        var lastParameter = function.Parameters.LastOrDefault();
        if (lastParameter is null)
            return;
        var canonical = lastParameter.Type.Handle.CanonicalType;
        if (canonical.kind != CXTypeKind.CXType_Pointer || canonical.PointeeType.CanonicalType.kind != CXTypeKind.CXType_Record)
            return;

        var nativeTypeName = CHeaderNameMapper.CleanTypeSpelling(lastParameter.Type.Handle.PointeeType);
        if (state.RecordByNativeName.ContainsKey(nativeTypeName) && function.Name.StartsWith(nativeTypeName + "_init"))
            state.SizeofTypes.Add(nativeTypeName);
    }
}

namespace AlvorKit.Script.Bindgen;

/// <summary>Builds generated managed delegates for callback typedefs used by selected commands.</summary>
/// <param name="config">Bindgen configuration containing callback metadata.</param>
/// <param name="catchAllName">Generated catch-all enum name for untyped callback GLenum positions.</param>
internal sealed class GlCallbackDelegateBuilder(BindgenConfig config, string catchAllName)
{
    /// <summary>Builds delegates only for callback typedefs that selected commands reference.</summary>
    public List<GlDelegate> Build(
        IReadOnlyDictionary<string, GlCallbackSignature> signatures,
        IReadOnlySet<string> usedCallbacks,
        IReadOnlyDictionary<string, string> managedNameByGroup)
    {
        var delegates = new List<GlDelegate>();
        foreach (var nativeName in usedCallbacks)
        {
            var callback = config.Callbacks[nativeName];
            var signature = signatures[nativeName];
            delegates.Add(new(
                nativeName,
                callback.ManagedName,
                MapReturn(signature.ReturnType, nativeName),
                [.. signature.Parameters.Select(parameter => MapParameter(parameter.CType, parameter.Name, callback, managedNameByGroup))]));
        }
        return [.. delegates.OrderBy(item => item.ManagedName, StringComparer.Ordinal)];
    }

    /// <summary>Maps a callback return type into the managed delegate return type.</summary>
    private static string MapReturn(string cType, string nativeName)
    {
        if (cType.Contains('*'))
            return "nint";
        if (!GlRegistryValueTypes.Map.TryGetValue(cType.Replace("const", "").Trim(), out var valueType))
            throw new InvalidOperationException($"{nativeName}: unmapped callback return type '{cType}'.");
        return valueType;
    }

    /// <summary>Maps one callback parameter using config-supplied enum group hints.</summary>
    private GlParameter MapParameter(
        string cType,
        string name,
        CallbackConfig callback,
        IReadOnlyDictionary<string, string> managedNameByGroup)
    {
        var pointerDepth = cType.Count(character => character == '*');
        var baseType = cType.Replace("const", "").Replace("struct", "").Replace("*", "").Trim();
        if (!GlRegistryValueTypes.Map.TryGetValue(baseType, out var valueType))
            throw new InvalidOperationException($"Unmapped callback parameter type '{cType.Trim()}' on {name}.");
        var managedName = CSharpName.Parameter(name);

        if (pointerDepth > 0)
            return new(name, managedName, "nint", "nint", null, pointerDepth, null, cType.TrimStart().StartsWith("const "), baseType == "GLchar");
        if (baseType is "GLenum" or "GLbitfield")
            return MapEnumParameter(name, managedName, callback, managedNameByGroup);
        return baseType == "GLboolean"
            ? new(name, managedName, "byte", "byte", null, 0, null, false, false)
            : new(name, managedName, valueType, valueType, null, 0, null, false, false);
    }

    /// <summary>Maps one callback enum parameter using configured group hints or the catch-all enum.</summary>
    private GlParameter MapEnumParameter(
        string name,
        string managedName,
        CallbackConfig callback,
        IReadOnlyDictionary<string, string> managedNameByGroup)
    {
        var managedType = callback.ParamGroups.TryGetValue(name, out var groupNative)
            && managedNameByGroup.TryGetValue(groupNative, out var managedGroup)
                ? managedGroup
                : catchAllName;
        return new(name, managedName, managedType, "uint", null, 0, null, false, false);
    }
}

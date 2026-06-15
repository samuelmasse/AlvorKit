namespace AlvorKit.Script.Bindgen;

/// <summary>Plans managed parameter shapes shared by generated convenience overload emitters.</summary>
internal static class BindingOverloadTypes
{
    /// <summary>Returns all non-native managed type combinations configured for a function.</summary>
    public static IEnumerable<List<(string Type, BindingParameter Parameter)>> Variants(BindingFunction function, EnumOverloads? overloads)
    {
        var perFunction = overloads?.Functions.GetValueOrDefault(function.NativeName);
        var candidateTypes = function.Parameters.Select(parameter => CandidateTypes(parameter, perFunction, overloads)).ToList();
        var emitted = new HashSet<string>();

        foreach (var combination in CartesianProduct(candidateTypes))
        {
            var typed = combination.Zip(function.Parameters).ToList();
            if (typed.All(pair => pair.First == pair.Second.ManagedType))
                continue;

            var signature = string.Join(", ", typed.Select(SignatureParameter));
            if (emitted.Add(signature))
                yield return typed;
        }
    }

    /// <summary>Formats one overload signature parameter.</summary>
    public static string SignatureParameter((string Type, BindingParameter Parameter) pair) =>
        $"{(pair.Parameter.Modifier.Length > 0 ? pair.Parameter.Modifier + " " : "")}{pair.Type} {pair.Parameter.ManagedName}";

    /// <summary>Formats one overload call argument.</summary>
    public static string Argument((string Type, BindingParameter Parameter) pair) =>
        pair.Parameter.Modifier.Length > 0 ? pair.Parameter.Modifier + " " + pair.Parameter.ManagedName
        : pair.Type == pair.Parameter.ManagedType ? pair.Parameter.ManagedName
        : pair.Type == "string" ? $"{pair.Parameter.ManagedName.TrimStart('@')}Utf8.Pointer"
        : pair.Type == "bool" ? $"({pair.Parameter.ManagedName} ? 1 : 0)"
        : pair.Parameter.ManagedType == "CULong" ? $"new CULong((uint){pair.Parameter.ManagedName})"
        : pair.Parameter.ManagedType == "CLong" ? $"new CLong((int){pair.Parameter.ManagedName})"
        : $"({pair.Parameter.ManagedType}){pair.Parameter.ManagedName}";

    /// <summary>Builds UTF-8 locals for managed string parameters in a convenience overload.</summary>
    public static string StringLocals(IEnumerable<(string Type, BindingParameter Parameter)> typed) =>
        string.Join(
            "",
            typed.Where(pair => pair.Type == "string").Select(StringLocal));

    /// <summary>Returns candidate managed types for one parameter.</summary>
    private static string[] CandidateTypes(BindingParameter parameter, FunctionEnums? perFunction, EnumOverloads? overloads)
    {
        var name = parameter.ManagedName.TrimStart('@');
        if (perFunction?.Params.TryGetValue(name, out var listed) == true && listed.Length > 0)
            return listed;
        if (parameter.Modifier.Length == 0 && overloads?.ByParamName.TryGetValue(name, out var byName) == true)
            return [byName];
        return parameter.HasStringConvenience ? [parameter.ManagedType, "string"] : [parameter.ManagedType];
    }

    /// <summary>Returns the cartesian product of candidate overload parameter types.</summary>
    private static IEnumerable<string[]> CartesianProduct(List<string[]> lists)
    {
        IEnumerable<string[]> result = [[]];
        foreach (var list in lists)
            result = result.SelectMany(prefix => list.Select(item => (string[])[.. prefix, item]));
        return result;
    }

    /// <summary>Formats one UTF-8 helper local for a managed string argument.</summary>
    private static string StringLocal((string Type, BindingParameter Parameter) pair)
    {
        var name = pair.Parameter.ManagedName;
        return $"        using var {name.TrimStart('@')}Utf8 = new Utf8({name}, stackalloc byte[256]);" + Environment.NewLine;
    }
}

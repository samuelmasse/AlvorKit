namespace AlvorKit.Script.Bindgen;

/// <summary>Emits typed convenience overloads for enums, booleans, and strings.</summary>
internal sealed class BindingTypedOverloadEmitter(BindingEmitterContext context)
{
    /// <summary>Emits all typed overloads for one generated function.</summary>
    public void TypedOverloads(StringBuilder output, BindingFunction function)
    {
        var overloads = context.Config.EnumOverloads;
        var perFunction = overloads?.Functions.GetValueOrDefault(function.NativeName);
        var candidateTypes = function.Parameters.Select(parameter => CandidateTypes(parameter, perFunction, overloads)).ToList();
        var inheritDoc = $"    /// <inheritdoc cref=\"{function.ManagedName}({BindingSignature.Cref(function.Parameters)})\"/>";
        var emitted = new HashSet<string>();

        foreach (var combination in CartesianProduct(candidateTypes))
        {
            var typed = combination.Zip(function.Parameters).ToList();
            if (typed.All(pair => pair.First == pair.Second.ManagedType))
                continue;

            var signature = string.Join(", ", typed.Select(SignatureParameter));
            if (!emitted.Add(signature))
                continue;
            EmitOverload(output, function, typed, inheritDoc, perFunction?.Return, signature);
        }
    }

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

    /// <summary>Emits one overload body for a specific type combination.</summary>
    private static void EmitOverload(
        StringBuilder output,
        BindingFunction function,
        List<(string First, BindingParameter Second)> typed,
        string inheritDoc,
        string? returnEnum,
        string signature)
    {
        var strings = typed.Where(pair => pair.First == "string").Select(pair => pair.Second.ManagedName).ToList();
        var arguments = string.Join(", ", typed.Select(Argument));
        var returnType = returnEnum ?? function.ReturnType;
        var invoke = returnEnum is not null ? $"({returnEnum}){function.ManagedName}({arguments})" : $"{function.ManagedName}({arguments})";

        output.AppendLine(inheritDoc);
        output.AppendLine(strings.Count > 0
            ? "    /// <remarks>Convenience overload. Marshals string arguments to UTF-8 on the stack when possible.</remarks>"
            : "    /// <remarks>Convenience overload. Casts typed arguments and forwards to the underlying method.</remarks>");
        if (strings.Count == 0)
        {
            output.AppendLine($"    public {returnType} {function.ManagedName}({signature}) => {invoke};");
            return;
        }
        output.AppendLine($"    public {returnType} {function.ManagedName}({signature})");
        output.AppendLine("    {");
        foreach (var name in strings)
            output.AppendLine($"        using var {name.TrimStart('@')}Utf8 = new Utf8({name}, stackalloc byte[256]);");
        output.AppendLine($"        {(returnType == "void" ? invoke : "return " + invoke)};");
        output.AppendLine("    }");
    }

    /// <summary>Formats one overload signature parameter.</summary>
    private static string SignatureParameter((string First, BindingParameter Second) pair) =>
        $"{(pair.Second.Modifier.Length > 0 ? pair.Second.Modifier + " " : "")}{pair.First} {pair.Second.ManagedName}";

    /// <summary>Formats one overload call argument.</summary>
    private static string Argument((string First, BindingParameter Second) pair) =>
        pair.Second.Modifier.Length > 0 ? pair.Second.Modifier + " " + pair.Second.ManagedName
        : pair.First == pair.Second.ManagedType ? pair.Second.ManagedName
        : pair.First == "string" ? $"{pair.Second.ManagedName.TrimStart('@')}Utf8.Pointer"
        : pair.First == "bool" ? $"({pair.Second.ManagedName} ? 1 : 0)"
        : $"(int){pair.Second.ManagedName}";

    /// <summary>Returns the cartesian product of candidate overload parameter types.</summary>
    private static IEnumerable<string[]> CartesianProduct(List<string[]> lists)
    {
        IEnumerable<string[]> result = [[]];
        foreach (var list in lists)
            result = result.SelectMany(prefix => list.Select(item => (string[])[.. prefix, item]));
        return result;
    }
}

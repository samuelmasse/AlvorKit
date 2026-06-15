namespace AlvorKit.Script.Bindgen;

/// <summary>Emits span overloads for pointer-plus-length parameters.</summary>
internal sealed class BindingSpanOverloadEmitter(BindingEmitterContext context)
{
    /// <summary>Emits span overload methods and returns true when any were produced.</summary>
    public bool SpanOverloads(StringBuilder overloads, BindingModel model)
    {
        var startLength = overloads.Length;
        foreach (var function in model.Functions)
        {
            var candidates = SpanCandidates(function);
            if (candidates.Count == 0)
                continue;
            var full = (1 << candidates.Count) - 1;
            EmitSpanOverload(overloads, function, candidates, full);
            for (var mask = 1; mask < full; mask++)
                EmitSpanOverload(overloads, function, candidates, mask);
        }
        return overloads.Length > startLength;
    }

    /// <summary>Emits the byte-length helper used by generated span overloads.</summary>
    public static void ByteLengthHelper(StringBuilder output)
    {
        output.AppendLine();
        output.AppendLine("    /// <summary>Returns the byte length of an unmanaged span.</summary>");
        output.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        output.AppendLine("    private static nuint ByteLength<T>(ReadOnlySpan<T> span) where T : unmanaged =>");
        output.AppendLine("        checked((nuint)span.Length * (nuint)sizeof(T));");
    }

    /// <summary>Finds pointer parameters that can safely become spans in overloads.</summary>
    private List<(int Pointer, int? Length)> SpanCandidates(BindingFunction function)
    {
        var candidates = new List<(int Pointer, int? Length)>();
        if (context.Config.SpanSkip.ContainsKey(function.NativeName))
            return candidates;

        var lengthlessParams = context.Config.SpanParams.GetValueOrDefault(function.NativeName, []);
        for (var i = 0; i < function.Parameters.Count; i++)
        {
            if (!function.Parameters[i].IsUntypedPointer)
                continue;
            if (i + 1 < function.Parameters.Count && function.Parameters[i + 1].IsSizeT)
                candidates.Add((i, i + 1));
            else if (lengthlessParams.Contains(function.Parameters[i].ManagedName.TrimStart('@')))
                candidates.Add((i, null));
        }
        return candidates;
    }

    /// <summary>Emits one span overload for a particular pointer-parameter mask.</summary>
    private void EmitSpanOverload(StringBuilder output, BindingFunction function, List<(int Pointer, int? Length)> candidates, int mask)
    {
        var spanned = candidates.Where((_, index) => (mask & (1 << index)) != 0).ToList();
        var typeParameterByPointer = spanned.ToDictionary(
            candidate => candidate.Pointer,
            candidate => "T" + BindingSignature.TypeParameterName(function.Parameters[candidate.Pointer].ManagedName));
        var pointerByLength = spanned.Where(candidate => candidate.Length is not null)
            .ToDictionary(candidate => candidate.Length!.Value, candidate => candidate.Pointer);
        var signature = Signature(function, typeParameterByPointer, pointerByLength, out var arguments);
        var typeParameters = spanned.Select(candidate => typeParameterByPointer[candidate.Pointer]).ToList();

        output.AppendLine();
        BindingDocs.InheritedConvenience(
            output,
            $"{context.Config.ApiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})",
            "Pins span arguments for the duration of the call, supplies byte lengths where the native method expects them, "
            + "and forwards to the underlying method.");
        output.Append($"    public {function.ReturnType} {function.ManagedName}<{string.Join(", ", typeParameters)}>({string.Join(", ", signature)})");
        EmitTypeConstraints(output, typeParameters);
        output.AppendLine("    {");
        foreach (var candidate in spanned)
        {
            var parameter = function.Parameters[candidate.Pointer];
            output.AppendLine($"        fixed ({typeParameterByPointer[candidate.Pointer]}* {parameter.ManagedName}Ptr = {parameter.ManagedName})");
        }
        var call = $"{function.ManagedName}({string.Join(", ", arguments)})";
        output.AppendLine($"            {(function.ReturnType == "void" ? call : "return " + call)};");
        output.AppendLine("    }");
    }

    /// <summary>Builds an overload signature and call argument list.</summary>
    private List<string> Signature(
        BindingFunction function,
        Dictionary<int, string> typeParameterByPointer,
        Dictionary<int, int> pointerByLength,
        out List<string> arguments)
    {
        var signature = new List<string>();
        arguments = [];
        foreach (var (parameter, index) in function.Parameters.Select((parameter, index) => (parameter, index)))
            AddParameter(function.Parameters, parameter, index, typeParameterByPointer, pointerByLength, signature, arguments);
        return signature;
    }

    /// <summary>Adds one overload parameter and its forwarded argument.</summary>
    private static void AddParameter(
        List<BindingParameter> parameters,
        BindingParameter parameter,
        int index,
        Dictionary<int, string> typeParameterByPointer,
        Dictionary<int, int> pointerByLength,
        List<string> signature,
        List<string> arguments)
    {
        if (typeParameterByPointer.TryGetValue(index, out var typeParameter))
        {
            var spanType = parameter.IsConstPointee ? "ReadOnlySpan" : "Span";
            signature.Add($"{spanType}<{typeParameter}> {parameter.ManagedName}");
            arguments.Add($"(nint){parameter.ManagedName}Ptr");
        }
        else if (pointerByLength.TryGetValue(index, out var pointerIndex))
        {
            arguments.Add($"ByteLength<{typeParameterByPointer[pointerIndex]}>({parameters[pointerIndex].ManagedName})");
        }
        else
        {
            var modifier = parameter.Modifier.Length > 0 ? parameter.Modifier + " " : "";
            signature.Add($"{modifier}{parameter.ManagedType} {parameter.ManagedName}");
            arguments.Add($"{modifier}{parameter.ManagedName}");
        }
    }

    /// <summary>Emits unmanaged constraints for span overload type parameters.</summary>
    private static void EmitTypeConstraints(StringBuilder output, List<string> typeParameters)
    {
        if (typeParameters.Count == 1)
            output.AppendLine($" where {typeParameters[0]} : unmanaged");
        else
        {
            output.AppendLine();
            foreach (var typeParameter in typeParameters)
                output.AppendLine($"        where {typeParameter} : unmanaged");
        }
    }
}

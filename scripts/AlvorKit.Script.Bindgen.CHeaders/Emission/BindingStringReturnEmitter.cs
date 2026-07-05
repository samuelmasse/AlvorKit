namespace AlvorKit.Script.Bindgen;

/// <summary>Emits string-return convenience overloads for const C-string functions.</summary>
internal sealed class BindingStringReturnEmitter(BindingEmitterContext context)
{
    /// <summary>Emits managed string-return overloads when a function returns a const C string.</summary>
    public void StringReturn(StringBuilder output, BindingFunction function)
    {
        if (!function.ReturnsCString)
            return;

        EmitStringReturn(output, function, [.. function.Parameters.Select(parameter => (parameter.ManagedType, parameter))]);
        foreach (var typed in BindingOverloadTypes.Variants(function, context.Config.EnumOverloads))
            EmitStringReturn(output, function, typed);
    }

    /// <summary>Emits out-string and span-string overloads for one parameter shape.</summary>
    private void EmitStringReturn(
        StringBuilder output,
        BindingFunction function,
        List<(string Type, BindingParameter Parameter)> typed)
    {
        var taken = function.Parameters.Select(parameter => parameter.ManagedName.TrimStart('@')).ToHashSet();
        var value = Unique(taken, "value");
        var destination = Unique(taken, "destination");
        var result = Unique(taken, "result");
        var leading = typed.Select(BindingOverloadTypes.SignatureParameter).ToList();
        var callArguments = string.Join(", ", typed.Select(BindingOverloadTypes.Argument));
        var inheritCref = $"{context.Config.ApiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})";
        var stringLocals = BindingOverloadTypes.StringLocals(typed);
        var call = $"{function.ManagedName}({callArguments})";

        var stringDocumentation = new StringBuilder();
        BindingDocs.InheritedConvenience(
            stringDocumentation,
            inheritCref,
            function.NativeName,
            "Decodes the returned C string to a managed string, or null when the pointer is null.");
        stringDocumentation.Append(BindingMethodAttributes.PlatformOnly(function));
        var stringParameters = string.Join(", ", leading.Append($"out string? {value}"));
        output.Append(TemplateResource.Render(
            typeof(BindingStringReturnEmitter),
            "res/templates/bindgen/c-headers/csharp/string-return.csfrag.tmpl",
            ("Documentation", stringDocumentation.ToString()),
            ("ManagedName", function.ManagedName),
            ("StringParameters", stringParameters),
            ("Value", value),
            ("StringLocals", stringLocals),
            ("Call", call)));

        var spanDocumentation = new StringBuilder();
        BindingDocs.InheritedConvenience(
            spanDocumentation,
            inheritCref,
            function.NativeName,
            $"Decodes the returned C string into <paramref name=\"{destination}\"/> and returns the slice written.");
        spanDocumentation.Append(BindingMethodAttributes.PlatformOnly(function));
        var spanParameters = string.Join(
            ", ",
            leading.Append($"Span<char> {destination}").Append($"out ReadOnlySpan<char> {result}"));
        output.Append(TemplateResource.Render(
            typeof(BindingStringReturnEmitter),
            "res/templates/bindgen/c-headers/csharp/string-return-span.csfrag.tmpl",
            ("Documentation", spanDocumentation.ToString()),
            ("ManagedName", function.ManagedName),
            ("SpanParameters", spanParameters),
            ("StringLocals", stringLocals),
            ("Call", call),
            ("Result", result),
            ("Destination", destination)));
    }

    /// <summary>Returns a unique generated parameter name.</summary>
    private static string Unique(HashSet<string> taken, string preferred)
    {
        while (taken.Contains(preferred))
            preferred += "_";
        taken.Add(preferred);
        return preferred;
    }
}

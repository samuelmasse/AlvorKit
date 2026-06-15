namespace AlvorKit.Script.Bindgen;

/// <summary>Emits string-return convenience overloads for const C-string functions.</summary>
internal static class BindingStringReturnEmitter
{
    /// <summary>Emits managed string-return overloads when a function returns a const C string.</summary>
    public static void StringReturn(StringBuilder output, BindingFunction function, string apiClass)
    {
        if (!function.ReturnsCString)
            return;
        var taken = function.Parameters.Select(parameter => parameter.ManagedName.TrimStart('@')).ToHashSet();
        var value = Unique(taken, "value");
        var destination = Unique(taken, "destination");
        var result = Unique(taken, "result");
        var leading = function.Parameters.Select(BindingSignature.Parameter).ToList();
        var callArguments = string.Join(", ", function.Parameters.Select(BindingSignature.Argument));
        var inheritCref = $"{apiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})";
        var call = $"{function.ManagedName}({callArguments})";

        var stringDocumentation = new StringBuilder();
        BindingDocs.InheritedConvenience(
            stringDocumentation,
            inheritCref,
            "Decodes the returned C string to a managed string, or null when the pointer is null.");
        var stringParameters = string.Join(", ", leading.Append($"out string? {value}"));
        output.Append(TemplateResource.Render(
            typeof(BindingStringReturnEmitter),
            "res/templates/bindgen/c-headers/csharp/string-return.csfrag.tmpl",
            ("Documentation", stringDocumentation.ToString()),
            ("ManagedName", function.ManagedName),
            ("StringParameters", stringParameters),
            ("Value", value),
            ("Call", call)));

        var spanDocumentation = new StringBuilder();
        BindingDocs.InheritedConvenience(
            spanDocumentation,
            inheritCref,
            $"Decodes the returned C string into <paramref name=\"{destination}\"/> and returns the slice written.");
        var spanParameters = string.Join(
            ", ",
            leading.Append($"Span<char> {destination}").Append($"out ReadOnlySpan<char> {result}"));
        output.Append(TemplateResource.Render(
            typeof(BindingStringReturnEmitter),
            "res/templates/bindgen/c-headers/csharp/string-return-span.csfrag.tmpl",
            ("Documentation", spanDocumentation.ToString()),
            ("ManagedName", function.ManagedName),
            ("SpanParameters", spanParameters),
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

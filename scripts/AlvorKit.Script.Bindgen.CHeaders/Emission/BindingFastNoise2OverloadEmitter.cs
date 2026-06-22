namespace AlvorKit.Script.Bindgen;

/// <summary>Emits FastNoise2-specific span overloads for bulk noise generation APIs.</summary>
internal static class BindingFastNoise2OverloadEmitter
{
    private static readonly FastNoise2Spec[] Specs =
    [
        new("fnGenUniformGrid2D", []),
        new("fnGenUniformGrid3D", []),
        new("fnGenUniformGrid4D", []),
        new("fnGenTileable2D", []),
        new("fnGenPositionArray2D", ["xPosArray", "yPosArray"]),
        new("fnGenPositionArray3D", ["xPosArray", "yPosArray", "zPosArray"]),
        new("fnGenPositionArray4D", ["xPosArray", "yPosArray", "zPosArray", "wPosArray"])
    ];

    /// <summary>Emits FastNoise2 convenience overloads for APIs that write into caller-owned buffers.</summary>
    public static void FastNoise2Overloads(StringBuilder output, BindingModel model, string apiClass)
    {
        foreach (var spec in Specs)
        {
            if (model.Functions.FirstOrDefault(function => function.NativeName == spec.NativeName) is not { } function)
                continue;

            SpanOverload(output, function, spec, apiClass, includeMinMax: false);
            SpanOverload(output, function, spec, apiClass, includeMinMax: true);
        }
    }

    /// <summary>Emits one span overload variant for a FastNoise2 generation function.</summary>
    private static void SpanOverload(
        StringBuilder output,
        BindingFunction function,
        FastNoise2Spec spec,
        string apiClass,
        bool includeMinMax)
    {
        var summary = includeMinMax
            ? "Pins caller-owned noise output and min/max spans for the duration of the native call."
            : "Pins a caller-owned noise output span for the duration of the native call.";
        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})",
            function.NativeName,
            summary);

        output.Append(TemplateResource.RenderFragment(
            typeof(BindingFastNoise2OverloadEmitter),
            "res/templates/bindgen/c-headers/csharp/fastnoise2-span-overload.csfrag.tmpl",
            ("ManagedName", function.ManagedName),
            ("Signature", Signature(function, spec, includeMinMax)),
            ("FixedStatements", FixedStatements(spec, includeMinMax)),
            ("Arguments", Arguments(function, spec, includeMinMax))));
    }

    /// <summary>Builds the managed signature with native pointers replaced by spans.</summary>
    private static string Signature(BindingFunction function, FastNoise2Spec spec, bool includeMinMax)
    {
        var parts = new List<string>();
        foreach (var parameter in function.Parameters.Where(parameter => parameter.ManagedName != "outputMinMax"))
            parts.Add(ParameterSignature(parameter, spec));
        if (includeMinMax)
            parts.Add("Span<float> outputMinMax");
        return string.Join(", ", parts);
    }

    /// <summary>Returns one managed parameter signature.</summary>
    private static string ParameterSignature(BindingParameter parameter, FastNoise2Spec spec) =>
        parameter.ManagedName switch
        {
            "noiseOut" => "Span<float> noiseOut",
            var name when spec.PositionArrays.Contains(name) => $"ReadOnlySpan<float> {name}",
            _ => $"{parameter.ManagedType} {parameter.ManagedName}"
        };

    /// <summary>Builds the fixed statements for pinned spans.</summary>
    private static string FixedStatements(FastNoise2Spec spec, bool includeMinMax)
    {
        var output = new StringBuilder();
        output.AppendLine("        fixed (float* noiseOutPtr = noiseOut)");
        foreach (var name in spec.PositionArrays)
            output.AppendLine($"        fixed (float* {name}Ptr = {name})");
        if (includeMinMax)
            output.AppendLine("        fixed (float* outputMinMaxPtr = outputMinMax)");
        return output.ToString();
    }

    /// <summary>Builds the raw generated method argument list.</summary>
    private static string Arguments(BindingFunction function, FastNoise2Spec spec, bool includeMinMax) =>
        string.Join(", ", function.Parameters.Select(parameter => Argument(parameter.ManagedName, spec, includeMinMax)));

    /// <summary>Builds one forwarded raw argument.</summary>
    private static string Argument(string name, FastNoise2Spec spec, bool includeMinMax) =>
        name switch
        {
            "noiseOut" => "(nint)noiseOutPtr",
            "outputMinMax" => includeMinMax ? "(nint)outputMinMaxPtr" : "0",
            var pos when spec.PositionArrays.Contains(pos) => $"(nint){pos}Ptr",
            _ => name
        };

    /// <summary>FastNoise2 buffer overload metadata.</summary>
    private sealed record FastNoise2Spec(string NativeName, string[] PositionArrays);
}

namespace AlvorKit.Script.Bindgen;

/// <summary>Emits span overloads for configured typed pointer plus element-count parameter pairs.</summary>
internal sealed class BindingCountedSpanOverloadEmitter(BindingEmitterContext context)
{
    /// <summary>Emits configured counted span overloads and returns true when any were produced.</summary>
    public bool CountedSpanOverloads(StringBuilder output, BindingModel model)
    {
        var startLength = output.Length;
        foreach (var function in model.Functions)
            EmitFunction(output, function);
        return output.Length > startLength;
    }

    /// <summary>Emits counted span overloads for one function when configured.</summary>
    private void EmitFunction(StringBuilder output, BindingFunction function)
    {
        if (!context.Config.CountedSpanParams.TryGetValue(function.NativeName, out var configured))
            return;

        var spans = Spans(function, configured);
        if (spans.Count == 0)
            return;

        var signature = Signature(function, spans, out var arguments);
        var fixedStatements = string.Join("", spans.Values.Select(span =>
            $"        fixed ({span.ElementType}* {span.LocalName}Ptr = {span.Parameter.ManagedName}){Environment.NewLine}"));
        var documentation = new StringBuilder();
        BindingDocs.InheritedConvenience(
            documentation,
            $"{context.Config.ApiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})",
            function.NativeName,
            "Pins span arguments for the duration of the native call and supplies element counts.");
        documentation.Append(BindingMethodAttributes.PlatformOnly(function));
        var call = $"{function.ManagedName}({string.Join(", ", arguments)})";
        output.Append(TemplateResource.Render(
            typeof(BindingCountedSpanOverloadEmitter),
            "res/templates/bindgen/c-headers/csharp/counted-span-overload.csfrag.tmpl",
            ("Documentation", documentation.ToString()),
            ("ReturnType", function.ReturnType),
            ("ManagedName", function.ManagedName),
            ("Signature", string.Join(", ", signature)),
            ("FixedStatements", fixedStatements),
            ("Invocation", function.ReturnType == "void" ? call : "return " + call)));
    }

    /// <summary>Finds configured pointer/count pairs that can be projected as spans.</summary>
    private static Dictionary<int, CountedSpan> Spans(BindingFunction function, Dictionary<string, string> configured)
    {
        var spans = new Dictionary<int, CountedSpan>();
        foreach (var (pointerName, countName) in configured)
        {
            var pointerIndex = function.Parameters.FindIndex(parameter => parameter.ManagedName.TrimStart('@') == pointerName);
            var countIndex = function.Parameters.FindIndex(parameter => parameter.ManagedName.TrimStart('@') == countName);
            if (pointerIndex < 0 || countIndex < 0 || !function.Parameters[pointerIndex].ManagedType.EndsWith('*'))
                continue;

            var pointer = function.Parameters[pointerIndex];
            var elementType = pointer.ManagedType.TrimEnd('*').TrimEnd();
            spans.Add(pointerIndex, new(pointer, function.Parameters[countIndex], countIndex, elementType));
        }
        return spans;
    }

    /// <summary>Builds an overload signature and forwarded argument list.</summary>
    private static List<string> Signature(BindingFunction function, Dictionary<int, CountedSpan> spans, out List<string> arguments)
    {
        var signature = new List<string>();
        arguments = [];
        var spanByCount = spans.Values.ToDictionary(span => span.CountIndex);
        foreach (var (parameter, index) in function.Parameters.Select((parameter, index) => (parameter, index)))
        {
            if (spans.TryGetValue(index, out var span))
            {
                var spanType = parameter.IsConstPointee ? "ReadOnlySpan" : "Span";
                signature.Add($"{spanType}<{span.ElementType}> {parameter.ManagedName}");
                arguments.Add($"{span.LocalName}Ptr");
            }
            else if (spanByCount.TryGetValue(index, out span))
            {
                arguments.Add(CountArgument(span.CountParameter, span.Parameter.ManagedName.TrimStart('@')));
            }
            else
            {
                signature.Add(BindingSignature.Parameter(parameter));
                arguments.Add(BindingSignature.Argument(parameter));
            }
        }
        return signature;
    }

    /// <summary>Formats the element count argument for the native-shaped method.</summary>
    private static string CountArgument(BindingParameter countParameter, string spanName) =>
        countParameter.ManagedType == "int"
            ? $"{spanName}.Length"
            : $"checked(({countParameter.ManagedType}){spanName}.Length)";

    /// <summary>Configured counted span metadata for one pointer parameter.</summary>
    /// <param name="Parameter">Pointer parameter projected as a span.</param>
    /// <param name="CountParameter">Parameter receiving the span element count.</param>
    /// <param name="CountIndex">Index of the count parameter in the source function.</param>
    /// <param name="ElementType">Managed element type pointed to by <paramref name="Parameter"/>.</param>
    private sealed record CountedSpan(BindingParameter Parameter, BindingParameter CountParameter, int CountIndex, string ElementType)
    {
        /// <summary>Local name used for the pinned pointer variable.</summary>
        public string LocalName { get; } = Parameter.ManagedName.TrimStart('@');
    }
}

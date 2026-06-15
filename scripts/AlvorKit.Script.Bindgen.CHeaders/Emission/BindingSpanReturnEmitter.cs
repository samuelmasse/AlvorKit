namespace AlvorKit.Script.Bindgen;

/// <summary>Emits configured span-return convenience overloads.</summary>
internal sealed class BindingSpanReturnEmitter(BindingEmitterContext context)
{
    /// <summary>Emits span-return overloads for functions configured with trailing count outputs.</summary>
    public void SpanReturns(StringBuilder output, BindingModel model)
    {
        foreach (var function in model.Functions)
        {
            if (!context.Config.SpanReturns.TryGetValue(function.NativeName, out var elementType))
                continue;
            var leading = function.Parameters.Take(function.Parameters.Count - 1).ToList();
            var signature = string.Join(", ", leading.Select(BindingSignature.Parameter));
            var callArguments = string.Join(", ", leading.Select(BindingSignature.Argument).Append("out var count"));
            var nullCheck = function.ReturnType.TrimEnd().EndsWith('*') ? "pointer == null" : "pointer == 0";
            var documentation = new StringBuilder();
            BindingDocs.InheritedConvenience(
                documentation,
                $"{context.Config.ApiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})",
                "Returns a read-only span over native memory while supplying the count internally.");
            output.Append(TemplateResource.Render(
                typeof(BindingSpanReturnEmitter),
                "res/templates/bindgen/c-headers/csharp/span-return.csfrag.tmpl",
                ("Documentation", documentation.ToString()),
                ("ElementType", elementType),
                ("ManagedName", function.ManagedName),
                ("Signature", signature),
                ("CallArguments", callArguments),
                ("NullCheck", nullCheck)));
        }
    }
}

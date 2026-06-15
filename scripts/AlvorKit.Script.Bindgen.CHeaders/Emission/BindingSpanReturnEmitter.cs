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
            output.AppendLine();
            BindingDocs.InheritedConvenience(
                output,
                $"{context.Config.ApiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})",
                "Returns a read-only span over native memory while supplying the count internally.");
            output.AppendLine($"    public unsafe ReadOnlySpan<{elementType}> {function.ManagedName}({signature})");
            output.AppendLine("    {");
            output.AppendLine($"        var pointer = {function.ManagedName}({callArguments});");
            output.AppendLine($"        return pointer == 0 || count <= 0 ? default : new ReadOnlySpan<{elementType}>((void*)pointer, count);");
            output.AppendLine("    }");
        }
    }
}

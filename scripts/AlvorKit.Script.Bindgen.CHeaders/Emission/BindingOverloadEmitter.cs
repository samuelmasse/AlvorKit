namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated partial-class overloads for managed convenience shapes.</summary>
internal sealed class BindingOverloadEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the overload source file, or null when no overloads are needed.</summary>
    public string? Overloads(BindingModel model)
    {
        var overloads = new StringBuilder();
        foreach (var function in model.Functions)
            new BindingTypedOverloadEmitter(context).TypedOverloads(overloads, function);
        BindingCallbackSetterEmitter.CallbackSetters(overloads, model, context.Config.ApiClass);
        foreach (var function in model.Functions)
            BindingStringReturnEmitter.StringReturn(overloads, function, context.Config.ApiClass);
        new BindingSpanReturnEmitter(context).SpanReturns(overloads, model);
        var hasSpanOverloads = context.Config.SpanOverloads && new BindingSpanOverloadEmitter(context).SpanOverloads(overloads, model);
        if (overloads.Length == 0)
            return null;

        var hasStringConvenience = model.Functions.Any(function => function.Parameters.Any(parameter => parameter.HasStringConvenience));
        var output = context.SourceHeader();
        if (hasStringConvenience)
            output.AppendLine("using System.Text;").AppendLine();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine($"/// <summary>Convenience overloads for <see cref=\"{context.Config.ApiClass}\"/>.</summary>");
        output.AppendLine("/// <remarks>These methods adapt managed-friendly argument shapes and forward to the native-shaped API members.</remarks>");
        output.AppendLine($"public{(NeedsUnsafe(model) ? " unsafe" : "")} partial class {context.Config.ApiClass}");
        output.AppendLine("{");
        output.Append(overloads);
        if (hasSpanOverloads)
            BindingSpanOverloadEmitter.ByteLengthHelper(output);
        if (hasStringConvenience)
            BindingUtf8HelperEmitter.Utf8Helper(output);
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Returns true when generated overloads require unsafe code.</summary>
    private bool NeedsUnsafe(BindingModel model) =>
        context.Config.SpanOverloads
        || context.Config.SpanReturns.Count > 0
        || model.Functions.Any(function => function.ReturnsCString || function.Parameters.Any(parameter => parameter.HasStringConvenience));
}

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
            new BindingStringReturnEmitter(context).StringReturn(overloads, function);
        if (context.Config.FreeTypeConvenience)
            BindingFreeTypeOverloadEmitter.FreeTypeOverloads(overloads, model, context.Config.ApiClass);
        if (context.Config.XxHashConvenience)
            BindingXxHashOverloadEmitter.XxHashOverloads(overloads, model, context.Config.ApiClass);
        if (context.Config.FastNoise2Convenience)
            BindingFastNoise2OverloadEmitter.FastNoise2Overloads(overloads, model, context.Config.ApiClass);
        new BindingSpanReturnEmitter(context).SpanReturns(overloads, model);
        new BindingStringArrayReturnEmitter(context).StringArrayReturns(overloads, model);
        new BindingCountedSpanOverloadEmitter(context).CountedSpanOverloads(overloads, model);
        var hasSpanOverloads = context.Config.SpanOverloads && new BindingSpanOverloadEmitter(context).SpanOverloads(overloads, model);
        if (overloads.Length == 0)
            return null;

        var hasStringConvenience = model.Functions.Any(function => function.Parameters.Any(parameter => parameter.HasStringConvenience));
        var needsText = hasStringConvenience || context.Config.FreeTypeConvenience;
        var helpers = new StringBuilder();
        if (hasSpanOverloads)
            BindingSpanOverloadEmitter.ByteLengthHelper(helpers);
        if (hasStringConvenience)
            BindingUtf8HelperEmitter.Utf8Helper(helpers, context.Config.NativeLibrary);
        return TemplateResource.Render(
            typeof(BindingOverloadEmitter),
            "res/templates/bindgen/c-headers/csharp/overloads.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Usings", needsText ? "using System.Text;" + Environment.NewLine + Environment.NewLine : ""),
            ("Namespace", context.Config.Namespace),
            ("ApiClass", context.Config.ApiClass),
            ("NativeLibrary", context.Config.NativeLibrary),
            ("Unsafe", NeedsUnsafe(model) ? " unsafe" : ""),
            ("Overloads", overloads.ToString()),
            ("Helpers", helpers.ToString()));
    }

    /// <summary>Returns true when generated overloads require unsafe code.</summary>
    private bool NeedsUnsafe(BindingModel model) =>
        context.Config.SpanOverloads
        || context.Config.SpanReturns.Count > 0
        || context.Config.StringArrayReturns.Length > 0
        || context.Config.CountedSpanParams.Count > 0
        || context.Config.FreeTypeConvenience
        || context.Config.XxHashConvenience
        || context.Config.FastNoise2Convenience
        || model.Functions.Any(function => function.ReturnsCString || function.Parameters.Any(parameter => parameter.HasStringConvenience));
}

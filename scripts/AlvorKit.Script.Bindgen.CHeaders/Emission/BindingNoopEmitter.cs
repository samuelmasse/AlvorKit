namespace AlvorKit.Script.Bindgen;

/// <summary>Emits a generated null-object API implementation.</summary>
internal sealed class BindingNoopEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the noop class source file.</summary>
    public string Noop(BindingModel model)
    {
        var methods = string.Join("", model.Functions.Select(NoopMethod));
        return TemplateResource.Render(
            typeof(BindingNoopEmitter),
            "res/templates/bindgen/c-headers/csharp/noop.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("ApiClass", context.Config.ApiClass),
            ("NativeLibrary", context.Config.NativeLibrary),
            ("Methods", methods));
    }

    /// <summary>Renders one noop override.</summary>
    private static string NoopMethod(BindingFunction function)
    {
        var outParameters = function.Parameters.Where(parameter => parameter.Modifier == "out").ToList();
        if (outParameters.Count == 0)
            return TemplateResource.Render(
                typeof(BindingNoopEmitter),
                "res/templates/bindgen/c-headers/csharp/noop-expression-method.csfrag.tmpl",
                ("Attributes", BindingMethodAttributes.ForFunction(function)),
                ("ReturnType", function.ReturnType),
                ("ManagedName", function.ManagedName),
                ("Unsafe", BindingSignature.UnsafeModifier(function)),
                ("Signature", BindingSignature.ForFunction(function)),
                ("Body", function.ReturnType == "void" ? "{ }" : "=> default;"));

        var outAssignments = string.Join("", outParameters.Select(parameter => $"        {parameter.ManagedName} = default;{Environment.NewLine}"));
        return TemplateResource.Render(
            typeof(BindingNoopEmitter),
            "res/templates/bindgen/c-headers/csharp/noop-block-method.csfrag.tmpl",
            ("Attributes", BindingMethodAttributes.ForFunction(function)),
            ("ReturnType", function.ReturnType),
            ("ManagedName", function.ManagedName),
            ("Unsafe", BindingSignature.UnsafeModifier(function)),
            ("Signature", BindingSignature.ForFunction(function)),
            ("OutAssignments", outAssignments),
            ("ReturnDefault", function.ReturnType == "void" ? "" : $"        return default;{Environment.NewLine}"));
    }
}

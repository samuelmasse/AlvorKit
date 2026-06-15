namespace AlvorKit.Script.Bindgen;

/// <summary>Emits the generated public API base class.</summary>
internal sealed class BindingApiEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the generated API contract source.</summary>
    public string ApiContract(BindingModel model)
    {
        var members = Members(model);
        return TemplateResource.Render(
            typeof(BindingApiEmitter),
            "res/templates/bindgen/c-headers/csharp/api-contract.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("ApiSummary", context.Config.ApiSummary),
            ("BackendClass", context.Config.BackendClass),
            ("ApiClass", context.Config.ApiClass),
            ("Members", members));
    }

    /// <summary>Renders all generated API members.</summary>
    private static string Members(BindingModel model)
    {
        var output = new StringBuilder();
        var first = true;
        foreach (var function in model.Functions)
        {
            if (!first)
                output.AppendLine();
            first = false;
            BindingDocs.Function(output, function);
            output.Append(Method(function));
        }
        return output.ToString();
    }

    /// <summary>Renders a virtual native function method.</summary>
    private static string Method(BindingFunction function) =>
        TemplateResource.Render(
            typeof(BindingApiEmitter),
            "res/templates/bindgen/c-headers/csharp/api-method.csfrag.tmpl",
            ("Attributes", BindingMethodAttributes.ForFunction(function)),
            ("ReturnType", function.ReturnType),
            ("ManagedName", function.ManagedName),
            ("Unsafe", BindingSignature.UnsafeModifier(function)),
            ("Signature", BindingSignature.ForFunction(function)));
}

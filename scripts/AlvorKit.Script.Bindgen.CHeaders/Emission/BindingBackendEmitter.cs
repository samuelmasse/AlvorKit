namespace AlvorKit.Script.Bindgen;

/// <summary>Emits the generated backend implementation.</summary>
internal sealed class BindingBackendEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the backend class source file.</summary>
    public string Backend(BindingModel model)
    {
        var methods = string.Join(Environment.NewLine, model.Functions.Select(Method));
        return TemplateResource.Render(
            typeof(BindingBackendEmitter),
            "res/templates/bindgen/c-headers/csharp/backend.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("ApiClass", context.Config.ApiClass),
            ("NativeLibrary", context.Config.NativeLibrary),
            ("BackendClass", context.Config.BackendClass),
            ("Methods", methods));
    }

    /// <summary>Renders one backend override.</summary>
    private string Method(BindingFunction function) =>
        TemplateResource.Render(
            typeof(BindingBackendEmitter),
            "res/templates/bindgen/c-headers/csharp/backend-method.csfrag.tmpl",
            ("ReturnType", function.ReturnType),
            ("ManagedName", function.ManagedName),
            ("Signature", BindingSignature.ForFunction(function)),
            ("Body", Body(function)));

    /// <summary>Formats a backend method body for a native import call.</summary>
    private string Body(BindingFunction function)
    {
        var arguments = string.Join(", ", function.Parameters.Select(BindingSignature.NativeArgument));
        var call = $"{context.Config.NativeClass}.{function.ManagedName}({arguments})";
        return function.ReturnType == function.ReturnInteropType ? call
            : function.ReturnType == "bool" ? $"{call} != 0"
            : $"({function.ReturnType}){call}";
    }
}

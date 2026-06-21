namespace AlvorKit.Script.Bindgen;

/// <summary>Emits a generated forwarding wrapper base class.</summary>
internal sealed class BindingWrapperEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the wrapper class source file.</summary>
    public string Wrapper(BindingModel model)
    {
        var methods = string.Join("", model.Functions.Select(Method));
        return TemplateResource.Render(
            typeof(BindingWrapperEmitter),
            "res/templates/bindgen/c-headers/csharp/wrapper.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("ApiClass", context.Config.ApiClass),
            ("NativeLibrary", context.Config.NativeLibrary),
            ("Methods", methods));
    }

    /// <summary>Renders one forwarding override.</summary>
    private static string Method(BindingFunction function)
    {
        var arguments = string.Join(", ", function.Parameters.Select(BindingSignature.Argument));
        return TemplateResource.Render(
            typeof(BindingWrapperEmitter),
            "res/templates/bindgen/c-headers/csharp/wrapper-method.csfrag.tmpl",
            ("Attributes", BindingMethodAttributes.ForFunction(function)),
            ("ReturnType", function.ReturnType),
            ("ManagedName", function.ManagedName),
            ("Unsafe", BindingSignature.UnsafeModifier(function)),
            ("Signature", BindingSignature.ForFunction(function)),
            ("Arguments", arguments));
    }
}

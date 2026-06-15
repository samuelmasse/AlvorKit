namespace AlvorKit.Script.Bindgen;

/// <summary>Emits the generated base OpenGL API contract.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlApiContractEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits the generated base API contract.</summary>
    public string Emit(GlBindingModel model)
    {
        var callbackSetters = new StringBuilder();
        GlCallbackSetterEmitter.Emit(callbackSetters, model);
        return TemplateResource.Render(
            typeof(GlApiContractEmitter),
            "res/templates/bindgen/opengl-registry/csharp/api-contract.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("ApiSummary", context.Config.ApiSummary),
            ("BackendClass", context.Config.BackendClass),
            ("ApiClass", context.Config.ApiClass),
            ("Members", Commands(model.Commands)),
            ("CallbackSetters", callbackSetters.ToString()));
    }

    /// <summary>Renders all raw virtual command members.</summary>
    private static string Commands(IReadOnlyList<GlCommand> commands)
    {
        var output = new StringBuilder();
        var first = true;
        foreach (var command in commands)
        {
            if (!first)
                output.AppendLine();
            first = false;
            GlCommandDocEmitter.Emit(output, command);
            output.Append(Command(command));
        }
        return output.ToString();
    }

    /// <summary>Renders one raw virtual command member.</summary>
    private static string Command(GlCommand command) =>
        TemplateResource.Render(
            typeof(GlApiContractEmitter),
            "res/templates/bindgen/opengl-registry/csharp/api-command.csfrag.tmpl",
            ("ReturnType", command.ReturnType),
            ("ManagedName", command.ManagedName),
            ("Signature", GlSignatureFormatter.Signature(command)));
}

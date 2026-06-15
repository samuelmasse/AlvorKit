namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated function-pointer backends for OpenGL APIs.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlBackendEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits the function-pointer backend implementation.</summary>
    public string Emit(GlBindingModel model)
    {
        return TemplateResource.Render(
            typeof(GlBackendEmitter),
            "res/templates/bindgen/opengl-registry/csharp/backend.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("ApiClass", context.Config.ApiClass),
            ("BackendClass", context.Config.BackendClass),
            ("Fields", Fields(model.Commands)),
            ("Constructor", Constructor(model.Commands)),
            ("Commands", string.Join("", model.Commands.Select(Command))),
            ("NotLoaded", TemplateResource.Read(typeof(GlBackendEmitter), "res/templates/bindgen/opengl-registry/csharp/backend-not-loaded.csfrag.tmpl")));
    }

    /// <summary>Renders resolved native function-pointer fields.</summary>
    private static string Fields(IReadOnlyList<GlCommand> commands) =>
        string.Join("", commands.Select(command => TemplateResource.Render(
            typeof(GlBackendEmitter),
            "res/templates/bindgen/opengl-registry/csharp/backend-field.csfrag.tmpl",
            ("NativeName", command.NativeName),
            ("DelegateType", GlSignatureFormatter.DelegateType(command)))));

    /// <summary>Renders the backend constructor that resolves all entry points.</summary>
    private string Constructor(IReadOnlyList<GlCommand> commands)
    {
        var assignments = string.Join("", commands.Select(command =>
            $"        {command.NativeName} = ({GlSignatureFormatter.DelegateType(command)})getProcAddress(\"{command.NativeName}\");{Environment.NewLine}"));
        return TemplateResource.Render(
            typeof(GlBackendEmitter),
            "res/templates/bindgen/opengl-registry/csharp/backend-constructor.csfrag.tmpl",
            ("BackendClass", context.Config.BackendClass),
            ("Assignments", assignments));
    }

    /// <summary>Renders one backend override.</summary>
    private static string Command(GlCommand command) =>
        TemplateResource.Render(
            typeof(GlBackendEmitter),
            "res/templates/bindgen/opengl-registry/csharp/backend-command.csfrag.tmpl",
            ("ReturnType", command.ReturnType),
            ("ManagedName", command.ManagedName),
            ("Signature", GlSignatureFormatter.Signature(command)),
            ("NativeName", command.NativeName),
            ("Call", GlSignatureFormatter.BackendCall(command)));
}

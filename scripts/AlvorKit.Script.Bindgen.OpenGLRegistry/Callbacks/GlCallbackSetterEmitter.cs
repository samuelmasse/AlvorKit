namespace AlvorKit.Script.Bindgen;

/// <summary>Emits callback setter overloads that root managed delegates on the API instance.</summary>
internal static class GlCallbackSetterEmitter
{
    /// <summary>Emits callback setter overloads for commands with configured callback parameters.</summary>
    public static void Emit(StringBuilder output, GlBindingModel model)
    {
        var setters = model.Commands.Where(command => command.Parameters.Any(parameter => parameter.CallbackType is not null)).ToList();
        if (setters.Count == 0)
            return;

        EmitRootStore(output);
        var slot = 0;
        foreach (var command in setters)
            EmitSetter(output, command, slot++);
    }

    /// <summary>Emits the private callback-root storage and helper method.</summary>
    private static void EmitRootStore(StringBuilder output)
    {
        output.Append(TemplateResource.Read(typeof(GlCallbackSetterEmitter), "res/templates/bindgen/opengl-registry/csharp/callback-root-store.csfrag.tmpl"));
    }

    /// <summary>Emits one typed callback setter overload.</summary>
    private static void EmitSetter(StringBuilder output, GlCommand command, int slot)
    {
        var callbackParameter = command.Parameters.First(parameter => parameter.CallbackType is not null);
        var signature = string.Join(", ", command.Parameters.Select(parameter =>
            parameter == callbackParameter ? $"{parameter.CallbackType}? {parameter.ManagedName}" : $"{parameter.ManagedType} {parameter.ManagedName}"));
        var arguments = string.Join(", ", command.Parameters.Select(parameter =>
            parameter == callbackParameter ? $"RootCallback({slot}, {parameter.ManagedName})" : parameter.ManagedName));
        var cref = string.Join(", ", command.Parameters.Select(parameter => parameter.ManagedType));
        output.Append(TemplateResource.Render(
            typeof(GlCallbackSetterEmitter),
            "res/templates/bindgen/opengl-registry/csharp/callback-setter.csfrag.tmpl",
            ("Cref", $"{command.ManagedName}({cref})"),
            ("NativeName", command.NativeName),
            ("ManagedName", command.ManagedName),
            ("Signature", signature),
            ("Arguments", arguments)));
    }
}

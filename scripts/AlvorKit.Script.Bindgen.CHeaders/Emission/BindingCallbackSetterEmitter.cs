namespace AlvorKit.Script.Bindgen;

/// <summary>Emits callback setter overloads that root delegates on the API instance.</summary>
internal static class BindingCallbackSetterEmitter
{
    /// <summary>Emits callback rooting support and delegate-typed setter overloads.</summary>
    public static void CallbackSetters(StringBuilder output, BindingModel model, string apiClass)
    {
        var setters = model.Functions.Where(function => function.Parameters.Any(parameter => parameter.CallbackType is not null)).ToList();
        if (setters.Count == 0)
            return;

        output.Append(TemplateResource.Read(typeof(BindingCallbackSetterEmitter), "res/templates/bindgen/c-headers/csharp/callback-root-store.csfrag.tmpl"));

        var slot = 0;
        foreach (var function in setters)
            EmitSetter(output, model, function, apiClass, slot++);
    }

    /// <summary>Emits one delegate-typed callback setter overload.</summary>
    private static void EmitSetter(StringBuilder output, BindingModel model, BindingFunction function, string apiClass, int slot)
    {
        var callbackParameter = function.Parameters.First(parameter => parameter.CallbackType is not null);
        var ownerParameter = function.Parameters.FirstOrDefault(parameter => model.Handles.Any(handle => handle.ManagedName == parameter.ManagedType));
        var owner = ownerParameter is not null ? $"{ownerParameter.ManagedName}.Handle" : "0";
        var signature = string.Join(", ", function.Parameters.Select(parameter =>
            parameter == callbackParameter ? $"{parameter.CallbackType}? {parameter.ManagedName}" : $"{parameter.ManagedType} {parameter.ManagedName}"));
        var arguments = string.Join(", ", function.Parameters.Select(parameter =>
            parameter == callbackParameter ? $"RootCallback({owner}, {slot}, {parameter.ManagedName})" : parameter.ManagedName));

        var documentation = new StringBuilder();
        BindingDocs.InheritedConvenience(
            documentation,
            $"{apiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})",
            "Roots the delegate and installs its function pointer; pass null to clear it.");
        output.Append(TemplateResource.Render(
            typeof(BindingCallbackSetterEmitter),
            "res/templates/bindgen/c-headers/csharp/callback-setter.csfrag.tmpl",
            ("Documentation", documentation.ToString()),
            ("ManagedName", function.ManagedName),
            ("Signature", signature),
            ("Arguments", arguments)));
    }
}

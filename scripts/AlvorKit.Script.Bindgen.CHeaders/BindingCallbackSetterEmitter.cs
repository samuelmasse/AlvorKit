namespace AlvorKit.Script.Bindgen;

/// <summary>Emits callback setter overloads that root delegates on the API instance.</summary>
internal static class BindingCallbackSetterEmitter
{
    /// <summary>Emits callback rooting support and delegate-typed setter overloads.</summary>
    public static void CallbackSetters(StringBuilder output, BindingModel model)
    {
        var setters = model.Functions.Where(function => function.Parameters.Any(parameter => parameter.CallbackType is not null)).ToList();
        if (setters.Count == 0)
            return;

        output.AppendLine();
        output.AppendLine("    /// <summary>Delegates rooted by native owner and callback slot.</summary>");
        output.AppendLine("    private Dictionary<(nint Owner, int Slot), Delegate>? rootedCallbacks;");
        output.AppendLine();
        output.AppendLine("    /// <summary>Roots, replaces, or clears a native callback delegate.</summary>");
        output.AppendLine("    private nint RootCallback(nint owner, int slot, Delegate? handler)");
        output.AppendLine("    {");
        output.AppendLine("        if (handler is null) { rootedCallbacks?.Remove((owner, slot)); return 0; }");
        output.AppendLine("        (rootedCallbacks ??= [])[(owner, slot)] = handler;");
        output.AppendLine("        return Marshal.GetFunctionPointerForDelegate(handler);");
        output.AppendLine("    }");

        var slot = 0;
        foreach (var function in setters)
            EmitSetter(output, model, function, slot++);
    }

    /// <summary>Emits one delegate-typed callback setter overload.</summary>
    private static void EmitSetter(StringBuilder output, BindingModel model, BindingFunction function, int slot)
    {
        var callbackParameter = function.Parameters.First(parameter => parameter.CallbackType is not null);
        var ownerParameter = function.Parameters.FirstOrDefault(parameter => model.Handles.Any(handle => handle.ManagedName == parameter.ManagedType));
        var owner = ownerParameter is not null ? $"{ownerParameter.ManagedName}.Handle" : "0";
        var signature = string.Join(", ", function.Parameters.Select(parameter =>
            parameter == callbackParameter ? $"{parameter.CallbackType}? {parameter.ManagedName}" : $"{parameter.ManagedType} {parameter.ManagedName}"));
        var arguments = string.Join(", ", function.Parameters.Select(parameter =>
            parameter == callbackParameter ? $"RootCallback({owner}, {slot}, {parameter.ManagedName})" : parameter.ManagedName));

        output.AppendLine();
        output.AppendLine($"    /// <inheritdoc cref=\"{function.ManagedName}({BindingSignature.Cref(function.Parameters)})\"/>");
        output.AppendLine("    /// <remarks>Convenience overload. Roots the delegate and installs its function pointer; pass null to clear it.</remarks>");
        output.AppendLine($"    public void {function.ManagedName}({signature}) => {function.ManagedName}({arguments});");
    }
}

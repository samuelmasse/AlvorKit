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
        output.AppendLine();
        output.AppendLine("    /// <summary>Delegates rooted for native callback slots installed on this API instance.</summary>");
        output.AppendLine("    private Dictionary<int, Delegate>? rootedCallbacks;");
        output.AppendLine();
        output.AppendLine("    /// <summary>Roots or clears a callback delegate and returns its native function pointer.</summary>");
        output.AppendLine("    private nint RootCallback(int slot, Delegate? handler)");
        output.AppendLine("    {");
        output.AppendLine("        if (handler is null) { rootedCallbacks?.Remove(slot); return 0; }");
        output.AppendLine("        (rootedCallbacks ??= [])[slot] = handler;");
        output.AppendLine("        return Marshal.GetFunctionPointerForDelegate(handler);");
        output.AppendLine("    }");
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
        output.AppendLine();
        output.AppendLine($"    /// <inheritdoc cref=\"{command.ManagedName}({cref})\"/>");
        output.AppendLine(
            "    /// <remarks>Convenience overload. Roots the delegate on this instance and installs " +
            "its function pointer; pass null to clear it.</remarks>");
        output.AppendLine($"    public void {command.ManagedName}({signature}) => {command.ManagedName}({arguments});");
    }
}

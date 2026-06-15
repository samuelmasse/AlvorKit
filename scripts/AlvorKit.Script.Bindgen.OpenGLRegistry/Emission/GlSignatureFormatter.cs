namespace AlvorKit.Script.Bindgen;

/// <summary>Formats common generated OpenGL command signatures and call fragments.</summary>
internal static class GlSignatureFormatter
{
    /// <summary>Formats a public managed command signature.</summary>
    public static string Signature(GlCommand command) =>
        string.Join(", ", command.Parameters.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));

    /// <summary>Formats the raw GL function pointer type.</summary>
    public static string DelegateType(GlCommand command) =>
        $"delegate* unmanaged<{string.Join(", ", command.Parameters.Select(parameter => parameter.InteropType).Append(command.ReturnInteropType))}>";

    /// <summary>Formats a backend call, including bool and enum conversions.</summary>
    public static string BackendCall(GlCommand command)
    {
        var arguments = command.Parameters.Select(parameter =>
            parameter.ManagedType == parameter.InteropType ? parameter.ManagedName
            : parameter.ManagedType == "bool" ? $"{parameter.ManagedName} ? (byte)1 : (byte)0"
            : $"({parameter.InteropType}){parameter.ManagedName}");
        var call = $"{command.NativeName}({string.Join(", ", arguments)})";

        if (command.ReturnType == "void")
            return $"{call};";
        if (command.ReturnType == command.ReturnInteropType)
            return $"return {call};";
        return command.ReturnType == "bool"
            ? $"return {call} != 0;"
            : $"return ({command.ReturnType}){call};";
    }
}

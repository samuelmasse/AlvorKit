namespace AlvorKit.Script.Bindgen;

/// <summary>Emits XML documentation for generated OpenGL extension overloads.</summary>
internal static class GlExtensionDocEmitter
{
    /// <summary>Emits inherited docs plus a remark that names the marshalling performed by the overload.</summary>
    public static void Emit(StringBuilder output, BindgenConfig config, GlCommand command, string detail)
    {
        var cref = GlExtensionNames.CoreCref(config, command);
        output.AppendLine($"    /// <inheritdoc cref=\"{cref}\"/>");
        output.AppendLine($"    /// <remarks>Convenience overload. Calls <see cref=\"{cref}\"/>. {detail}</remarks>");
    }
}

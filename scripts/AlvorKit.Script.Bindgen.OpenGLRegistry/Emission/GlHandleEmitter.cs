namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated strongly typed OpenGL handle wrappers.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlHandleEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits typed wrappers around GL object names plus a generic handle.</summary>
    public string Emit(GlBindingModel model)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        foreach (var handle in model.HandleTypes)
            EmitHandle(output, handle);
        return output.ToString();
    }

    /// <summary>Emits one generated handle record struct.</summary>
    private static void EmitHandle(StringBuilder output, string handle)
    {
        output.AppendLine();
        output.AppendLine(handle == "GlHandle"
            ? "/// <summary>A strongly-typed handle to any OpenGL object; every specific handle widens to it.</summary>"
            : "/// <summary>A strongly-typed OpenGL object handle.</summary>");
        output.AppendLine("/// <param name=\"Handle\">Raw OpenGL object name.</param>");
        output.AppendLine($"public readonly record struct {handle}(uint Handle)");
        output.AppendLine("{");
        if (handle != "GlHandle")
        {
            output.AppendLine("    /// <summary>Widens to the generic <see cref=\"GlHandle\"/> handle.</summary>");
            output.AppendLine($"    public static implicit operator GlHandle({handle} handle) => new(handle.Handle);");
        }
        output.AppendLine("    /// <summary>The raw object name. Explicit: a handle never implicitly becomes an integer.</summary>");
        output.AppendLine($"    public static explicit operator uint({handle} handle) => handle.Handle;");
        output.AppendLine("    /// <summary>Wraps a raw object name. Explicit: an integer is never implicitly a handle.</summary>");
        output.AppendLine($"    public static explicit operator {handle}(uint handle) => new(handle);");
        output.AppendLine("}");
    }
}

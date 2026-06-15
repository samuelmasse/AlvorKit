namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated constants for OpenGL tokens too wide for enum backing types.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlConstantEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits 64-bit sentinel tokens that cannot fit in uint-backed enum types.</summary>
    public string Emit(GlBindingModel model)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine($"/// <summary>OpenGL tokens whose values are too wide for the uint-backed {context.Config.ApiClass} enums.</summary>");
        output.AppendLine($"public static class {context.Config.ApiClass}Constants");
        output.AppendLine("{");
        var first = true;
        foreach (var constant in model.WideConstants)
        {
            if (!first)
                output.AppendLine();
            first = false;
            output.AppendLine($"    /// <summary>{constant.NativeName} ({GlCodeEmissionContext.AvailabilityText(constant.Availability)}).</summary>");
            output.AppendLine($"    public const ulong {constant.ManagedName} = 0x{constant.Value:X};");
        }
        output.AppendLine("}");
        return output.ToString();
    }
}

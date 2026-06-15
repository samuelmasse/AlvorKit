namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated enum source files for OpenGL token groups.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlEnumEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits one generated enum from a registry token group.</summary>
    public string Emit(GlEnumGroup group, bool catchAll)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine(catchAll
            ? $"/// <summary>Every {context.Config.GlApi} {context.Config.GlVersion} {context.Config.GlProfile} token: " +
                "the fallback type for parameters without a typed group.</summary>"
            : $"/// <summary>OpenGL tokens from the <c>{group.NativeName}</c> registry group.</summary>");
        if (group.IsFlags)
            output.AppendLine("[Flags]");
        output.AppendLine($"public enum {group.ManagedName} : uint");
        output.AppendLine("{");
        foreach (var member in group.Members)
            EmitMember(output, member, catchAll);
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Emits one generated enum member.</summary>
    private static void EmitMember(StringBuilder output, GlEnumMember member, bool catchAll)
    {
        var membership = catchAll && member.Groups.Count > 0
            ? $" See {string.Join(", ", member.Groups.Select(group => $"<see cref=\"{group}\"/>"))}."
            : "";
        output.AppendLine($"    /// <summary>{member.NativeName} ({GlCodeEmissionContext.AvailabilityText(member.Availability)}).{membership}</summary>");
        output.AppendLine($"    {member.ManagedName} = 0x{member.Value:X4},");
    }
}

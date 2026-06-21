namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated enum source files for OpenGL token groups.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlEnumEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits one generated enum from a registry token group.</summary>
    public string Emit(GlEnumGroup group, bool catchAll)
    {
        var summary = catchAll
            ? "Native OpenGL token constants from <c>gl.xml</c> used when a command parameter has no narrower registry group."
            : group.UnderlyingType == "ulong"
                ? "Native OpenGL token constants from <c>gl.xml</c> whose values are too wide for the uint-backed catch-all enum."
            : $"OpenGL tokens from the <c>{group.NativeName}</c> registry group.";
        var includeMembership = catchAll || group.UnderlyingType == "ulong";
        var members = string.Join("", group.Members.Select(member => Member(member, includeMembership)));
        return TemplateResource.Render(
            typeof(GlEnumEmitter),
            "res/templates/bindgen/opengl-registry/csharp/enum.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("Summary", summary),
            ("Flags", group.IsFlags ? "[Flags]" + Environment.NewLine : ""),
            ("ManagedName", group.ManagedName),
            ("UnderlyingType", group.UnderlyingType),
            ("Members", members));
    }

    /// <summary>Renders one generated enum member.</summary>
    private static string Member(GlEnumMember member, bool includeMembership)
    {
        var membership = includeMembership && member.Groups.Count > 0
            ? $" See {string.Join(", ", member.Groups.Select(group => $"<see cref=\"{group}\"/>"))}."
            : "";
        return TemplateResource.Render(
            typeof(GlEnumEmitter),
            "res/templates/bindgen/opengl-registry/csharp/enum-member.csfrag.tmpl",
            ("NativeName", $"<c>{member.NativeName}</c>"),
            ("Availability", GlCodeEmissionContext.AvailabilityText(member.Availability)),
            ("Membership", membership),
            ("ManagedName", member.ManagedName),
            ("Value", member.Value.ToString("X4")));
    }
}

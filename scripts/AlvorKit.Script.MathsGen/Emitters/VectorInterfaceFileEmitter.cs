namespace AlvorKit.Script.MathsGen;

/// <summary>Emits vector interfaces whose signatures depend on generated concrete vector types.</summary>
internal static class VectorInterfaceFileEmitter
{
    /// <summary>Returns source files for vector interfaces that cannot live in the core project.</summary>
    public static IReadOnlyList<(string FileName, string Source)> EmitAll() =>
    [
        ("IVec2Mask.g.cs", MathsTemplate.Render("vec2-mask-interface.cs.tmpl")),
        ("IVec3Mask.g.cs", MathsTemplate.Render("vec3-mask-interface.cs.tmpl")),
        ("IVec4Mask.g.cs", MathsTemplate.Render("vec4-mask-interface.cs.tmpl")),
        ("IVec2Floating.g.cs", MathsTemplate.Render("vec2-floating-interface.cs.tmpl")),
        ("IVec3Floating.g.cs", MathsTemplate.Render("vec3-floating-interface.cs.tmpl")),
        ("IVec4Floating.g.cs", MathsTemplate.Render("vec4-floating-interface.cs.tmpl")),
        ("IVec2SignedInteger.g.cs", MathsTemplate.Render("vec2-signed-integer-interface.cs.tmpl")),
        ("IVec3SignedInteger.g.cs", MathsTemplate.Render("vec3-signed-integer-interface.cs.tmpl")),
        ("IVec4SignedInteger.g.cs", MathsTemplate.Render("vec4-signed-integer-interface.cs.tmpl")),
        ("IVec2UnsignedInteger.g.cs", MathsTemplate.Render("vec2-unsigned-integer-interface.cs.tmpl")),
        ("IVec3UnsignedInteger.g.cs", MathsTemplate.Render("vec3-unsigned-integer-interface.cs.tmpl")),
        ("IVec4UnsignedInteger.g.cs", MathsTemplate.Render("vec4-unsigned-integer-interface.cs.tmpl")),
    ];
}

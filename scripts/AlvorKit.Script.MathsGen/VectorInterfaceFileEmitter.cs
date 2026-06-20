namespace AlvorKit.Script.MathsGen;

/// <summary>Emits vector interface source files for the primitives package.</summary>
internal static class VectorInterfaceFileEmitter
{
    /// <summary>Returns source files for vector interfaces.</summary>
    public static IReadOnlyList<(string FileName, string Source)> EmitAll() =>
    [
        ("IVec.g.cs", MathsTemplate.Render("vector-interface.cs.tmpl")),
        ("IVec2.g.cs", MathsTemplate.Render("vec2-interface.cs.tmpl")),
        ("IVec3.g.cs", MathsTemplate.Render("vec3-interface.cs.tmpl")),
        ("IVec4.g.cs", MathsTemplate.Render("vec4-interface.cs.tmpl")),
        ("IVecMask.g.cs", MathsTemplate.Render("vec-mask-interface.cs.tmpl")),
        ("IVec2Mask.g.cs", MathsTemplate.Render("vec2-mask-interface.cs.tmpl")),
        ("IVec3Mask.g.cs", MathsTemplate.Render("vec3-mask-interface.cs.tmpl")),
        ("IVec4Mask.g.cs", MathsTemplate.Render("vec4-mask-interface.cs.tmpl")),
        ("IVecRelationalOperators.g.cs", MathsTemplate.Render("vec-relational-operators-interface.cs.tmpl")),
        ("IVecMetric.g.cs", MathsTemplate.Render("vec-metric-interface.cs.tmpl")),
        ("IVecNumeric.g.cs", MathsTemplate.Render("vec-numeric-interface.cs.tmpl")),
        ("IVecSignedNumeric.g.cs", MathsTemplate.Render("vec-signed-numeric-interface.cs.tmpl")),
        ("IVecInteger.g.cs", MathsTemplate.Render("vec-integer-interface.cs.tmpl")),
        ("IVecIntegerCountShiftOperators.g.cs", MathsTemplate.Render("vec-integer-count-shift-operators-interface.cs.tmpl")),
        ("IVecFloating.g.cs", MathsTemplate.Render("vec-floating-interface.cs.tmpl")),
        ("IVecFloatingGeometry.g.cs", MathsTemplate.Render("vec-floating-geometry-interface.cs.tmpl")),
        ("IVecFloatingScalarFunctions.g.cs", MathsTemplate.Render("vec-floating-scalar-functions-interface.cs.tmpl")),
        ("IVecFloatingVectorInterpolation.g.cs", MathsTemplate.Render("vec-floating-vector-interpolation-interface.cs.tmpl")),
        ("IVec3Cross.g.cs", MathsTemplate.Render("vec3-cross-interface.cs.tmpl")),
        ("IVec2Axes.g.cs", MathsTemplate.Render("vec2-axes-interface.cs.tmpl")),
        ("IVec3Axes.g.cs", MathsTemplate.Render("vec3-axes-interface.cs.tmpl")),
        ("IVec4Axes.g.cs", MathsTemplate.Render("vec4-axes-interface.cs.tmpl")),
        ("IVec2Planar.g.cs", MathsTemplate.Render("vec2-planar-interface.cs.tmpl")),
        ("IVecScalarArithmeticOperators.g.cs", MathsTemplate.Render("vec-scalar-arithmetic-operators-interface.cs.tmpl")),
        ("IVecScalarIntegerOperators.g.cs", MathsTemplate.Render("vec-scalar-integer-operators-interface.cs.tmpl")),
        ("IVec2Floating.g.cs", MathsTemplate.Render("vec2-floating-interface.cs.tmpl")),
        ("IVec2FloatingToInteger.g.cs", MathsTemplate.Render("vec2-floating-to-integer-interface.cs.tmpl")),
        ("IVec2SystemNumerics.g.cs", MathsTemplate.Render("vec2-system-numerics-interface.cs.tmpl")),
        ("IVec3Floating.g.cs", MathsTemplate.Render("vec3-floating-interface.cs.tmpl")),
        ("IVec3FloatingToInteger.g.cs", MathsTemplate.Render("vec3-floating-to-integer-interface.cs.tmpl")),
        ("IVec3SystemNumerics.g.cs", MathsTemplate.Render("vec3-system-numerics-interface.cs.tmpl")),
        ("IVec4Floating.g.cs", MathsTemplate.Render("vec4-floating-interface.cs.tmpl")),
        ("IVec4FloatingToInteger.g.cs", MathsTemplate.Render("vec4-floating-to-integer-interface.cs.tmpl")),
        ("IVec4SystemNumerics.g.cs", MathsTemplate.Render("vec4-system-numerics-interface.cs.tmpl")),
        ("IVec2SignedInteger.g.cs", MathsTemplate.Render("vec2-signed-integer-interface.cs.tmpl")),
        ("IVec3SignedInteger.g.cs", MathsTemplate.Render("vec3-signed-integer-interface.cs.tmpl")),
        ("IVec4SignedInteger.g.cs", MathsTemplate.Render("vec4-signed-integer-interface.cs.tmpl")),
        ("IVec2UnsignedInteger.g.cs", MathsTemplate.Render("vec2-unsigned-integer-interface.cs.tmpl")),
        ("IVec3UnsignedInteger.g.cs", MathsTemplate.Render("vec3-unsigned-integer-interface.cs.tmpl")),
        ("IVec4UnsignedInteger.g.cs", MathsTemplate.Render("vec4-unsigned-integer-interface.cs.tmpl")),
    ];
}

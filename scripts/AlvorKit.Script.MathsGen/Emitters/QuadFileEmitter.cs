namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated 3D quad source file.</summary>
internal static class QuadFileEmitter
{
    /// <summary>Returns source code for <paramref name="quad"/>.</summary>
    public static string Emit(QuadSpec quad) =>
        MathsTemplate.Render(
            "quad-file.cs.tmpl",
            ("TypeSummary", TypeSummary(quad)),
            ("TypeName", quad.TypeName),
            ("ScalarType", quad.Scalar.CSharpName),
            ("Vector3Type", quad.Vector3TypeName),
            ("Box3Type", quad.Box3TypeName),
            ("SizeBytes", quad.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("FourLiteral", quad.FourLiteral),
            ("CrossScalarConversions", CrossScalarConversions(quad)),
            ("ImplementedInterfaces", ImplementedInterfaces(quad)));

    private static string ImplementedInterfaces(QuadSpec quad) =>
        $"IQuad3<{quad.TypeName}, {quad.Scalar.CSharpName}, {quad.Vector3TypeName}, {quad.Box3TypeName}>";

    private static string TypeSummary(QuadSpec quad) =>
        $"{Capitalized(quad.Scalar.Description)} 3D quad with top-left, top-right, bottom-left, and bottom-right corners.";

    private static string CrossScalarConversions(QuadSpec quad)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in QuadCatalog.Scalars)
        {
            if (targetScalar == quad.Scalar)
                continue;

            var target = new QuadSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(quad.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "quad-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("KeywordCapitalized", Capitalized(keyword)),
                ("TargetType", target.TypeName),
                ("SourceType", quad.TypeName),
                ("TargetVector3Type", target.Vector3TypeName)));
        }

        return builder.ToString();
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

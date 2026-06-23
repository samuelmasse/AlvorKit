namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated 3D sphere source file.</summary>
internal static class SphereFileEmitter
{
    /// <summary>Returns source code for <paramref name="sphere"/>.</summary>
    public static string Emit(SphereSpec sphere) =>
        MathsTemplate.Render(
            "sphere-file.cs.tmpl",
            ("TypeSummary", TypeSummary(sphere)),
            ("TypeName", sphere.TypeName),
            ("ScalarType", sphere.Scalar.CSharpName),
            ("Vector3Type", sphere.Vector3TypeName),
            ("Box3Type", sphere.Box3TypeName),
            ("SizeBytes", sphere.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", sphere.Scalar.ZeroLiteral),
            ("OneLiteral", sphere.Scalar.OneLiteral),
            ("TwoLiteral", sphere.Scalar.TwoLiteral),
            ("CrossScalarConversions", CrossScalarConversions(sphere)),
            ("ImplementedInterfaces", ImplementedInterfaces(sphere)));

    private static string ImplementedInterfaces(SphereSpec sphere) =>
        $"ISphere3<{sphere.TypeName}, {sphere.Scalar.CSharpName}, {sphere.Vector3TypeName}, {sphere.Box3TypeName}>";

    private static string TypeSummary(SphereSpec sphere) =>
        $"{Capitalized(sphere.Scalar.Description)} 3D sphere for spatial queries.";

    private static string CrossScalarConversions(SphereSpec sphere)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in SphereCatalog.Scalars)
        {
            if (targetScalar == sphere.Scalar)
                continue;

            var target = new SphereSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(sphere.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "sphere-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("KeywordCapitalized", Capitalized(keyword)),
                ("TargetType", target.TypeName),
                ("SourceType", sphere.TypeName),
                ("TargetVector3Type", target.Vector3TypeName),
                ("TargetScalarType", targetScalar.CSharpName)));
        }

        return builder.ToString();
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

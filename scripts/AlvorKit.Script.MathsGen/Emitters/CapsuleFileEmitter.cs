namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated 3D capsule source file.</summary>
internal static class CapsuleFileEmitter
{
    /// <summary>Returns source code for <paramref name="capsule"/>.</summary>
    public static string Emit(CapsuleSpec capsule) =>
        MathsTemplate.Render(
            "capsule-file.cs.tmpl",
            ("TypeSummary", TypeSummary(capsule)),
            ("TypeName", capsule.TypeName),
            ("ScalarType", capsule.Scalar.CSharpName),
            ("Vector3Type", capsule.Vector3TypeName),
            ("Vector4Type", capsule.Vector4TypeName),
            ("Segment3Type", capsule.Segment3TypeName),
            ("Plane3Type", capsule.Plane3TypeName),
            ("Ray3Type", capsule.Ray3TypeName),
            ("Box3Type", capsule.Box3TypeName),
            ("Sphere3Type", capsule.Sphere3TypeName),
            ("Frustum3Type", capsule.Frustum3TypeName),
            ("IntervalType", capsule.IntervalTypeName),
            ("SizeBytes", capsule.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", capsule.Scalar.ZeroLiteral),
            ("OneLiteral", capsule.Scalar.OneLiteral),
            ("TwoLiteral", capsule.Scalar.TwoLiteral),
            ("PositiveInfinity", $"{capsule.Scalar.CSharpName}.PositiveInfinity"),
            ("CrossScalarConversions", CrossScalarConversions(capsule)),
            ("ImplementedInterfaces", ImplementedInterfaces(capsule)));

    private static string ImplementedInterfaces(CapsuleSpec capsule) =>
        $"ICapsule3<{capsule.TypeName}, {capsule.Scalar.CSharpName}, {capsule.Vector3TypeName}, " +
        $"{capsule.Vector4TypeName}, {capsule.Segment3TypeName}, {capsule.Plane3TypeName}, {capsule.Ray3TypeName}, " +
        $"{capsule.Box3TypeName}, {capsule.Sphere3TypeName}, {capsule.Frustum3TypeName}, {capsule.IntervalTypeName}>";

    private static string TypeSummary(CapsuleSpec capsule) =>
        $"{Capitalized(capsule.Scalar.Description)} 3D capsule for spatial queries.";

    private static string CrossScalarConversions(CapsuleSpec capsule)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in CapsuleCatalog.Scalars)
        {
            if (targetScalar == capsule.Scalar)
                continue;

            var target = new CapsuleSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(capsule.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "capsule-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("KeywordCapitalized", Capitalized(keyword)),
                ("TargetType", target.TypeName),
                ("SourceType", capsule.TypeName),
                ("TargetSegment3Type", target.Segment3TypeName),
                ("TargetScalarType", targetScalar.CSharpName)));
        }

        return builder.ToString();
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

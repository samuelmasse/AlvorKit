namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated 3D segment source file.</summary>
internal static class SegmentFileEmitter
{
    /// <summary>Returns source code for <paramref name="segment"/>.</summary>
    public static string Emit(SegmentSpec segment) =>
        MathsTemplate.Render(
            "segment-file.cs.tmpl",
            ("TypeSummary", TypeSummary(segment)),
            ("TypeName", segment.TypeName),
            ("ScalarType", segment.Scalar.CSharpName),
            ("Vector3Type", segment.Vector3TypeName),
            ("Vector4Type", segment.Vector4TypeName),
            ("Plane3Type", segment.Plane3TypeName),
            ("Box3Type", segment.Box3TypeName),
            ("Sphere3Type", segment.Sphere3TypeName),
            ("SizeBytes", segment.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", segment.Scalar.ZeroLiteral),
            ("OneLiteral", segment.Scalar.OneLiteral),
            ("TwoLiteral", segment.Scalar.TwoLiteral),
            ("PositiveInfinity", $"{segment.Scalar.CSharpName}.PositiveInfinity"),
            ("CrossScalarConversions", CrossScalarConversions(segment)),
            ("ImplementedInterfaces", ImplementedInterfaces(segment)));

    private static string ImplementedInterfaces(SegmentSpec segment) =>
        $"ISegment3<{segment.TypeName}, {segment.Scalar.CSharpName}, {segment.Vector3TypeName}, {segment.Vector4TypeName}, " +
        $"{segment.Plane3TypeName}, {segment.Box3TypeName}, {segment.Sphere3TypeName}>";

    private static string TypeSummary(SegmentSpec segment) =>
        $"{Capitalized(segment.Scalar.Description)} finite 3D line segment for spatial queries.";

    private static string CrossScalarConversions(SegmentSpec segment)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in SegmentCatalog.Scalars)
        {
            if (targetScalar == segment.Scalar)
                continue;

            var target = new SegmentSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(segment.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "segment-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("KeywordCapitalized", Capitalized(keyword)),
                ("TargetType", target.TypeName),
                ("SourceType", segment.TypeName),
                ("TargetVector3Type", target.Vector3TypeName)));
        }

        return builder.ToString();
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated 3D triangle source file.</summary>
internal static class TriangleFileEmitter
{
    /// <summary>Returns source code for <paramref name="triangle"/>.</summary>
    public static string Emit(TriangleSpec triangle) =>
        MathsTemplate.Render(
            "triangle-file.cs.tmpl",
            ("TypeSummary", TypeSummary(triangle)),
            ("TypeName", triangle.TypeName),
            ("ScalarType", triangle.Scalar.CSharpName),
            ("Vector3Type", triangle.Vector3TypeName),
            ("Vector4Type", triangle.Vector4TypeName),
            ("Plane3Type", triangle.Plane3TypeName),
            ("Ray3Type", triangle.Ray3TypeName),
            ("Box3Type", triangle.Box3TypeName),
            ("Sphere3Type", triangle.Sphere3TypeName),
            ("Frustum3Type", triangle.Frustum3TypeName),
            ("IntervalType", triangle.IntervalTypeName),
            ("SizeBytes", triangle.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", triangle.Scalar.ZeroLiteral),
            ("OneLiteral", triangle.Scalar.OneLiteral),
            ("TwoLiteral", triangle.Scalar.TwoLiteral),
            ("CrossScalarConversions", CrossScalarConversions(triangle)),
            ("ImplementedInterfaces", ImplementedInterfaces(triangle)));

    private static string ImplementedInterfaces(TriangleSpec triangle) =>
        $"ITriangle3<{triangle.TypeName}, {triangle.Scalar.CSharpName}, {triangle.Vector3TypeName}, " +
        $"{triangle.Vector4TypeName}, {triangle.Plane3TypeName}, {triangle.Ray3TypeName}, {triangle.Box3TypeName}, " +
        $"{triangle.Sphere3TypeName}, {triangle.Frustum3TypeName}, {triangle.IntervalTypeName}>";

    private static string TypeSummary(TriangleSpec triangle) =>
        $"{Capitalized(triangle.Scalar.Description)} finite 3D triangle for spatial queries.";

    private static string CrossScalarConversions(TriangleSpec triangle)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in TriangleCatalog.Scalars)
        {
            if (targetScalar == triangle.Scalar)
                continue;

            var target = new TriangleSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(triangle.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "triangle-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("KeywordCapitalized", Capitalized(keyword)),
                ("TargetType", target.TypeName),
                ("SourceType", triangle.TypeName),
                ("TargetVector3Type", target.Vector3TypeName)));
        }

        return builder.ToString();
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

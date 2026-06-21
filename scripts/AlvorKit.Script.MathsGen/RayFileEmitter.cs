namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated 3D ray source file.</summary>
internal static class RayFileEmitter
{
    /// <summary>Returns source code for <paramref name="ray"/>.</summary>
    public static string Emit(RaySpec ray) =>
        MathsTemplate.Render(
            "ray-file.cs.tmpl",
            ("TypeSummary", TypeSummary(ray)),
            ("TypeName", ray.TypeName),
            ("ScalarType", ray.Scalar.CSharpName),
            ("Vector3Type", ray.Vector3TypeName),
            ("Vector4Type", ray.Vector4TypeName),
            ("Plane3Type", ray.Plane3TypeName),
            ("Box3Type", ray.Box3TypeName),
            ("Sphere3Type", ray.Sphere3TypeName),
            ("Frustum3Type", ray.Frustum3TypeName),
            ("IntervalType", ray.IntervalTypeName),
            ("SizeBytes", ray.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", ray.Scalar.ZeroLiteral),
            ("TwoLiteral", ray.Scalar.TwoLiteral),
            ("FourLiteral", ray.Scalar.Kind == ScalarKind.Float ? "4f" : "4d"),
            ("PositiveInfinity", $"{ray.Scalar.CSharpName}.PositiveInfinity"),
            ("CrossScalarConversions", CrossScalarConversions(ray)),
            ("ImplementedInterfaces", ImplementedInterfaces(ray)));

    private static string ImplementedInterfaces(RaySpec ray) =>
        $"IRay3<{ray.TypeName}, {ray.Scalar.CSharpName}, {ray.Vector3TypeName}, {ray.Vector4TypeName}, " +
        $"{ray.Plane3TypeName}, {ray.Box3TypeName}, {ray.Sphere3TypeName}, {ray.Frustum3TypeName}, {ray.IntervalTypeName}>";

    private static string TypeSummary(RaySpec ray) =>
        $"{Capitalized(ray.Scalar.Description)} 3D ray for spatial queries.";

    private static string CrossScalarConversions(RaySpec ray)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in RayCatalog.Scalars)
        {
            if (targetScalar == ray.Scalar)
                continue;

            var target = new RaySpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(ray.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "ray-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("KeywordCapitalized", Capitalized(keyword)),
                ("TargetType", target.TypeName),
                ("SourceType", ray.TypeName),
                ("TargetVector3Type", target.Vector3TypeName)));
        }

        return builder.ToString();
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated 3D oriented bounding box source file.</summary>
internal static class ObbFileEmitter
{
    /// <summary>Returns source code for <paramref name="obb"/>.</summary>
    public static string Emit(ObbSpec obb) =>
        MathsTemplate.Render(
            "obb-file.cs.tmpl",
            ("TypeSummary", TypeSummary(obb)),
            ("TypeName", obb.TypeName),
            ("ScalarType", obb.Scalar.CSharpName),
            ("Vector3Type", obb.Vector3TypeName),
            ("Vector4Type", obb.Vector4TypeName),
            ("QuaternionType", obb.QuaternionTypeName),
            ("Matrix4Type", obb.Matrix4TypeName),
            ("Plane3Type", obb.Plane3TypeName),
            ("Box3Type", obb.Box3TypeName),
            ("Sphere3Type", obb.Sphere3TypeName),
            ("Frustum3Type", obb.Frustum3TypeName),
            ("SizeBytes", obb.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", obb.Scalar.ZeroLiteral),
            ("OneLiteral", obb.Scalar.OneLiteral),
            ("TwoLiteral", obb.Scalar.TwoLiteral),
            ("CrossScalarConversions", CrossScalarConversions(obb)),
            ("ImplementedInterfaces", ImplementedInterfaces(obb)));

    private static string ImplementedInterfaces(ObbSpec obb) =>
        $"IObb3<{obb.TypeName}, {obb.Scalar.CSharpName}, {obb.Vector3TypeName}, {obb.Vector4TypeName}, " +
        $"{obb.QuaternionTypeName}, {obb.Plane3TypeName}, {obb.Box3TypeName}, {obb.Sphere3TypeName}, {obb.Frustum3TypeName}>";

    private static string TypeSummary(ObbSpec obb) =>
        $"{Capitalized(obb.Scalar.Description)} 3D oriented bounding box for spatial queries.";

    private static string CrossScalarConversions(ObbSpec obb)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in ObbCatalog.Scalars)
        {
            if (targetScalar == obb.Scalar)
                continue;

            var target = new ObbSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(obb.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "obb-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("KeywordCapitalized", Capitalized(keyword)),
                ("TargetType", target.TypeName),
                ("SourceType", obb.TypeName),
                ("TargetVector3Type", target.Vector3TypeName),
                ("TargetQuaternionType", target.QuaternionTypeName)));
        }

        return builder.ToString();
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

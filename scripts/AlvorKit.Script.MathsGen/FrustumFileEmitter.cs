namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated 3D frustum source file.</summary>
internal static class FrustumFileEmitter
{
    /// <summary>Returns source code for <paramref name="frustum"/>.</summary>
    public static string Emit(FrustumSpec frustum) =>
        MathsTemplate.Render(
            "frustum-file.cs.tmpl",
            ("TypeSummary", TypeSummary(frustum)),
            ("TypeName", frustum.TypeName),
            ("ScalarType", frustum.Scalar.CSharpName),
            ("Vector3Type", frustum.Vector3TypeName),
            ("Vector4Type", frustum.Vector4TypeName),
            ("Matrix4Type", frustum.Matrix4TypeName),
            ("Plane3Type", frustum.Plane3TypeName),
            ("Box3Type", frustum.Box3TypeName),
            ("Sphere3Type", frustum.Sphere3TypeName),
            ("Capsule3Type", frustum.Capsule3TypeName),
            ("Obb3Type", frustum.Obb3TypeName),
            ("SizeBytes", frustum.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", frustum.Scalar.ZeroLiteral),
            ("CrossScalarConversions", CrossScalarConversions(frustum)),
            ("ImplementedInterfaces", ImplementedInterfaces(frustum)));

    private static string ImplementedInterfaces(FrustumSpec frustum) =>
        $"IFrustum3Transform<{frustum.TypeName}, {frustum.Scalar.CSharpName}, {frustum.Vector3TypeName}, " +
        $"{frustum.Vector4TypeName}, {frustum.Matrix4TypeName}, {frustum.Plane3TypeName}, {frustum.Box3TypeName}>," +
        Environment.NewLine +
        $"    IFrustum3Sphere<{frustum.TypeName}, {frustum.Scalar.CSharpName}, {frustum.Vector3TypeName}, " +
        $"{frustum.Vector4TypeName}, {frustum.Plane3TypeName}, {frustum.Box3TypeName}, {frustum.Sphere3TypeName}>";

    private static string TypeSummary(FrustumSpec frustum) =>
        $"{Capitalized(frustum.Scalar.Description)} 3D frustum volume.";

    private static string CrossScalarConversions(FrustumSpec frustum)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in FrustumCatalog.Scalars)
        {
            if (targetScalar == frustum.Scalar)
                continue;

            var target = new FrustumSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(frustum.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "frustum-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("TargetType", target.TypeName),
                ("SourceType", frustum.TypeName),
                ("TargetPlane3Type", target.Plane3TypeName)));
        }

        return builder.ToString();
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

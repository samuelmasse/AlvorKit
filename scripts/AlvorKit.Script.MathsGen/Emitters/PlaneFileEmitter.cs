namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated 3D plane source file.</summary>
internal static class PlaneFileEmitter
{
    /// <summary>Returns source code for <paramref name="plane"/>.</summary>
    public static string Emit(PlaneSpec plane) =>
        MathsTemplate.Render(
            "plane-file.cs.tmpl",
            ("TypeSummary", TypeSummary(plane)),
            ("TypeName", plane.TypeName),
            ("ScalarType", plane.Scalar.CSharpName),
            ("Vector3Type", plane.Vector3TypeName),
            ("Vector4Type", plane.Vector4TypeName),
            ("Box3Type", plane.Box3TypeName),
            ("Sphere3Type", plane.Sphere3TypeName),
            ("Obb3Type", plane.Obb3TypeName),
            ("Matrix4Type", plane.Matrix4TypeName),
            ("QuaternionType", plane.QuaternionTypeName),
            ("SizeBytes", plane.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", plane.Scalar.ZeroLiteral),
            ("OneLiteral", plane.Scalar.OneLiteral),
            ("TwoLiteral", plane.Scalar.TwoLiteral),
            ("CrossScalarConversions", CrossScalarConversions(plane)),
            ("SystemNumericsConversions", SystemNumericsConversions(plane)),
            ("ImplementedInterfaces", ImplementedInterfaces(plane)));

    private static string ImplementedInterfaces(PlaneSpec plane)
    {
        var interfaces = new List<string>
        {
            $"IPlane3Transform<{plane.TypeName}, {plane.Scalar.CSharpName}, {plane.Vector3TypeName}, " +
            $"{plane.Vector4TypeName}, {plane.Matrix4TypeName}, {plane.QuaternionTypeName}>",
        };

        if (plane.Scalar.Kind == ScalarKind.Float)
            interfaces.Add($"IPlane3SystemNumerics<{plane.TypeName}>");

        return string.Join($",{Environment.NewLine}      ", interfaces);
    }

    private static string TypeSummary(PlaneSpec plane) =>
        $"{Capitalized(plane.Scalar.Description)} 3D plane in dot-normal-plus-offset form.";

    private static string CrossScalarConversions(PlaneSpec plane)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in PlaneCatalog.Scalars)
        {
            if (targetScalar == plane.Scalar)
                continue;

            var target = new PlaneSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(plane.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "plane-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("KeywordCapitalized", Capitalized(keyword)),
                ("TargetType", target.TypeName),
                ("SourceType", plane.TypeName),
                ("TargetVector3Type", target.Vector3TypeName),
                ("TargetScalarType", targetScalar.CSharpName)));
        }

        return builder.ToString();
    }

    private static string SystemNumericsConversions(PlaneSpec plane) =>
        plane.Scalar.Kind == ScalarKind.Float
            ? MathsTemplate.Fragment("plane-system-numerics.csfrag.tmpl", ("TypeName", plane.TypeName))
            : string.Empty;

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

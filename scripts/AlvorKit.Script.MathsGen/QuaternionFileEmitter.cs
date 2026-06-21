namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated quaternion source file.</summary>
internal static class QuaternionFileEmitter
{
    /// <summary>Returns source code for <paramref name="quaternion"/>.</summary>
    public static string Emit(QuaternionSpec quaternion) =>
        MathsTemplate.Render(
            "quat-file.cs.tmpl",
            ("TypeSummary", TypeSummary(quaternion)),
            ("TypeName", quaternion.TypeName),
            ("ScalarType", quaternion.Scalar.CSharpName),
            ("Suffix", quaternion.Scalar.Suffix),
            ("Vector3Type", quaternion.Vector3TypeName),
            ("Vector4Type", quaternion.Vector4TypeName),
            ("MaskType", quaternion.MaskTypeName),
            ("Matrix3Type", quaternion.Matrix3TypeName),
            ("Matrix4Type", quaternion.Matrix4TypeName),
            ("SizeBytes", quaternion.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", quaternion.Scalar.ZeroLiteral),
            ("OneLiteral", quaternion.Scalar.OneLiteral),
            ("TwoLiteral", quaternion.Scalar.TwoLiteral),
            ("ThreeLiteral", quaternion.Scalar.ThreeLiteral),
            ("FourLiteral", FourLiteral(quaternion.Scalar)),
            ("HalfLiteral", HalfLiteral(quaternion.Scalar)),
            ("QuarterLiteral", QuarterLiteral(quaternion.Scalar)),
            ("MinusOneLiteral", MinusOneLiteral(quaternion.Scalar)),
            ("EpsilonLiteral", $"{quaternion.Scalar.CSharpName}.Epsilon"),
            ("ToleranceLiteral", ToleranceLiteral(quaternion.Scalar)),
            ("PositiveInfinityLiteral", $"{quaternion.Scalar.CSharpName}.PositiveInfinity"),
            ("PiLiteral", $"{quaternion.Scalar.CSharpName}.Pi"),
            ("CrossScalarConversions", CrossScalarConversions(quaternion)),
            ("SystemNumericsConversions", SystemNumericsConversions(quaternion)),
            ("ImplementedInterfaces", ImplementedInterfaces(quaternion)));

    private static string ImplementedInterfaces(QuaternionSpec quaternion)
    {
        var interfaces = new List<string>
        {
            $"IQuat<{quaternion.TypeName}, {quaternion.Scalar.CSharpName}, {quaternion.Vector3TypeName}, " +
            $"{quaternion.Vector4TypeName}, {quaternion.MaskTypeName}, {quaternion.Matrix3TypeName}, {quaternion.Matrix4TypeName}>",
        };

        if (quaternion.Scalar.Kind == ScalarKind.Float)
            interfaces.Add($"IQuatSystemNumerics<{quaternion.TypeName}>");

        return string.Join($",{Environment.NewLine}      ", interfaces);
    }

    private static string TypeSummary(QuaternionSpec quaternion) =>
        $"{quaternion.Scalar.Description} quaternion for 3D rotations and orientation interpolation.";

    private static string CrossScalarConversions(QuaternionSpec quaternion)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in QuaternionCatalog.Scalars)
        {
            if (targetScalar == quaternion.Scalar)
                continue;

            var target = new QuaternionSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(quaternion.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment("quat-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("TargetType", target.TypeName),
                ("SourceType", quaternion.TypeName),
                ("TargetScalarType", targetScalar.CSharpName)));
        }

        return builder.ToString();
    }

    private static string SystemNumericsConversions(QuaternionSpec quaternion) =>
        quaternion.Scalar.Kind == ScalarKind.Float
            ? MathsTemplate.Fragment("quat-system-numerics.csfrag.tmpl", ("TypeName", quaternion.TypeName))
            : string.Empty;

    private static string HalfLiteral(ScalarSpec scalar) => scalar.Kind switch
    {
        ScalarKind.Float => "0.5f",
        ScalarKind.Double => "0.5d",
        _ => "0.5",
    };

    private static string QuarterLiteral(ScalarSpec scalar) => scalar.Kind switch
    {
        ScalarKind.Float => "0.25f",
        ScalarKind.Double => "0.25d",
        _ => "0.25",
    };

    private static string FourLiteral(ScalarSpec scalar) => scalar.Kind switch
    {
        ScalarKind.Float => "4f",
        ScalarKind.Double => "4d",
        _ => "4",
    };

    private static string MinusOneLiteral(ScalarSpec scalar) => scalar.Kind switch
    {
        ScalarKind.Float => "-1f",
        ScalarKind.Double => "-1d",
        _ => "-1",
    };

    private static string ToleranceLiteral(ScalarSpec scalar) => scalar.Kind switch
    {
        ScalarKind.Float => "1e-6f",
        ScalarKind.Double => "1e-12d",
        _ => "0",
    };
}

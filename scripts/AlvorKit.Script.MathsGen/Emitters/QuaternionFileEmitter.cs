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
            ("LayoutKind", quaternion.Scalar.Kind == ScalarKind.Float ? "Explicit" : "Sequential"),
            ("XFieldOffset", FieldOffset(quaternion, 0)),
            ("YFieldOffset", FieldOffset(quaternion, 1)),
            ("ZFieldOffset", FieldOffset(quaternion, 2)),
            ("WFieldOffset", FieldOffset(quaternion, 3)),
            ("PackedStorage", PackedStorage(quaternion)),
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
            ("RegisterType", QuaternionSimdExpression.RegisterType(quaternion)),
            ("RegisterApi", QuaternionSimdExpression.RegisterApi(quaternion)),
            ("HamiltonHelper", HamiltonHelper(quaternion)),
            ("ArithmeticOperators", ArithmeticOperators(quaternion)),
            ("ConjugateExpression", QuaternionSimdExpression.Conjugate(quaternion, "value")),
            ("NormalizeExpression", NormalizeExpression(quaternion)),
            ("TransformVectorMember", TransformVectorMember(quaternion)),
            ("CrossScalarConversions", CrossScalarConversions(quaternion)),
            ("SystemNumericsConversions", SystemNumericsConversions(quaternion)),
            ("ImplementedInterfaces", ImplementedInterfaces(quaternion)));

    private static string ArithmeticOperators(QuaternionSpec quaternion) =>
        MathsTemplate.Fragment(
            "quat-operators-simd.csfrag.tmpl",
            ("TypeName", quaternion.TypeName),
            ("ScalarType", quaternion.Scalar.CSharpName),
            ("Vector3Type", quaternion.Vector3TypeName),
            ("RegisterType", QuaternionSimdExpression.RegisterType(quaternion)),
            ("RegisterApi", QuaternionSimdExpression.RegisterApi(quaternion)),
            ("PairAddExpression", QuaternionSimdExpression.Binary(quaternion, "left", "+", "right")),
            ("VectorScalarAddExpression", QuaternionSimdExpression.VectorScalar(quaternion, "left", "+", "right")),
            ("ScalarVectorAddExpression", QuaternionSimdExpression.ScalarVector(quaternion, "left", "+", "right")),
            ("PairSubtractExpression", QuaternionSimdExpression.Binary(quaternion, "left", "-", "right")),
            ("VectorScalarSubtractExpression", QuaternionSimdExpression.VectorScalar(quaternion, "left", "-", "right")),
            ("ScalarVectorSubtractExpression", QuaternionSimdExpression.ScalarVector(quaternion, "left", "-", "right")),
            ("NegateExpression", QuaternionSimdExpression.Unary(quaternion, "-", "value")),
            ("HamiltonExpression", HamiltonExpression(quaternion)),
            ("ScaleExpression", QuaternionSimdExpression.VectorScalar(quaternion, "left", "*", "right")),
            ("DivideScalarExpression", QuaternionSimdExpression.VectorScalar(quaternion, "left", "/", "right")),
            ("ScalarDivideExpression", QuaternionSimdExpression.ScalarVector(quaternion, "left", "/", "right")));

    private static string HamiltonExpression(QuaternionSpec quaternion) =>
        MathsTemplate.Fragment(
            quaternion.Scalar.Kind == ScalarKind.Float
                ? "quat-hamilton-system.csfrag.tmpl"
                : "quat-hamilton-packed-double.csfrag.tmpl",
            ("TypeName", quaternion.TypeName));

    private static string HamiltonHelper(QuaternionSpec quaternion) =>
        quaternion.Scalar.Kind == ScalarKind.Double
            ? MathsTemplate.Fragment("quat-hamilton-packed-double-helper.csfrag.tmpl", ("TypeName", quaternion.TypeName))
            : string.Empty;

    private static string NormalizeExpression(QuaternionSpec quaternion) =>
        quaternion.Scalar.Kind == ScalarKind.Float
            ? "FromPacked(System.Numerics.Quaternion.Normalize(value.packed))"
            : "value / value.Length";

    private static string TransformVectorMember(QuaternionSpec quaternion) =>
        MathsTemplate.Fragment(
            quaternion.Scalar.Kind == ScalarKind.Float
                ? "quat-transform-vector.csfrag.tmpl"
                : "quat-transform-vector-scalar.csfrag.tmpl",
            ("TypeName", quaternion.TypeName),
            ("ScalarType", quaternion.Scalar.CSharpName),
            ("Vector3Type", quaternion.Vector3TypeName),
            ("RegisterType", QuaternionSimdExpression.RegisterType(quaternion)),
            ("RegisterApi", QuaternionSimdExpression.RegisterApi(quaternion)),
            ("ZeroLiteral", quaternion.Scalar.ZeroLiteral),
            ("TwoLiteral", quaternion.Scalar.TwoLiteral));

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
        $"{Capitalized(quaternion.Scalar.Description)} quaternion for 3D rotations and orientation interpolation.";

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

    private static string FieldOffset(QuaternionSpec quaternion, int component) =>
        quaternion.Scalar.Kind == ScalarKind.Float
            ? $"[FieldOffset({(component * quaternion.Scalar.SizeBytes).ToString(CultureInfo.InvariantCulture)})]{Environment.NewLine}    "
            : string.Empty;

    private static string PackedStorage(QuaternionSpec quaternion) =>
        quaternion.Scalar.Kind == ScalarKind.Float
            ? MathsTemplate.Fragment("quat-packed-system-storage.csfrag.tmpl")
            : string.Empty;

    private static string HalfLiteral(ScalarSpec scalar) => scalar.Kind == ScalarKind.Float ? "0.5f" : "0.5d";

    private static string QuarterLiteral(ScalarSpec scalar) => scalar.Kind == ScalarKind.Float ? "0.25f" : "0.25d";

    private static string FourLiteral(ScalarSpec scalar) => scalar.Kind == ScalarKind.Float ? "4f" : "4d";

    private static string MinusOneLiteral(ScalarSpec scalar) => scalar.Kind == ScalarKind.Float ? "-1f" : "-1d";

    private static string ToleranceLiteral(ScalarSpec scalar) => scalar.Kind == ScalarKind.Float ? "1e-6f" : "1e-12d";

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

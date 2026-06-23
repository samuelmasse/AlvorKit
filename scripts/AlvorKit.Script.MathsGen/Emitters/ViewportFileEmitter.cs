namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated viewport source file.</summary>
internal static class ViewportFileEmitter
{
    /// <summary>Returns source code for <paramref name="viewport"/>.</summary>
    public static string Emit(ViewportSpec viewport) =>
        MathsTemplate.Render(
            "viewport-file.cs.tmpl",
            ("TypeSummary", TypeSummary(viewport)),
            ("TypeName", viewport.TypeName),
            ("ScalarType", viewport.Scalar.CSharpName),
            ("Box2Type", viewport.Box2TypeName),
            ("IntervalType", viewport.IntervalTypeName),
            ("Vector2Type", viewport.Vector2TypeName),
            ("Vector3Type", viewport.Vector3TypeName),
            ("Vector4Type", viewport.Vector4TypeName),
            ("Matrix4Type", viewport.Matrix4TypeName),
            ("Ray3Type", viewport.Ray3TypeName),
            ("SizeBytes", viewport.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", viewport.Scalar.ZeroLiteral),
            ("OneLiteral", viewport.Scalar.OneLiteral),
            ("TwoLiteral", viewport.Scalar.TwoLiteral),
            ("CrossScalarConversions", CrossScalarConversions(viewport)),
            ("ImplementedInterfaces", ImplementedInterfaces(viewport)));

    private static string ImplementedInterfaces(ViewportSpec viewport) =>
        $"IEquatable<{viewport.TypeName}>, IComparable<{viewport.TypeName}>, " +
        $"IEqualityOperators<{viewport.TypeName}, {viewport.TypeName}, bool>, ISpanFormattable, IUtf8SpanFormattable, " +
        $"ISpanParsable<{viewport.TypeName}>, IUtf8SpanParsable<{viewport.TypeName}>";

    private static string TypeSummary(ViewportSpec viewport) =>
        $"{Capitalized(viewport.Scalar.Description)} viewport for projection, unprojection, and picking.";

    private static string CrossScalarConversions(ViewportSpec viewport)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in ViewportCatalog.Scalars)
        {
            if (targetScalar == viewport.Scalar)
                continue;

            var target = new ViewportSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(viewport.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "viewport-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("KeywordCapitalized", Capitalized(keyword)),
                ("TargetType", target.TypeName),
                ("SourceType", viewport.TypeName),
                ("TargetBox2Type", target.Box2TypeName),
                ("TargetIntervalType", target.IntervalTypeName)));
        }

        return builder.ToString();
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

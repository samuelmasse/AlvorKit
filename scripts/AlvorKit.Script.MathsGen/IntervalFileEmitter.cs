namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated scalar interval source file.</summary>
internal static class IntervalFileEmitter
{
    /// <summary>Returns source code for <paramref name="interval"/>.</summary>
    public static string Emit(IntervalSpec interval) =>
        MathsTemplate.Render(
            "interval-file.cs.tmpl",
            ("TypeSummary", TypeSummary(interval)),
            ("TypeName", interval.TypeName),
            ("ScalarType", interval.Scalar.CSharpName),
            ("SizeBytes", interval.SizeBytes.ToString(CultureInfo.InvariantCulture)),
            ("ZeroLiteral", interval.Scalar.ZeroLiteral),
            ("TwoLiteral", interval.Scalar.TwoLiteral),
            ("PositiveInfinity", $"{interval.Scalar.CSharpName}.PositiveInfinity"),
            ("NegativeInfinity", $"{interval.Scalar.CSharpName}.NegativeInfinity"),
            ("CrossScalarConversions", CrossScalarConversions(interval)),
            ("ImplementedInterfaces", $"IInterval<{interval.TypeName}, {interval.Scalar.CSharpName}>"));

    private static string TypeSummary(IntervalSpec interval) =>
        $"{Capitalized(interval.Scalar.Description)} inclusive scalar interval.";

    private static string CrossScalarConversions(IntervalSpec interval)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in IntervalCatalog.Scalars)
        {
            if (targetScalar == interval.Scalar)
                continue;

            var target = new IntervalSpec(targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(interval.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment(
                "interval-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("KeywordCapitalized", Capitalized(keyword)),
                ("TargetType", target.TypeName),
                ("SourceType", interval.TypeName),
                ("TargetScalarType", targetScalar.CSharpName)));
        }

        return builder.ToString();
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated axis-aligned box source file.</summary>
internal static class BoxFileEmitter
{
    /// <summary>Returns source code for <paramref name="box"/>.</summary>
    public static string Emit(BoxSpec box) =>
        MathsTemplate.Render(
            "box-file.cs.tmpl",
            ("TypeSummary", TypeSummary(box)),
            ("TypeName", box.TypeName),
            ("ScalarType", box.Scalar.CSharpName),
            ("VectorType", box.VectorTypeName),
            ("Dimension", box.Dimension.ToString(CultureInfo.InvariantCulture)),
            ("ComponentCount", (box.Dimension * 2).ToString(CultureInfo.InvariantCulture)),
            ("TwoLiteral", box.Scalar.TwoLiteral),
            ("ExtremeMinVector", ExtremeVector(box, high: true)),
            ("ExtremeMaxVector", ExtremeVector(box, high: false)),
            ("ShapeProperties", ShapeProperties(box)),
            ("AnyMinGreaterMax", ComponentJoin(box, component => $"Min.{component} > Max.{component}", " || ")),
            ("ContainsInclusive", PointComparison(box, ">=", "<=")),
            ("ContainsHalfOpen", PointComparison(box, ">=", "<")),
            ("ContainsExclusive", PointComparison(box, ">", "<")),
            ("ContainsOtherHalfOpen", OtherComparison(box, ">=", "<=")),
            ("ContainsOtherExclusive", OtherComparison(box, ">", "<")),
            ("IntersectsInclusive", IntersectsComparison(box, "<=")),
            ("IntersectsExclusive", IntersectsComparison(box, "<")),
            ("DistanceType", box.DistanceTypeName),
            ("SphereRelationships", SphereRelationships(box)),
            ("CrossScalarConversions", CrossScalarConversions(box)),
            ("ImplementedInterfaces", ImplementedInterfaces(box)));

    private static string TypeSummary(BoxSpec box) =>
        $"Axis-aligned {box.Dimension}D {box.Scalar.Description} bounding box for spatial queries.";

    private static string ExtremeVector(BoxSpec box, bool high)
    {
        var scalar = box.Scalar.Kind switch
        {
            ScalarKind.Float => high ? "float.PositiveInfinity" : "float.NegativeInfinity",
            ScalarKind.Double => high ? "double.PositiveInfinity" : "double.NegativeInfinity",
            _ => high ? "int.MaxValue" : "int.MinValue",
        };
        return $"new {box.VectorTypeName}({scalar})";
    }

    private static string ShapeProperties(BoxSpec box) =>
        box.Dimension == 2
            ? MathsTemplate.Fragment("box2-properties.csfrag.tmpl", ("ScalarType", box.Scalar.CSharpName))
            : MathsTemplate.Fragment("box3-properties.csfrag.tmpl", ("ScalarType", box.Scalar.CSharpName));

    private static string CrossScalarConversions(BoxSpec box)
    {
        var builder = new StringBuilder();
        foreach (var targetScalar in BoxCatalog.Scalars)
        {
            if (targetScalar == box.Scalar)
                continue;

            var target = new BoxSpec(box.Dimension, targetScalar);
            var keyword = VectorCatalog.IsImplicitConversion(box.Scalar, targetScalar) ? "implicit" : "explicit";
            builder.Append(MathsTemplate.Fragment("box-conversion.csfrag.tmpl",
                ("Keyword", keyword),
                ("TargetType", target.TypeName),
                ("SourceType", box.TypeName),
                ("TargetVectorType", target.VectorTypeName)));
        }

        return builder.ToString();
    }

    private static string SphereRelationships(BoxSpec box) =>
        box.SupportsSphereRelationships
            ? MathsTemplate.Fragment(
                "box3-sphere-relationships.csfrag.tmpl",
                ("TypeName", box.TypeName),
                ("VectorType", box.VectorTypeName),
                ("SphereType", box.SphereTypeName))
            : "";

    private static string ImplementedInterfaces(BoxSpec box) =>
        box.SupportsSphereRelationships
            ? $"IBox3Sphere<{box.TypeName}, {box.Scalar.CSharpName}, {box.VectorTypeName}, {box.SphereTypeName}>"
            : $"IBox{box.Dimension}<{box.TypeName}, {box.Scalar.CSharpName}, {box.VectorTypeName}>";

    private static string PointComparison(BoxSpec box, string lowerOperator, string upperOperator) =>
        ComponentJoin(box, component => $"point.{component} {lowerOperator} Min.{component} && point.{component} {upperOperator} Max.{component}", " && ");

    private static string OtherComparison(BoxSpec box, string lowerOperator, string upperOperator) =>
        ComponentJoin(box, component => $"other.Min.{component} {lowerOperator} Min.{component} && other.Max.{component} {upperOperator} Max.{component}", " && ");

    private static string IntersectsComparison(BoxSpec box, string op) =>
        ComponentJoin(box, component => $"Min.{component} {op} other.Max.{component} && other.Min.{component} {op} Max.{component}", " && ");

    private static string ComponentJoin(BoxSpec box, Func<string, string> selector, string separator) =>
        string.Join(separator, box.Components.Select(selector));
}

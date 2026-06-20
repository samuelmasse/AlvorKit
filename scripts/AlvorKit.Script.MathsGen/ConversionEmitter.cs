namespace AlvorKit.Script.MathsGen;

/// <summary>Emits tuple and cross-scalar vector conversions.</summary>
internal static class ConversionEmitter
{
    /// <summary>Appends conversion operators for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        var tupleType = TupleType(vector);
        members.Append(MathsTemplate.Fragment("tuple-to-vector.csfrag.tmpl", ("Description", vector.Scalar.CSharpName),
            ("TypeName", vector.TypeName), ("TupleType", tupleType), ("Arguments", TupleArguments(vector))));
        members.Append(MathsTemplate.Fragment("vector-to-tuple.csfrag.tmpl", ("Description", vector.Scalar.CSharpName),
            ("TypeName", vector.TypeName), ("TupleType", tupleType), ("Arguments", ComponentArguments(vector, "value"))));
        EmitSystemNumericsConversions(vector, members);

        foreach (var source in VectorCatalog.Scalars.Where(source => source != vector.Scalar))
            EmitCrossConversion(vector, source, members);
        foreach (var source in HigherDimensionSources(vector))
            EmitDimensionConversion(vector, source, members);
    }

    /// <summary>Emits one conversion from a different scalar vector of the same dimension.</summary>
    private static void EmitCrossConversion(VectorSpec target, ScalarSpec source, MemberBlock members)
    {
        var sourceType = source.VectorName(target.Dimension);
        var conversion = VectorCatalog.IsImplicitConversion(source, target.Scalar) ? "implicit" : "explicit";
        var summary = $"{conversion.ToUpperInvariant()[0]}{conversion[1..]}ly converts a {sourceType} to a {target.TypeName}.";
        var args = target.Components.Select(component => CastComponent(target.Scalar, source, $"value.{component}"));
        members.Append(MathsTemplate.Fragment("conversion-expression.csfrag.tmpl", ("Summary", summary), ("ConversionKind", conversion),
            ("TypeName", target.TypeName), ("SourceType", sourceType), ("Expression", NumericFunctionsEmitter.NewRaw(args))));
    }

    /// <summary>Emits an explicit conversion from a higher-dimension source vector.</summary>
    private static void EmitDimensionConversion(VectorSpec target, VectorSpec source, MemberBlock members)
    {
        var summary = $"Explicitly converts a {source.TypeName} to a {target.TypeName} by taking the first {target.Dimension} components.";
        var args = target.Components.Select(component => CastComponent(target.Scalar, source.Scalar, $"value.{component}"));
        members.Append(MathsTemplate.Fragment("conversion-expression.csfrag.tmpl", ("Summary", summary), ("ConversionKind", "explicit"),
            ("TypeName", target.TypeName), ("SourceType", source.TypeName), ("Expression", NumericFunctionsEmitter.NewRaw(args))));
    }

    /// <summary>Emits implicit conversions for the matching System.Numerics float vector.</summary>
    private static void EmitSystemNumericsConversions(VectorSpec vector, MemberBlock members)
    {
        if (vector.Scalar.Kind != ScalarKind.Float)
            return;

        var systemType = $"System.Numerics.Vector{vector.Dimension.ToString(CultureInfo.InvariantCulture)}";
        members.Append(MathsTemplate.Fragment("conversion-expression.csfrag.tmpl",
            ("Summary", $"Implicitly converts a {systemType} to a {vector.TypeName}."),
            ("ConversionKind", "implicit"),
            ("TypeName", vector.TypeName),
            ("SourceType", systemType),
            ("Expression", $"new({ComponentArguments(vector, "value")})")));
        members.Append(MathsTemplate.Fragment("conversion-expression.csfrag.tmpl",
            ("Summary", $"Implicitly converts a {vector.TypeName} to a {systemType}."),
            ("ConversionKind", "implicit"),
            ("TypeName", systemType),
            ("SourceType", vector.TypeName),
            ("Expression", $"new({ComponentArguments(vector, "value")})")));
    }

    /// <summary>Returns the generated tuple type for one vector.</summary>
    private static string TupleType(VectorSpec vector)
    {
        var entries = vector.Components.Select(component => $"{vector.Scalar.CSharpName} {component}");
        return $"({string.Join(", ", entries)})";
    }

    /// <summary>Returns tuple-to-vector constructor arguments.</summary>
    private static string TupleArguments(VectorSpec vector) =>
        string.Join(", ", vector.Components.Select(component => $"value.{component}"));

    /// <summary>Returns vector-to-tuple arguments.</summary>
    private static string ComponentArguments(VectorSpec vector, string valueName) =>
        string.Join(", ", vector.Components.Select(component => $"{valueName}.{component}"));

    /// <summary>Returns a component conversion expression.</summary>
    internal static string CastComponent(ScalarSpec target, ScalarSpec source, string expression)
    {
        if (target.IsBool)
            return source.IsBool ? expression : $"{expression} != {source.ZeroLiteral}";
        if (source.IsBool)
            return $"{expression} ? {target.OneLiteral} : {target.ZeroLiteral}";

        return VectorCatalog.IsImplicitConversion(source, target) ? expression : $"({target.CSharpName}){expression}";
    }

    /// <summary>Returns all generated vectors with more components than <paramref name="target"/>.</summary>
    private static IEnumerable<VectorSpec> HigherDimensionSources(VectorSpec target) =>
        VectorCatalog.Vectors.Where(source => source.Dimension > target.Dimension);
}

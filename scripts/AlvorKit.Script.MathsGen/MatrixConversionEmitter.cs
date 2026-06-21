namespace AlvorKit.Script.MathsGen;

/// <summary>Emits cross-scalar matrix conversions.</summary>
internal static class MatrixConversionEmitter
{
    /// <summary>Appends conversion operators for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        foreach (var source in MatrixCatalog.Scalars.Where(source => source != matrix.Scalar))
            EmitCrossConversion(matrix, source, members);
    }

    private static void EmitCrossConversion(MatrixSpec target, ScalarSpec source, MemberBlock members)
    {
        var sourceMatrix = target with { Scalar = source };
        var conversion = VectorCatalog.IsImplicitConversion(source, target.Scalar) ? "implicit" : "explicit";
        var summary = $"{Capitalized(conversion)}ly converts a {sourceMatrix.TypeName} to a {target.TypeName}.";
        members.Append(MathsTemplate.Fragment("conversion-expression.csfrag.tmpl",
            ("Summary", summary),
            ("ConversionKind", conversion),
            ("TypeName", target.TypeName),
            ("SourceType", sourceMatrix.TypeName),
            ("Expression", MatrixExpression.New(target, (column, row) =>
                ConversionEmitter.CastComponent(target.Scalar, source, $"value[{column}, {row}]")))));
    }

    private static string Capitalized(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}

namespace AlvorKit.Script.MathsGen;

/// <summary>Emits equality, hashing, and text formatting for generated vectors.</summary>
internal static class ValueSemanticsEmitter
{
    /// <summary>Appends value semantics for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members) =>
        members.Append(MathsTemplate.Fragment("value-semantics.csfrag.tmpl",
            ("TypeName", vector.TypeName),
            ("ScalarType", vector.Scalar.CSharpName),
            ("TryFormatComponents", ValueSemanticsSequenceEmitter.TryFormatComponents(vector)),
            ("TryFormatUtf8Components", ValueSemanticsSequenceEmitter.TryFormatUtf8Components(vector)),
            ("CompareComponents", ValueSemanticsSequenceEmitter.CompareComponents(vector)),
            ("ParseComponents", ValueSemanticsSequenceEmitter.ParseComponents(vector)),
            ("ParseUtf8Components", ValueSemanticsSequenceEmitter.ParseUtf8Components(vector)),
            ("ParsedComponentArguments", string.Join(", ", vector.Parameters)),
            ("EqualityExpression", EqualityExpression(vector)),
            ("HashComponents", string.Join(", ", vector.Components)),
            ("EqualScalarExpression", ValueSemanticsScalarEmitter.EqualScalarExpression(vector))));

    /// <summary>Returns the exact equality expression selected for this scalar representation.</summary>
    private static string EqualityExpression(VectorSpec vector) =>
        vector.Scalar.Kind == ScalarKind.Float
            ? "packed.Equals(other.packed)"
            : string.Join(" && ", vector.Components.Select(component => $"EqualScalar({component}, other.{component})"));

}

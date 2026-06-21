namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated vector source file.</summary>
internal static class VectorFileEmitter
{
    /// <summary>Returns source code for <paramref name="vector"/>.</summary>
    public static string Emit(VectorSpec vector)
    {
        var members = new MemberBlock();
        CoreEmitter.Emit(vector, members);
        SpanInteropEmitter.Emit(vector, members);
        CompositionConstructorEmitter.Emit(vector, members);
        ConversionEmitter.Emit(vector, members);
        OperatorEmitter.Emit(vector, members);
        FunctionEmitter.Emit(vector, members);

        return MathsTemplate.Render(
            "vector-file.cs.tmpl",
            ("TypeSummary", TypeSummary(vector)),
            ("ParameterDocs", ParameterDocs(vector)),
            ("TypeName", vector.TypeName),
            ("ConstructorParameters", string.Join(", ", vector.Parameters.Select(p => $"{vector.Scalar.CSharpName} {p}"))),
            ("ImplementedInterfaces", ImplementedInterfaces(vector)),
            ("Members", members.ToString()));
    }

    /// <summary>Returns generated interfaces implemented by one vector type.</summary>
    private static string ImplementedInterfaces(VectorSpec vector)
    {
        var interfaces = new List<string>();

        if (vector.Scalar.IsBool)
            AddMaskInterface(vector, interfaces);
        else
            AddNumericInterfaces(vector, interfaces);

        return string.Join($",{Environment.NewLine}      ", interfaces);
    }

    /// <summary>Adds numeric generic-math interfaces for one vector type.</summary>
    private static void AddNumericInterfaces(VectorSpec vector, List<string> interfaces)
    {
        var type = vector.TypeName;
        var scalar = vector.Scalar.CSharpName;
        var mask = vector.BoolTypeName;
        var length = NumericFunctionsEmitter.LengthType(vector);
        var arithmetic = ArithmeticType(vector);
        if (vector.Scalar.IsFloating)
            interfaces.Add($"IVec{vector.Dimension}Floating<{type}, {scalar}, {mask}>");
        else if (vector.Scalar.IsSigned)
            interfaces.Add($"IVec{vector.Dimension}SignedInteger<{type}, {scalar}, {mask}, {vector.IntTypeName}, {length}, {arithmetic}>");
        else
            interfaces.Add($"IVec{vector.Dimension}UnsignedInteger<{type}, {scalar}, {mask}, {vector.IntTypeName}, {length}, {arithmetic}>");

        interfaces.Add($"IVecScalarArithmeticOperators<{type}, {scalar}, {arithmetic}>");
        if (vector.Scalar.IsInteger)
            interfaces.Add($"IVecScalarIntegerOperators<{type}, {scalar}, {arithmetic}>");

        if (vector.Scalar.Kind == ScalarKind.Float)
            interfaces.Add($"IVec{vector.Dimension}SystemNumerics<{type}>");
    }

    /// <summary>Returns the vector type produced by same-scalar arithmetic operators.</summary>
    private static string ArithmeticType(VectorSpec vector)
    {
        var resultScalar = ScalarPromotion.BinaryNumericResult(vector.Scalar, vector.Scalar) ?? vector.Scalar;
        return (vector with { Scalar = resultScalar }).TypeName;
    }

    /// <summary>Adds the mask family interface for one Boolean vector type.</summary>
    private static void AddMaskInterface(VectorSpec vector, List<string> interfaces) =>
        interfaces.Add($"IVec{vector.Dimension}Mask<{vector.TypeName}>");

    /// <summary>Returns the XML documentation summary for one generated type.</summary>
    private static string TypeSummary(VectorSpec vector)
    {
        if (vector.Scalar.IsBool)
            return $"{vector.Dimension}-component Boolean vector used for component masks and comparison results.";

        return $"{vector.Dimension}-component {vector.Scalar.Description} vector for game math and graphics APIs.";
    }

    /// <summary>Returns XML documentation for primary constructor parameters.</summary>
    private static string ParameterDocs(VectorSpec vector)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < vector.Dimension; index++)
        {
            var parameter = vector.Parameters[index];
            var component = vector.Components[index];
            builder.Append("/// <param name=\"")
                .Append(parameter)
                .Append("\">The ")
                .Append(Ordinal(index))
                .Append(" component, commonly used as ")
                .Append(component)
                .Append(".</param>")
                .AppendLine();
        }

        return builder.ToString();
    }

    /// <summary>Returns a human-readable ordinal for a component index.</summary>
    private static string Ordinal(int index) => index switch
    {
        0 => "first",
        1 => "second",
        2 => "third",
        _ => "fourth",
    };
}

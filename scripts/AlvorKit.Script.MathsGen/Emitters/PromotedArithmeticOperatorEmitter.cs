namespace AlvorKit.Script.MathsGen;

/// <summary>Emits cross-scalar operators whose result follows C# numeric promotion.</summary>
internal static class PromotedArithmeticOperatorEmitter
{
    /// <summary>Appends promoted scalar and vector arithmetic operators for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        foreach (var op in new[] { "+", "-", "*", "/", "%" })
        {
            foreach (var scalar in VectorCatalog.Scalars.Where(scalar => scalar != vector.Scalar))
                EmitScalarPair(vector, scalar, op, members, ScalarPromotion.BinaryNumericResult);
            foreach (var right in VectorCatalog.Vectors.Where(right => right.Dimension == vector.Dimension && right.Scalar != vector.Scalar))
                EmitVectorPair(vector, right, op, members, ScalarPromotion.BinaryNumericResult);
        }

        if (!vector.Scalar.IsInteger)
            return;

        foreach (var op in new[] { "&", "|", "^" })
        {
            foreach (var scalar in VectorCatalog.Scalars.Where(scalar => scalar != vector.Scalar))
                EmitScalarPair(vector, scalar, op, members, ScalarPromotion.BinaryIntegerResult);
            foreach (var right in VectorCatalog.Vectors.Where(right => right.Dimension == vector.Dimension && right.Scalar != vector.Scalar))
                EmitVectorPair(vector, right, op, members, ScalarPromotion.BinaryIntegerResult);
        }
    }

    private static void EmitScalarPair(
        VectorSpec vector,
        ScalarSpec scalar,
        string op,
        MemberBlock members,
        Func<ScalarSpec, ScalarSpec, ScalarSpec?> result)
    {
        var resultScalar = result(vector.Scalar, scalar);
        if (resultScalar is null || ScalarPromotion.ExistingScalarOperatorCovers(vector.Scalar, scalar))
            return;

        var target = vector with { Scalar = resultScalar };
        var description = OperatorDescription(op);
        members.Append(Operator(
            $"Applies C#-promoted {description} between each component and a {scalar.Description} scalar.",
            target.TypeName,
            op,
            $"{vector.TypeName} left, {scalar.CSharpName} right",
            New(target, component => Cast(target, $"left.{component}") + $" {op} " + Cast(target, "right"))));
        members.Append(Operator(
            $"Applies C#-promoted {description} between a {scalar.Description} scalar and each component.",
            target.TypeName,
            op,
            $"{scalar.CSharpName} left, {vector.TypeName} right",
            New(target, component => Cast(target, "left") + $" {op} " + Cast(target, $"right.{component}"))));
    }

    private static void EmitVectorPair(
        VectorSpec left,
        VectorSpec right,
        string op,
        MemberBlock members,
        Func<ScalarSpec, ScalarSpec, ScalarSpec?> result)
    {
        var resultScalar = result(left.Scalar, right.Scalar);
        if (resultScalar is null || ScalarPromotion.ExistingVectorOperatorCovers(left.Scalar, right.Scalar, resultScalar))
            return;

        // Integer pairs with no implicit conversion in either direction (signed vs
        // unsigned of the same or smaller size) make tuple literals ambiguous: a
        // non-negative constant tuple converts to both vector types and overload
        // resolution has no betterness between them, so no operator is generated.
        if (left.Scalar.IsInteger && right.Scalar.IsInteger &&
            !VectorCatalog.IsImplicitConversion(left.Scalar, right.Scalar) &&
            !VectorCatalog.IsImplicitConversion(right.Scalar, left.Scalar))
            return;

        var target = left with { Scalar = resultScalar };
        members.Append(Operator(
            $"Applies C#-promoted {OperatorDescription(op)} component by component.",
            target.TypeName,
            op,
            $"{left.TypeName} left, {right.TypeName} right",
            New(target, component => Cast(target, $"left.{component}") + $" {op} " + Cast(target, $"right.{component}"))));
    }

    private static string Cast(VectorSpec target, string expression) => $"({target.Scalar.CSharpName}){expression}";

    private static string Operator(string summary, string returnType, string op, string parameters, string expression) =>
        MathsTemplate.Fragment("operator-expression.csfrag.tmpl", ("Summary", summary), ("ReturnType", returnType),
            ("Operator", op), ("Parameters", parameters), ("Expression", expression));

    private static string OperatorDescription(string op) => op switch
    {
        "+" => "addition",
        "-" => "subtraction",
        "*" => "multiplication",
        "/" => "division",
        "%" => "remainder",
        "&" => "AND",
        "|" => "OR",
        "^" => "XOR",
        _ => op,
    };

    private static string New(VectorSpec vector, Func<string, string> expression) =>
        NumericFunctionsEmitter.New(vector, expression);
}

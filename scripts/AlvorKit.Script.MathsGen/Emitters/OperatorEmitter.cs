namespace AlvorKit.Script.MathsGen;

/// <summary>Emits arithmetic, bitwise, logical, and equality operators.</summary>
internal static class OperatorEmitter
{
    /// <summary>Appends operators for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        if (vector.Scalar.IsBool)
        {
            EmitBool(vector, members);
            return;
        }

        EmitNumeric(vector, members);
    }

    /// <summary>Emits numeric operators.</summary>
    private static void EmitNumeric(VectorSpec vector, MemberBlock members)
    {
        EmitUnary(vector, members, "+", "Returns the C#-promoted value.", ScalarPromotion.UnaryPlusResult, component => $"+value.{component}");
        EmitUnary(vector, members, "-", "Negates each component.", ScalarPromotion.UnaryNegationResult, component => $"-value.{component}");

        members.Append(Operator("Adds one to each component.", vector.TypeName, "++", $"{vector.TypeName} value",
            New(vector, component => $"value.{component} + {vector.Scalar.OneLiteral}")));
        members.Append(Operator("Subtracts one from each component.", vector.TypeName, "--", $"{vector.TypeName} value",
            New(vector, component => $"value.{component} - {vector.Scalar.OneLiteral}")));
        foreach (var op in new[] { "+", "-", "*", "/", "%" })
            EmitPromotedNumericPair(vector, members, op);
        PromotedArithmeticOperatorEmitter.Emit(vector, members);

        if (vector.Scalar.IsInteger)
        {
            EmitUnary(vector, members, "~", "Bitwise-complements each component.", ScalarPromotion.UnaryComplementResult,
                component => $"~value.{component}");
            foreach (var op in new[] { "&", "|", "^" })
                EmitPromotedIntegerPair(vector, members, op);
            EmitShifts(vector, members);
        }

        EmitRelationalOperators(vector, members);
        EmitEquality(vector, members, "vector");
    }

    /// <summary>Emits bool mask operators.</summary>
    private static void EmitBool(VectorSpec vector, MemberBlock members)
    {
        members.Append(Operator("Negates each component.", vector.TypeName, "!", $"{vector.TypeName} value",
            New(vector, component => $"!value.{component}")));
        members.Append(Operator("Bitwise-complements each component.", vector.TypeName, "~", $"{vector.TypeName} value",
            New(vector, component => $"!value.{component}")));
        foreach (var op in new[] { "&", "|", "^" })
            EmitArithmeticPair(vector, members, op);

        members.Append(Operator("Returns whether every component is true.", "bool", "true", $"{vector.TypeName} value", "value.All"));
        var falseOperator = Operator("Returns whether every component is false.", "bool", "false", $"{vector.TypeName} value",
            vector.Dimension == 3
                ? string.Join(" && ", vector.Components.Select(component => $"!value.{component}"))
                : "value.None");
        members.Append(vector.Dimension == 3 ? Inline(falseOperator) : falseOperator);
        EmitEquality(vector, members, "mask");
    }

    /// <summary>Emits scalar and vector overloads for one binary operator.</summary>
    private static void EmitPromotedNumericPair(VectorSpec vector, MemberBlock members, string op)
    {
        var resultScalar = ScalarPromotion.BinaryNumericResult(vector.Scalar, vector.Scalar) ?? vector.Scalar;
        EmitPromotedPair(vector, members, op, resultScalar, $"C#-promoted {OperatorDescription(op)}");
    }

    /// <summary>Emits scalar and vector overloads for one integer binary operator.</summary>
    private static void EmitPromotedIntegerPair(VectorSpec vector, MemberBlock members, string op)
    {
        var resultScalar = ScalarPromotion.BinaryIntegerResult(vector.Scalar, vector.Scalar);
        if (resultScalar is not null)
            EmitPromotedPair(vector, members, op, resultScalar, $"C#-promoted {OperatorDescription(op)}");
    }

    /// <summary>Emits scalar and vector overloads with a promoted result.</summary>
    private static void EmitPromotedPair(VectorSpec vector, MemberBlock members, string op, ScalarSpec resultScalar, string description)
    {
        var target = vector with { Scalar = resultScalar };
        var scalar = vector.Scalar.CSharpName;
        var vectorScalarSummary = $"Applies {description} between each component and a scalar.";
        members.Append(VectorScalarOperator(vectorScalarSummary, vector, target, op, scalar));
        var scalarVectorSummary = $"Applies {description} between a scalar and each component.";
        members.Append(ScalarVectorOperator(scalarVectorSummary, vector, target, op, scalar));
        var vectorPairSummary = $"Applies {description} component by component.";
        members.Append(VectorPairOperator(vectorPairSummary, vector, target, op));
    }

    /// <summary>Renders a scalar-vector operator, using System.Numerics SIMD for supported single-precision arithmetic.</summary>
    private static string ScalarVectorOperator(string summary, VectorSpec vector, VectorSpec target, string op, string scalar)
    {
        var scalarExpression = New(target, component => Cast(target, "left") + $" {op} " + Cast(target, $"right.{component}"));
        if (op == "*" && vector.Scalar.Kind == ScalarKind.Float)
            return Operator(summary, target.TypeName, op, $"{scalar} left, {vector.TypeName} right", "new(left * right.packed)");

        return Operator(summary, target.TypeName, op, $"{scalar} left, {vector.TypeName} right", scalarExpression);
    }

    /// <summary>Renders a vector-scalar operator, using measured packed paths for supported floating-point arithmetic.</summary>
    private static string VectorScalarOperator(string summary, VectorSpec vector, VectorSpec target, string op, string scalar)
    {
        var scalarExpression = New(target, component => Cast(target, $"left.{component}") + $" {op} " + Cast(target, "right"));
        if ((op is "*" or "/") && vector.Scalar.Kind == ScalarKind.Float)
            return Operator(summary, target.TypeName, op, $"{vector.TypeName} left, {scalar} right", $"new(left.packed {op} right)");

        if (DoubleVectorExpression.SupportsVectorScalar(vector, op))
            return Operator(summary, target.TypeName, op, $"{vector.TypeName} left, {scalar} right",
                DoubleVectorExpression.VectorScalar(vector, "left", op, "right"));

        return Operator(summary, target.TypeName, op, $"{vector.TypeName} left, {scalar} right", scalarExpression);
    }

    /// <summary>Renders a vector-pair operator, using measured packed paths for supported arithmetic.</summary>
    private static string VectorPairOperator(string summary, VectorSpec vector, VectorSpec target, string op)
    {
        var scalarExpression = New(target, component => Cast(target, $"left.{component}") + $" {op} " + Cast(target, $"right.{component}"));
        if ((op is "+" or "-" or "*" or "/") && vector.Scalar.Kind == ScalarKind.Float)
            return Operator(summary, target.TypeName, op, $"{vector.TypeName} left, {vector.TypeName} right",
                $"new(left.packed {op} right.packed)");

        if (DoubleVectorExpression.SupportsBinary(vector, op))
            return Operator(summary, target.TypeName, op, $"{vector.TypeName} left, {vector.TypeName} right",
                DoubleVectorExpression.Binary(vector, "left", op, "right"));

        if ((op is "+" or "-" or "*" or "&" or "|" or "^") && Int32Vector128Expression.Supports(vector))
            return Operator(summary, target.TypeName, op, $"{vector.TypeName} left, {vector.TypeName} right",
                Int32Vector128Expression.Binary(vector, "left", op, "right"));

        if ((op is "&" or "|" or "^") && Int32Vector64Expression.Supports(vector))
            return Operator(summary, target.TypeName, op, $"{vector.TypeName} left, {vector.TypeName} right",
                Int32Vector64Expression.Binary(vector, "left", op, "right"));

        if ((op is "+" or "-" or "*" or "&" or "|" or "^") && Int64VectorExpression.Supports(vector))
            return Operator(summary, target.TypeName, op, $"{vector.TypeName} left, {vector.TypeName} right",
                Int64VectorExpression.Binary(vector, "left", op, "right"));

        return Operator(summary, target.TypeName, op, $"{vector.TypeName} left, {vector.TypeName} right", scalarExpression);
    }

    /// <summary>Emits a C#-promoted unary operator.</summary>
    private static void EmitUnary(
        VectorSpec vector,
        MemberBlock members,
        string op,
        string summary,
        Func<ScalarSpec, ScalarSpec?> result,
        Func<string, string> expression)
    {
        var resultScalar = result(vector.Scalar);
        if (resultScalar is null)
            return;

        var target = vector with { Scalar = resultScalar };
        var resultExpression = New(target, expression);
        if (op == "-" && vector.Scalar.Kind == ScalarKind.Float)
            resultExpression = "new(-value.packed)";
        else if (DoubleVectorExpression.SupportsUnary(vector, op))
        {
            resultExpression = DoubleVectorExpression.Unary(vector, op, "value");
        }
        else if (op == "-" && vector.Scalar.Kind == ScalarKind.Int && Int32Vector128Expression.Supports(vector))
        {
            resultExpression = Int32Vector128Expression.Unary(vector, op, "value");
        }
        else if (op == "-" && vector.Scalar.Kind == ScalarKind.Int64 && Int64VectorExpression.Supports(vector))
        {
            resultExpression = Int64VectorExpression.Unary(vector, op, "value");
        }
        else if (op == "~" && Int32Vector64Expression.Supports(vector))
        {
            resultExpression = Int32Vector64Expression.Unary(vector, op, "value");
        }
        else if (op == "~" && Int32Vector128Expression.Supports(vector))
        {
            resultExpression = Int32Vector128Expression.Unary(vector, op, "value");
        }
        else if (op == "~" && Int64VectorExpression.Supports(vector))
        {
            resultExpression = Int64VectorExpression.Unary(vector, op, "value");
        }

        members.Append(Operator(summary, target.TypeName, op, $"{vector.TypeName} value", resultExpression));
    }

    /// <summary>Emits C#-promoted shift operators.</summary>
    private static void EmitShifts(VectorSpec vector, MemberBlock members)
    {
        var resultScalar = ScalarPromotion.ShiftResult(vector.Scalar);
        if (resultScalar is null)
            return;

        var target = vector with { Scalar = resultScalar };
        foreach (var (op, description) in new[]
        {
            ("<<", "left"),
            (">>", "right"),
            (">>>", "right without sign extension"),
        })
        {
            var scalarCountExpression = Int32Vector128Expression.Supports(vector)
                ? Int32Vector128Expression.Shift(vector, "left", op, "right")
                : Int64VectorExpression.Supports(vector)
                    ? Int64VectorExpression.Shift(vector, "left", op, "right")
                    : New(target, component => $"{Cast(target, $"left.{component}")} {op} right");
            members.Append(Operator($"Shifts each component {description} by a scalar bit count.", target.TypeName, op,
                $"{vector.TypeName} left, int right",
                scalarCountExpression));
            var vectorCountFallback = New(target, component => $"{Cast(target, $"left.{component}")} {op} right.{component}");
            var vectorCountExpression = Int32VariableShiftExpression.Supports(vector)
                ? Int32VariableShiftExpression.Shift(vector, op, vectorCountFallback)
                : vectorCountFallback;
            members.Append(Operator($"Shifts each component {description} by matching integer-vector bit counts.", target.TypeName, op,
                $"{vector.TypeName} left, {vector.IntTypeName} right", vectorCountExpression));
        }
    }

    /// <summary>Emits scalar and vector overloads for one binary operator.</summary>
    private static void EmitArithmeticPair(VectorSpec vector, MemberBlock members, string op)
    {
        var scalar = vector.Scalar.CSharpName;
        var description = OperatorDescription(op);
        members.Append(Operator($"Applies {description} between each component and a scalar.", vector.TypeName, op,
            $"{vector.TypeName} left, {scalar} right", New(vector, component => $"left.{component} {op} right")));
        members.Append(Operator($"Applies {description} between a scalar and each component.", vector.TypeName, op,
            $"{scalar} left, {vector.TypeName} right", New(vector, component => $"left {op} right.{component}")));
        members.Append(Operator($"Applies {description} component by component.", vector.TypeName, op,
            $"{vector.TypeName} left, {vector.TypeName} right", New(vector, component => $"left.{component} {op} right.{component}")));
    }

    /// <summary>Emits whole-vector equality operators.</summary>
    private static void EmitEquality(VectorSpec vector, MemberBlock members, string noun)
    {
        members.Append(Operator($"Returns whether two {noun}s have identical components.", "bool", "==",
            $"{vector.TypeName} left, {vector.TypeName} right", "left.Equals(right)"));
        members.Append(Operator($"Returns whether two {noun}s have any different component.", "bool", "!=",
            $"{vector.TypeName} left, {vector.TypeName} right", "!left.Equals(right)"));
    }

    /// <summary>Emits component-wise comparison operators that return Boolean masks.</summary>
    private static void EmitRelationalOperators(VectorSpec vector, MemberBlock members)
    {
        var boolVector = vector with { Scalar = VectorCatalog.Bool };
        foreach (var op in new[] { "<", "<=", ">", ">=" })
        {
            var resultScalar = ScalarPromotion.BinaryNumericResult(vector.Scalar, vector.Scalar);
            if (resultScalar is null)
                continue;

            members.Append(Operator($"Returns a mask containing component-wise {OperatorDescription(op)} results.", vector.BoolTypeName, op,
                $"{vector.TypeName} left, {vector.TypeName} right",
                NumericFunctionsEmitter.New(boolVector, component => $"left.{component} {op} right.{component}")));
            foreach (var scalar in VectorCatalog.Scalars.Where(scalar => !scalar.IsBool))
                EmitRelationalScalarPair(vector, members, op, scalar);
            foreach (var right in VectorCatalog.Vectors.Where(right => right.Dimension == vector.Dimension && right.Scalar != vector.Scalar))
                EmitRelationalVectorPair(vector, members, op, right);
        }
    }

    /// <summary>Emits a vector-scalar comparison operator if the same-scalar overload does not already cover it.</summary>
    private static void EmitRelationalScalarPair(VectorSpec vector, MemberBlock members, string op, ScalarSpec scalar)
    {
        var resultScalar = ScalarPromotion.BinaryNumericResult(vector.Scalar, scalar);
        if (resultScalar is null || (scalar != vector.Scalar && ScalarPromotion.ExistingScalarOperatorCovers(vector.Scalar, scalar)))
            return;

        var boolVector = vector with { Scalar = VectorCatalog.Bool };
        var description = OperatorDescription(op);
        members.Append(Operator($"Returns a mask containing component-wise {description} results against a {scalar.Description} scalar.",
            vector.BoolTypeName, op, $"{vector.TypeName} left, {scalar.CSharpName} right",
            NumericFunctionsEmitter.New(boolVector, component => Compare(resultScalar, $"left.{component}", op, "right"))));
        members.Append(Operator($"Returns a mask containing component-wise {description} results from a {scalar.Description} scalar.",
            vector.BoolTypeName, op, $"{scalar.CSharpName} left, {vector.TypeName} right",
            NumericFunctionsEmitter.New(boolVector, component => Compare(resultScalar, "left", op, $"right.{component}"))));
    }

    /// <summary>Emits a cross-scalar vector comparison operator if implicit vector conversions do not already cover it.</summary>
    private static void EmitRelationalVectorPair(VectorSpec left, MemberBlock members, string op, VectorSpec right)
    {
        var resultScalar = ScalarPromotion.BinaryNumericResult(left.Scalar, right.Scalar);
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

        var boolVector = left with { Scalar = VectorCatalog.Bool };
        members.Append(Operator($"Returns a mask containing component-wise C#-promoted {OperatorDescription(op)} results.", left.BoolTypeName, op,
            $"{left.TypeName} left, {right.TypeName} right",
            NumericFunctionsEmitter.New(boolVector, component => Compare(resultScalar, $"left.{component}", op, $"right.{component}"))));
    }

    /// <summary>Renders an operator declaration.</summary>
    private static string Operator(string summary, string returnType, string op, string parameters, string expression) =>
        MathsTemplate.Fragment("operator-expression.csfrag.tmpl", ("Summary", summary), ("ReturnType", returnType),
            ("Operator", op), ("Parameters", parameters), ("Expression", expression));

    /// <summary>Marks a measured tiny operator for aggressive inlining.</summary>
    private static string Inline(string declaration)
    {
        var firstLineEnd = declaration.IndexOf(Environment.NewLine, StringComparison.Ordinal);
        return declaration.Insert(firstLineEnd + Environment.NewLine.Length,
            $"    [MethodImpl(MethodImplOptions.AggressiveInlining)]{Environment.NewLine}");
    }

    /// <summary>Returns XML-safe operator wording for documentation.</summary>
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
        "<" => "less-than",
        "<=" => "less-than-or-equal",
        ">" => "greater-than",
        ">=" => "greater-than-or-equal",
        _ => op,
    };

    /// <summary>Casts an operand expression to the promoted scalar type.</summary>
    private static string Cast(VectorSpec target, string expression) => $"({target.Scalar.CSharpName}){expression}";

    /// <summary>Returns a component comparison expression after applying the C# promoted scalar type.</summary>
    private static string Compare(ScalarSpec scalar, string left, string op, string right) =>
        $"({scalar.CSharpName}){left} {op} ({scalar.CSharpName}){right}";

    /// <summary>Returns a constructor expression with generated component expressions.</summary>
    private static string New(VectorSpec vector, Func<string, string> expression) =>
        NumericFunctionsEmitter.New(vector, expression);
}

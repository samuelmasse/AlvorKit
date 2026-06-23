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
        members.Append(Operator("Returns whether every component is false.", "bool", "false", $"{vector.TypeName} value", "value.None"));
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
        members.Append(Operator($"Applies {description} between each component and a scalar.", target.TypeName, op,
            $"{vector.TypeName} left, {scalar} right", New(target, component => Cast(target, $"left.{component}") + $" {op} " + Cast(target, "right"))));
        members.Append(Operator($"Applies {description} between a scalar and each component.", target.TypeName, op,
            $"{scalar} left, {vector.TypeName} right", New(target, component => Cast(target, "left") + $" {op} " + Cast(target, $"right.{component}"))));
        members.Append(Operator($"Applies {description} component by component.", target.TypeName, op,
            $"{vector.TypeName} left, {vector.TypeName} right",
            New(target, component => Cast(target, $"left.{component}") + $" {op} " + Cast(target, $"right.{component}"))));
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
        members.Append(Operator(summary, target.TypeName, op, $"{vector.TypeName} value", New(target, expression)));
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
            members.Append(Operator($"Shifts each component {description} by a scalar bit count.", target.TypeName, op,
                $"{vector.TypeName} left, int right",
                New(target, component => $"{Cast(target, $"left.{component}")} {op} right")));
            members.Append(Operator($"Shifts each component {description} by matching integer-vector bit counts.", target.TypeName, op,
                $"{vector.TypeName} left, {vector.IntTypeName} right",
                New(target, component => $"{Cast(target, $"left.{component}")} {op} right.{component}")));
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

        var boolVector = left with { Scalar = VectorCatalog.Bool };
        members.Append(Operator($"Returns a mask containing component-wise C#-promoted {OperatorDescription(op)} results.", left.BoolTypeName, op,
            $"{left.TypeName} left, {right.TypeName} right",
            NumericFunctionsEmitter.New(boolVector, component => Compare(resultScalar, $"left.{component}", op, $"right.{component}"))));
    }

    /// <summary>Renders an operator declaration.</summary>
    private static string Operator(string summary, string returnType, string op, string parameters, string expression) =>
        MathsTemplate.Fragment("operator-expression.csfrag.tmpl", ("Summary", summary), ("ReturnType", returnType),
            ("Operator", op), ("Parameters", parameters), ("Expression", expression));

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

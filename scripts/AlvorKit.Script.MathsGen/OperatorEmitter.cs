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
        members.Append(Operator("Returns value unchanged.", vector.TypeName, "+", $"{vector.TypeName} value", "value"));
        if (vector.Scalar.IsSigned)
            members.Append(Operator("Negates each component.", vector.TypeName, "-", $"{vector.TypeName} value", New(vector, component => $"-value.{component}")));

        members.Append(Operator("Adds one to each component.", vector.TypeName, "++", $"{vector.TypeName} value", $"value + {vector.Scalar.OneLiteral}"));
        members.Append(Operator("Subtracts one from each component.", vector.TypeName, "--", $"{vector.TypeName} value", $"value - {vector.Scalar.OneLiteral}"));
        foreach (var op in new[] { "+", "-", "*", "/" })
            EmitArithmeticPair(vector, members, op);

        if (vector.Scalar.IsInteger)
        {
            members.Append(Operator("Bitwise-complements each component.", vector.TypeName, "~", $"{vector.TypeName} value",
                New(vector, component => $"~value.{component}")));
            EmitArithmeticPair(vector, members, "%");
            foreach (var op in new[] { "&", "|", "^" })
                EmitArithmeticPair(vector, members, op);

            members.Append(Operator("Shifts each component left by a scalar bit count.", vector.TypeName, "<<",
                $"{vector.TypeName} left, int right", New(vector, component => $"left.{component} << right")));
            members.Append(Operator("Shifts each component right by a scalar bit count.", vector.TypeName, ">>",
                $"{vector.TypeName} left, int right", New(vector, component => $"left.{component} >> right")));
            members.Append(Operator("Shifts each component right without sign extension by a scalar bit count.", vector.TypeName, ">>>",
                $"{vector.TypeName} left, int right", New(vector, component => $"left.{component} >>> right")));
            members.Append(Operator("Shifts each component left by matching component bit counts.", vector.TypeName, "<<",
                $"{vector.TypeName} left, {vector.TypeName} right", New(vector, component => $"left.{component} << (int)right.{component}")));
            members.Append(Operator("Shifts each component right by matching component bit counts.", vector.TypeName, ">>",
                $"{vector.TypeName} left, {vector.TypeName} right", New(vector, component => $"left.{component} >> (int)right.{component}")));
            members.Append(Operator("Shifts each component right without sign extension by matching component bit counts.", vector.TypeName, ">>>",
                $"{vector.TypeName} left, {vector.TypeName} right", New(vector, component => $"left.{component} >>> (int)right.{component}")));
            if (vector.TypeName != vector.IntTypeName)
            {
                members.Append(Operator("Shifts each component left by matching integer-vector bit counts.", vector.TypeName, "<<",
                    $"{vector.TypeName} left, {vector.IntTypeName} right", New(vector, component => $"left.{component} << right.{component}")));
                members.Append(Operator("Shifts each component right by matching integer-vector bit counts.", vector.TypeName, ">>",
                    $"{vector.TypeName} left, {vector.IntTypeName} right", New(vector, component => $"left.{component} >> right.{component}")));
                members.Append(Operator(
                    "Shifts each component right without sign extension by matching integer-vector bit counts.",
                    vector.TypeName,
                    ">>>",
                    $"{vector.TypeName} left, {vector.IntTypeName} right",
                    New(vector, component => $"left.{component} >>> right.{component}")));
            }
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
            members.Append(Operator($"Returns a mask containing component-wise {OperatorDescription(op)} results.", vector.BoolTypeName, op,
                $"{vector.TypeName} left, {vector.TypeName} right",
                NumericFunctionsEmitter.New(boolVector, component => $"left.{component} {op} right.{component}")));
        }
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

    /// <summary>Returns a constructor expression with generated component expressions.</summary>
    private static string New(VectorSpec vector, Func<string, string> expression) =>
        NumericFunctionsEmitter.New(vector, expression);
}

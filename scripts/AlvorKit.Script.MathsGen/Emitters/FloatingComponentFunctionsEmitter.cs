namespace AlvorKit.Script.MathsGen;

/// <summary>Emits component-wise floating-point transcendental and root helpers.</summary>
internal static class FloatingComponentFunctionsEmitter
{
    /// <summary>Appends component-wise floating-point helpers for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        foreach (var method in new[] { "Sin", "Cos", "Tan", "Asin", "Acos", "Atan", "Exp", "Log", "Log2" })
            EmitUnary(vector, members, method);

        EmitSquareRoot(vector, members);
        EmitBinary(vector, members, "Atan2", "Returns component-wise two-argument arctangents.");
        EmitBinary(vector, members, "Pow", "Returns component-wise powers.");
        EmitVectorScalarPow(vector, members);
        var inverseSquareRoot = vector.Scalar.Kind == ScalarKind.Float
            ? $"new({FloatVectorExpression.SystemType(vector)}.One / " +
                $"{FloatVectorExpression.SystemType(vector)}.SquareRoot(value.packed))"
            : DoubleVectorExpression.Supports(vector)
                ? DoubleVectorExpression.InverseSqrt(vector, "value")
                : NumericFunctionsEmitter.New(vector, c => Call("InverseSqrt", $"value.{c}"));
        members.Append(NumericFunctionsEmitter.Method("Returns component-wise reciprocal square roots.", "static",
            vector.TypeName, "InverseSqrt", $"{vector.TypeName} value", inverseSquareRoot));
        EmitFusedMultiplyAdd(vector, members);
    }

    /// <summary>Emits one unary math helper.</summary>
    private static void EmitUnary(VectorSpec vector, MemberBlock members, string method) =>
        members.Append(NumericFunctionsEmitter.Method($"Returns component-wise {method}.", "static", vector.TypeName, method,
            $"{vector.TypeName} value", NumericFunctionsEmitter.New(vector, component => Call(method, $"value.{component}"))));

    /// <summary>Emits a component-wise square root, using measured packed paths for supported floating-point vectors.</summary>
    private static void EmitSquareRoot(VectorSpec vector, MemberBlock members)
    {
        var expression = vector.Scalar.Kind == ScalarKind.Float
            ? FloatVectorExpression.Function(vector, "SquareRoot", "value")
            : DoubleVectorExpression.Supports(vector)
                ? DoubleVectorExpression.Sqrt(vector, "value")
                : NumericFunctionsEmitter.New(vector, component => Call("Sqrt", $"value.{component}"));
        members.Append(NumericFunctionsEmitter.Method("Returns component-wise Sqrt.", "static", vector.TypeName, "Sqrt",
            $"{vector.TypeName} value", expression));
    }

    /// <summary>Emits one binary vector math helper.</summary>
    private static void EmitBinary(VectorSpec vector, MemberBlock members, string method, string summary) =>
        members.Append(NumericFunctionsEmitter.Method(summary, "static", vector.TypeName, method,
            $"{vector.TypeName} left, {vector.TypeName} right",
            NumericFunctionsEmitter.New(vector, component => Call(method, $"left.{component}", $"right.{component}"))));

    /// <summary>Emits a vector/scalar power overload.</summary>
    private static void EmitVectorScalarPow(VectorSpec vector, MemberBlock members) =>
        members.Append(NumericFunctionsEmitter.Method("Returns component-wise powers raised to a scalar exponent.", "static",
            vector.TypeName, "Pow", $"{vector.TypeName} value, {vector.Scalar.CSharpName} exponent",
            NumericFunctionsEmitter.New(vector, component => Call("Pow", $"value.{component}", "exponent"))));

    /// <summary>Emits component-wise fused multiply-add.</summary>
    private static void EmitFusedMultiplyAdd(VectorSpec vector, MemberBlock members)
    {
        var expression = vector.Scalar.Kind == ScalarKind.Float
            ? FloatVectorExpression.Function(vector, "FusedMultiplyAdd", "left", "right", "addend")
            : NumericFunctionsEmitter.New(vector, c => Call("FusedMultiplyAdd", $"left.{c}", $"right.{c}", $"addend.{c}"));
        members.Append(NumericFunctionsEmitter.Method("Returns component-wise fused multiply-add results.", "static",
            vector.TypeName, "FusedMultiplyAdd", $"{vector.TypeName} left, {vector.TypeName} right, {vector.TypeName} addend", expression));
    }

    /// <summary>Returns a scalar floating-point math call for this vector family.</summary>
    private static string Call(string method, params string[] args) =>
        $"ScalarMath.{method}({string.Join(", ", args)})";
}

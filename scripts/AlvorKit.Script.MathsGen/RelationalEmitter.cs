namespace AlvorKit.Script.MathsGen;

/// <summary>Emits component-wise numeric relational helpers.</summary>
internal static class RelationalEmitter
{
    /// <summary>Appends relational helpers for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        Emit(vector, members, "Equal", "equality", "==");
        Emit(vector, members, "NotEqual", "inequality", "!=");
        Emit(vector, members, "LessThan", "less-than", "<");
        Emit(vector, members, "LessThanOrEqual", "less-than-or-equal", "<=");
        Emit(vector, members, "GreaterThan", "greater-than", ">");
        Emit(vector, members, "GreaterThanOrEqual", "greater-than-or-equal", ">=");
    }

    /// <summary>Emits one relational helper.</summary>
    private static void Emit(VectorSpec vector, MemberBlock members, string name, string description, string op)
    {
        var boolVector = new VectorSpec(vector.Dimension, VectorCatalog.Bool);
        var resultScalar = ScalarPromotion.BinaryNumericResult(vector.Scalar, vector.Scalar);
        if (resultScalar is null)
            return;

        members.Append(NumericFunctionsEmitter.Method($"Returns a mask containing component-wise {description} results.", "static",
            vector.BoolTypeName, name, $"{vector.TypeName} left, {vector.TypeName} right",
            NumericFunctionsEmitter.New(boolVector, c => $"left.{c} {op} right.{c}")));
        foreach (var scalar in VectorCatalog.Scalars.Where(scalar => !scalar.IsBool))
            EmitScalarPair(vector, members, name, description, op, scalar);
        foreach (var right in VectorCatalog.Vectors.Where(right => right.Dimension == vector.Dimension && right.Scalar != vector.Scalar))
            EmitVectorPair(vector, members, name, description, op, right);
    }

    /// <summary>Emits one vector-scalar relational helper if the same-scalar helper does not already cover it.</summary>
    private static void EmitScalarPair(
        VectorSpec vector,
        MemberBlock members,
        string name,
        string description,
        string op,
        ScalarSpec scalar)
    {
        var resultScalar = ScalarPromotion.BinaryNumericResult(vector.Scalar, scalar);
        if (resultScalar is null || (scalar != vector.Scalar && ScalarPromotion.ExistingScalarOperatorCovers(vector.Scalar, scalar)))
            return;

        var boolVector = new VectorSpec(vector.Dimension, VectorCatalog.Bool);
        members.Append(NumericFunctionsEmitter.Method($"Returns a mask containing component-wise {description} results against a {scalar.Description} scalar.",
            "static", vector.BoolTypeName, name, $"{vector.TypeName} left, {scalar.CSharpName} right",
            NumericFunctionsEmitter.New(boolVector, c => Compare(resultScalar, $"left.{c}", op, "right"))));
        members.Append(NumericFunctionsEmitter.Method($"Returns a mask containing component-wise {description} results from a {scalar.Description} scalar.",
            "static", vector.BoolTypeName, name, $"{scalar.CSharpName} left, {vector.TypeName} right",
            NumericFunctionsEmitter.New(boolVector, c => Compare(resultScalar, "left", op, $"right.{c}"))));
    }

    /// <summary>Emits one cross-scalar vector relational helper if the same-type helper does not already cover it.</summary>
    private static void EmitVectorPair(
        VectorSpec left,
        MemberBlock members,
        string name,
        string description,
        string op,
        VectorSpec right)
    {
        var resultScalar = ScalarPromotion.BinaryNumericResult(left.Scalar, right.Scalar);
        if (resultScalar is null || VectorCatalog.IsImplicitConversion(right.Scalar, left.Scalar))
            return;

        var boolVector = new VectorSpec(left.Dimension, VectorCatalog.Bool);
        members.Append(NumericFunctionsEmitter.Method($"Returns a mask containing component-wise C#-promoted {description} results.", "static",
            left.BoolTypeName, name, $"{left.TypeName} left, {right.TypeName} right",
            NumericFunctionsEmitter.New(boolVector, c => Compare(resultScalar, $"left.{c}", op, $"right.{c}"))));
    }

    /// <summary>Returns a component comparison expression after applying the C# promoted scalar type.</summary>
    private static string Compare(ScalarSpec scalar, string left, string op, string right) =>
        $"({scalar.CSharpName}){left} {op} ({scalar.CSharpName}){right}";
}

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
        members.Append(NumericFunctionsEmitter.Method($"Returns a mask containing component-wise {description} results.", "static",
            vector.BoolTypeName, name, $"{vector.TypeName} left, {vector.TypeName} right",
            NumericFunctionsEmitter.New(boolVector, c => $"left.{c} {op} right.{c}")));
    }
}

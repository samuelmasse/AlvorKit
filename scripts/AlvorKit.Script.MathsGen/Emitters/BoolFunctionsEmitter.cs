namespace AlvorKit.Script.MathsGen;

/// <summary>Emits Boolean vector helpers.</summary>
internal static class BoolFunctionsEmitter
{
    /// <summary>Appends Boolean helpers for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        members.Append(NumericFunctionsEmitter.Property("Gets whether every component is true.", "readonly", "bool", "All",
            string.Join(" && ", vector.Components)));
        members.Append(NumericFunctionsEmitter.Property("Gets whether at least one component is true.", "readonly", "bool", "Any",
            string.Join(" || ", vector.Components)));
        members.Append(NumericFunctionsEmitter.Property("Gets whether every component is false.", "readonly", "bool", "None", "!Any"));
        members.Append(NumericFunctionsEmitter.Method("Returns a mask containing component-wise equality results.", "static", vector.TypeName, "Equal",
            $"{vector.TypeName} left, {vector.TypeName} right", New(vector, "==")));
        members.Append(NumericFunctionsEmitter.Method("Returns a mask containing component-wise inequality results.", "static", vector.TypeName,
            "NotEqual", $"{vector.TypeName} left, {vector.TypeName} right", New(vector, "!=")));
        foreach (var scalar in VectorCatalog.Scalars)
            EmitSelect(vector, members, scalar);
    }

    /// <summary>Emits a typed mask selection helper.</summary>
    private static void EmitSelect(VectorSpec vector, MemberBlock members, ScalarSpec scalar)
    {
        var typeName = scalar.VectorName(vector.Dimension);
        var expression = BooleanSelectExpression.Supports(vector, scalar)
            ? BooleanSelectExpression.Select(vector, scalar)
            : $"new({string.Join(", ", vector.Components.Select(c => $"ScalarMath.Select({c}, whenTrue.{c}, whenFalse.{c})"))})";
        members.Append(NumericFunctionsEmitter.Method($"Selects {scalar.Description} components using this mask.", "readonly", typeName, "Select",
            $"{typeName} whenTrue, {typeName} whenFalse", expression));
    }

    /// <summary>Returns a component-wise Boolean constructor expression.</summary>
    private static string New(VectorSpec vector, string op) =>
        NumericFunctionsEmitter.NewRaw(vector.Components.Select(c => $"left.{c} {op} right.{c}"));
}

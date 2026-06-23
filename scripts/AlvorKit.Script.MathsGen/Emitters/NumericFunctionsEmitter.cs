namespace AlvorKit.Script.MathsGen;

/// <summary>Emits numeric vector helpers.</summary>
internal static class NumericFunctionsEmitter
{
    /// <summary>Appends numeric helpers for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        EmitGeometry(vector, members);
        EmitCommon(vector, members);
        if (vector.Scalar.IsFloating)
            FloatingFunctionsEmitter.Emit(vector, members);
    }

    /// <summary>Emits dot, length, distance, and dimension-specific geometric helpers.</summary>
    private static void EmitGeometry(VectorSpec vector, MemberBlock members)
    {
        members.Append(Property("Gets the squared Euclidean length.", "readonly", vector.Scalar.CSharpName, "LengthSquared", "Dot(this, this)"));
        members.Append(Property("Gets the Euclidean length.", "readonly", LengthType(vector), "Length", Sqrt(vector, "LengthSquared")));
        if (vector.Scalar.IsFloating)
        {
            members.Append(Property("Gets this vector divided by its length.", "readonly", vector.TypeName, "Normalized", "this / Length"));
            members.Append(Property("Gets this vector divided by its length, or zero when its length is zero.", "readonly",
                vector.TypeName, "NormalizedOrZero", "NormalizedOr(default)"));
            members.Append(Method("Returns this vector divided by its length, or fallback when its length is zero.", "readonly", vector.TypeName,
                "NormalizedOr", $"{vector.TypeName} fallback", $"LengthSquared > {vector.Scalar.ZeroLiteral} ? this / Length : fallback"));
            members.Append(Method("Returns value divided by its length.", "static", vector.TypeName, "Normalize",
                $"{vector.TypeName} value", "value.Normalized"));
            members.Append(MathsTemplate.Fragment("try-normalize.csfrag.tmpl", ("TypeName", vector.TypeName),
                ("ZeroLiteral", vector.Scalar.ZeroLiteral), ("SqrtExpression", Sqrt(vector, "lengthSquared"))));
        }

        members.Append(Method("Returns the dot product of two vectors.", "static", vector.Scalar.CSharpName, "Dot",
            $"{vector.TypeName} left, {vector.TypeName} right", Sum(vector, c => $"left.{c} * right.{c}")));
        if (vector.Dimension == 3)
            EmitCross(vector, members);
        if (vector.Dimension == 2 && vector.Scalar.IsSigned)
            EmitPlanarHelpers(vector, members);

        members.Append(Method("Returns the squared distance between two points.", "static", vector.Scalar.CSharpName, "DistanceSquared",
            $"{vector.TypeName} left, {vector.TypeName} right", vector.Scalar.CastArithmetic("(left - right).LengthSquared")));
        members.Append(Method("Returns the distance between two points.", "static", LengthType(vector), "Distance",
            $"{vector.TypeName} left, {vector.TypeName} right", "(left - right).Length"));
    }

    /// <summary>Emits min, max, clamp, and absolute-value helpers.</summary>
    private static void EmitCommon(VectorSpec vector, MemberBlock members)
    {
        members.Append(Method("Returns the component-wise minimum of two vectors.", "static", vector.TypeName, "Min",
            $"{vector.TypeName} left, {vector.TypeName} right", New(vector, c => $"ScalarMath.Min(left.{c}, right.{c})")));
        members.Append(Method("Returns the component-wise maximum of two vectors.", "static", vector.TypeName, "Max",
            $"{vector.TypeName} left, {vector.TypeName} right", New(vector, c => $"ScalarMath.Max(left.{c}, right.{c})")));
        members.Append(Method("Constrains each component between matching minimum and maximum components.", "static", vector.TypeName, "Clamp",
            $"{vector.TypeName} value, {vector.TypeName} min, {vector.TypeName} max",
            New(vector, c => $"ScalarMath.Clamp(value.{c}, min.{c}, max.{c})")));
        members.Append(Method("Constrains each component between scalar minimum and maximum values.", "static", vector.TypeName, "Clamp",
            $"{vector.TypeName} value, {vector.Scalar.CSharpName} min, {vector.Scalar.CSharpName} max",
            $"Clamp(value, new {vector.TypeName}(min), new {vector.TypeName}(max))"));
        if (vector.Scalar.IsSigned)
            members.Append(Method("Returns the absolute value of each component.", "static", vector.TypeName, "Abs",
                $"{vector.TypeName} value", New(vector, c => $"ScalarMath.Abs(value.{c})")));
    }

    /// <summary>Emits a 3D cross product.</summary>
    private static void EmitCross(VectorSpec vector, MemberBlock members) =>
        members.Append(Method("Returns the cross product of two vectors.", "static", vector.TypeName, "Cross",
            $"{vector.TypeName} left, {vector.TypeName} right",
            NewRaw(vector, [
                "(left.Y * right.Z) - (left.Z * right.Y)",
                "(left.Z * right.X) - (left.X * right.Z)",
                "(left.X * right.Y) - (left.Y * right.X)",
            ])));

    /// <summary>Emits 2D perpendicular vectors and scalar cross product helpers.</summary>
    private static void EmitPlanarHelpers(VectorSpec vector, MemberBlock members)
    {
        members.Append(Property("Gets this vector rotated 90 degrees counter-clockwise.", "readonly", vector.TypeName,
            "PerpendicularLeft", NewRaw(vector, ["-Y", "X"])));
        members.Append(Property("Gets this vector rotated 90 degrees clockwise.", "readonly", vector.TypeName,
            "PerpendicularRight", NewRaw(vector, ["Y", "-X"])));
        var determinant = vector.Scalar.CastArithmetic("(left.X * right.Y) - (left.Y * right.X)");
        members.Append(Method("Returns the 2D scalar cross product.", "static", vector.Scalar.CSharpName, "Cross",
            $"{vector.TypeName} left, {vector.TypeName} right", determinant));
        members.Append(Method("Returns the 2D perpendicular dot product.", "static", vector.Scalar.CSharpName, "PerpDot",
            $"{vector.TypeName} left, {vector.TypeName} right", "Cross(left, right)"));
    }

    /// <summary>Renders a generated method.</summary>
    internal static string Method(string summary, string modifiers, string returnType, string name, string parameters, string expression)
    {
        var fullModifiers = modifiers.StartsWith("private", StringComparison.Ordinal) ? modifiers : $"public {modifiers}";
        return MathsTemplate.Fragment("method-expression.csfrag.tmpl", ("Summary", summary), ("Modifiers", fullModifiers),
            ("ReturnType", returnType), ("Name", name), ("Parameters", parameters), ("Expression", expression));
    }

    /// <summary>Renders a generated property.</summary>
    internal static string Property(string summary, string modifiers, string type, string name, string expression) =>
        MathsTemplate.Fragment("property-expression.csfrag.tmpl", ("Summary", summary), ("Modifiers", modifiers),
            ("Type", type), ("Name", name), ("Expression", expression));

    /// <summary>Returns the generated length type.</summary>
    internal static string LengthType(VectorSpec vector) => vector.Scalar.Kind switch
    {
        ScalarKind.Half => "Half",
        ScalarKind.Float or ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16 or ScalarKind.Int or ScalarKind.UInt =>
            "float",
        _ => "double",
    };

    /// <summary>Returns a square-root expression for the scalar family.</summary>
    internal static string Sqrt(VectorSpec vector, string value) => vector.Scalar.Kind switch
    {
        ScalarKind.Half => $"ScalarMath.Sqrt({value})",
        ScalarKind.Float or ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16 or ScalarKind.Int or ScalarKind.UInt =>
            $"ScalarMath.Sqrt((float){value})",
        ScalarKind.Double => $"ScalarMath.Sqrt({value})",
        _ => $"ScalarMath.Sqrt((double){value})",
    };

    /// <summary>Returns a constructor expression with generated component expressions.</summary>
    internal static string New(VectorSpec vector, Func<string, string> expression) =>
        NewRaw(vector, vector.Components.Select(expression));

    /// <summary>Returns a line-wrapped target-typed constructor expression.</summary>
    internal static string NewRaw(IEnumerable<string> expressions) =>
        $"new({Environment.NewLine}            {string.Join($",{Environment.NewLine}            ", expressions)})";

    /// <summary>Returns a line-wrapped target-typed constructor expression for the given scalar family.</summary>
    internal static string NewRaw(VectorSpec vector, IEnumerable<string> expressions) =>
        NewRaw(expressions.Select(vector.Scalar.CastArithmetic));

    /// <summary>Returns a sum expression over all components.</summary>
    private static string Sum(VectorSpec vector, Func<string, string> expression) =>
        vector.Scalar.CastArithmetic(string.Join(" + ", vector.Components.Select(expression)));
}

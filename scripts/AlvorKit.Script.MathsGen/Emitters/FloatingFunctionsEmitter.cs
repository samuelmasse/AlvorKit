namespace AlvorKit.Script.MathsGen;

/// <summary>Emits floating-point-only vector helpers.</summary>
internal static class FloatingFunctionsEmitter
{
    /// <summary>Appends floating-point helpers for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        members.Append(NumericFunctionsEmitter.Method("Linearly interpolates between two vectors without clamping amount.", "static",
            vector.TypeName, "Lerp", $"{vector.TypeName} from, {vector.TypeName} to, {vector.Scalar.CSharpName} amount",
            NumericFunctionsEmitter.New(vector, c => $"ScalarMath.Lerp(from.{c}, to.{c}, amount)")));
        members.Append(NumericFunctionsEmitter.Method("Linearly interpolates between two vectors with component-wise amounts.", "static",
            vector.TypeName, "Lerp", $"{vector.TypeName} from, {vector.TypeName} to, {vector.TypeName} amount",
            NumericFunctionsEmitter.New(vector, c => $"ScalarMath.Lerp(from.{c}, to.{c}, amount.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns the barycentric blend of three vectors.", "static", vector.TypeName, "Barycentric",
            $"{vector.TypeName} a, {vector.TypeName} b, {vector.TypeName} c, {vector.Scalar.CSharpName} u, {vector.Scalar.CSharpName} v",
            NumericFunctionsEmitter.New(vector, c => $"ScalarMath.Barycentric(a.{c}, b.{c}, c.{c}, u, v)")));
        members.Append(NumericFunctionsEmitter.Method("Returns incident reflected around normal.", "static", vector.TypeName, "Reflect",
            $"{vector.TypeName} incident, {vector.TypeName} normal", $"incident - (({vector.Scalar.TwoLiteral} * Dot(normal, incident)) * normal)"));
        members.Append(NumericFunctionsEmitter.Method("Returns a vector facing away from incident according to referenceNormal.", "static",
            vector.TypeName, "FaceForward", $"{vector.TypeName} normal, {vector.TypeName} incident, {vector.TypeName} referenceNormal",
            $"Dot(referenceNormal, incident) < {vector.Scalar.ZeroLiteral} ? normal : -normal"));
        members.Append(MathsTemplate.Fragment("refract.csfrag.tmpl", ("TypeName", vector.TypeName), ("ScalarType", vector.Scalar.CSharpName),
            ("OneLiteral", vector.Scalar.OneLiteral), ("ZeroLiteral", vector.Scalar.ZeroLiteral),
            ("SqrtKExpression", NumericFunctionsEmitter.Sqrt(vector, "k"))));
        EmitCommon(vector, members);
        FloatingComponentFunctionsEmitter.Emit(vector, members);
        EmitRoundingToInt(vector, members);
        EmitFloatingRelational(vector, members);
    }

    /// <summary>Emits common floating-point helpers.</summary>
    private static void EmitCommon(VectorSpec vector, MemberBlock members)
    {
        members.Append(NumericFunctionsEmitter.Method("Constrains each component to the inclusive zero-to-one range.", "static",
            vector.TypeName, "Saturate", $"{vector.TypeName} value", NumericFunctionsEmitter.New(vector, c => $"ScalarMath.Saturate(value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns each component rounded downward.", "static", vector.TypeName, "Floor",
            $"{vector.TypeName} value", NumericFunctionsEmitter.New(vector, c => RoundCall("Floor", $"value.{c}"))));
        members.Append(NumericFunctionsEmitter.Method("Returns each component rounded upward.", "static", vector.TypeName, "Ceiling",
            $"{vector.TypeName} value", NumericFunctionsEmitter.New(vector, c => RoundCall("Ceiling", $"value.{c}"))));
        members.Append(NumericFunctionsEmitter.Method("Returns each component rounded to the nearest value.", "static", vector.TypeName, "Round",
            $"{vector.TypeName} value", NumericFunctionsEmitter.New(vector, c => RoundCall("Round", $"value.{c}"))));
        members.Append(NumericFunctionsEmitter.Method("Returns each component rounded to the nearest value using mode for midpoints.", "static",
            vector.TypeName, "Round", $"{vector.TypeName} value, MidpointRounding mode",
            NumericFunctionsEmitter.New(vector, c => RoundCall("Round", $"value.{c}", ", mode"))));
        members.Append(NumericFunctionsEmitter.Method("Returns each component rounded toward zero.", "static", vector.TypeName, "Truncate",
            $"{vector.TypeName} value", NumericFunctionsEmitter.New(vector, c => RoundCall("Truncate", $"value.{c}"))));
        members.Append(NumericFunctionsEmitter.Method("Returns the fractional part of each component using floor-based modulo semantics.", "static",
            vector.TypeName, "FractionalPart", $"{vector.TypeName} value",
            NumericFunctionsEmitter.New(vector, c => $"ScalarMath.FractionalPart(value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns floor-based modulo for each component.", "static", vector.TypeName, "Modulo",
            $"{vector.TypeName} left, {vector.TypeName} right", NumericFunctionsEmitter.New(vector, c => $"ScalarMath.Modulo(left.{c}, right.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns floor-based modulo for each component against a scalar divisor.", "static",
            vector.TypeName, "Modulo", $"{vector.TypeName} left, {vector.Scalar.CSharpName} right", $"Modulo(left, new {vector.TypeName}(right))"));
        members.Append(NumericFunctionsEmitter.Method("Returns floor-based modulo for each component.", "static", vector.TypeName, "Mod",
            $"{vector.TypeName} left, {vector.TypeName} right", NumericFunctionsEmitter.New(vector, c => $"ScalarMath.Mod(left.{c}, right.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns floor-based modulo for each component against a scalar divisor.", "static",
            vector.TypeName, "Mod", $"{vector.TypeName} left, {vector.Scalar.CSharpName} right", "Modulo(left, right)"));
        EmitStep(vector, members);
    }

    /// <summary>Emits step and smooth-step helpers.</summary>
    private static void EmitStep(VectorSpec vector, MemberBlock members)
    {
        members.Append(NumericFunctionsEmitter.Method("Returns zero where value is below edge and one otherwise.", "static", vector.TypeName, "Step",
            $"{vector.TypeName} edge, {vector.TypeName} value",
            NumericFunctionsEmitter.New(vector, c => $"ScalarMath.Step(edge.{c}, value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns zero where value is below scalar edge and one otherwise.", "static", vector.TypeName,
            "Step", $"{vector.Scalar.CSharpName} edge, {vector.TypeName} value", $"Step(new {vector.TypeName}(edge), value)"));
        members.Append(NumericFunctionsEmitter.Method("Smoothly interpolates from zero to one between edge values.", "static", vector.TypeName,
            "SmoothStep", $"{vector.TypeName} edge0, {vector.TypeName} edge1, {vector.TypeName} value",
            NumericFunctionsEmitter.New(vector, c => $"ScalarMath.SmoothStep(edge0.{c}, edge1.{c}, value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Smoothly interpolates from zero to one between scalar edge values.", "static",
            vector.TypeName, "SmoothStep", $"{vector.Scalar.CSharpName} edge0, {vector.Scalar.CSharpName} edge1, {vector.TypeName} value",
            $"SmoothStep(new {vector.TypeName}(edge0), new {vector.TypeName}(edge1), value)"));
    }

    /// <summary>Emits explicit conversion helpers to matching integer vectors.</summary>
    private static void EmitRoundingToInt(VectorSpec vector, MemberBlock members)
    {
        var intType = vector.IntTypeName;
        members.Append(NumericFunctionsEmitter.Method("Returns this vector truncated toward zero to integer components.", "readonly", intType,
            $"TruncateTo{intType}", "", $"({intType})this"));
        foreach (var (name, method) in new[] { ("Floor", "Floor"), ("Ceiling", "Ceiling"), ("Round", "Round") })
            members.Append(NumericFunctionsEmitter.Method($"Returns this vector rounded with {method} to integer components.", "readonly", intType,
                $"{name}To{intType}", "", $"new({string.Join(", ", vector.Components.Select(c => $"(int)ScalarMath.{method}({c})"))})"));
    }

    /// <summary>Emits floating-point special-value checks.</summary>
    private static void EmitFloatingRelational(VectorSpec vector, MemberBlock members)
    {
        members.Append(NumericFunctionsEmitter.Method("Returns a mask containing component-wise NaN checks.", "static", vector.BoolTypeName, "IsNaN",
            $"{vector.TypeName} value", NumericFunctionsEmitter.New(vector with { Scalar = VectorCatalog.Bool }, c => $"ScalarMath.IsNaN(value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns a mask containing component-wise infinity checks.", "static", vector.BoolTypeName,
            "IsInfinity", $"{vector.TypeName} value",
            NumericFunctionsEmitter.New(vector with { Scalar = VectorCatalog.Bool }, c => $"ScalarMath.IsInfinity(value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns a mask containing component-wise finite-number checks.", "static", vector.BoolTypeName,
            "IsFinite", $"{vector.TypeName} value",
            NumericFunctionsEmitter.New(vector with { Scalar = VectorCatalog.Bool }, c => $"ScalarMath.IsFinite(value.{c})")));
    }

    /// <summary>Returns a scalar rounding call for the floating-point family.</summary>
    private static string RoundCall(string method, string value, string suffix = "") =>
        $"ScalarMath.{method}({value}{suffix})";
}

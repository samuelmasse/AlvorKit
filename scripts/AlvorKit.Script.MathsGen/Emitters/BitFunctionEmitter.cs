namespace AlvorKit.Script.MathsGen;

/// <summary>Emits integer bit inspection helpers.</summary>
internal static class BitFunctionEmitter
{
    /// <summary>Appends bit helpers for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        var intVector = vector.IntTypeName;
        members.Append(NumericFunctionsEmitter.Method("Returns the number of set bits in each component.", "static", intVector, "BitCount",
            $"{vector.TypeName} value", NewInt(vector, c => $"ScalarMath.BitCount(value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns the number of leading zero bits in each component.", "static", intVector,
            "LeadingZeroCount", $"{vector.TypeName} value", NewInt(vector, c => $"ScalarMath.LeadingZeroCount(value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns the number of trailing zero bits in each component.", "static", intVector,
            "TrailingZeroCount", $"{vector.TypeName} value", NewInt(vector, c => $"ScalarMath.TrailingZeroCount(value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns the least-significant set-bit index for each component, or -1 for zero components.",
            "static", intVector, "FindLeastSignificantBit", $"{vector.TypeName} value",
            NewInt(vector, c => $"ScalarMath.FindLeastSignificantBit(value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns the most-significant set-bit index for each component, or -1 for zero components.",
            "static", intVector, "FindMostSignificantBit", $"{vector.TypeName} value",
            NewInt(vector, c => $"ScalarMath.FindMostSignificantBit(value.{c})")));
        members.Append(NumericFunctionsEmitter.Method("Returns whether each component is a positive power of two.", "static", vector.BoolTypeName,
            "IsPowerOfTwo", $"{vector.TypeName} value", NewBool(vector, c => $"ScalarMath.IsPowerOfTwo(value.{c})")));
    }

    /// <summary>Returns a component-wise int vector constructor expression.</summary>
    private static string NewInt(VectorSpec vector, Func<string, string> expression) =>
        $"new {vector.IntTypeName}({Environment.NewLine}            {string.Join($",{Environment.NewLine}            ", vector.Components.Select(expression))})";

    /// <summary>Returns a component-wise bool vector constructor expression.</summary>
    private static string NewBool(VectorSpec vector, Func<string, string> expression) =>
        $"new {vector.BoolTypeName}({Environment.NewLine}            {string.Join($",{Environment.NewLine}            ", vector.Components.Select(expression))})";

}

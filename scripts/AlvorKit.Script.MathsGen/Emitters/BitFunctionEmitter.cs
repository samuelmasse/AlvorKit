namespace AlvorKit.Script.MathsGen;

/// <summary>Emits integer bit inspection helpers.</summary>
internal static class BitFunctionEmitter
{
    /// <summary>Appends bit helpers for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        var intVector = vector.IntTypeName;
        var bitCount = NewInt(vector, c => $"ScalarMath.BitCount(value.{c})");
        members.Append(InlineRetainedVec4(vector, "BitCount",
            NumericFunctionsEmitter.Method("Returns the number of set bits in each component.", "static", intVector, "BitCount",
                $"{vector.TypeName} value", Int32BitFunctionExpression.Function(vector, "BitCount", bitCount))));
        var leadingZeroCount = NewInt(vector, c => $"ScalarMath.LeadingZeroCount(value.{c})");
        members.Append(InlineRetainedVec4(vector, "LeadingZeroCount",
            NumericFunctionsEmitter.Method("Returns the number of leading zero bits in each component.", "static", intVector,
                "LeadingZeroCount", $"{vector.TypeName} value",
                Int32BitFunctionExpression.Function(vector, "LeadingZeroCount", leadingZeroCount))));
        var trailingZeroCount = NewInt(vector, c => $"ScalarMath.TrailingZeroCount(value.{c})");
        members.Append(InlineRetainedVec4(vector, "TrailingZeroCount",
            NumericFunctionsEmitter.Method("Returns the number of trailing zero bits in each component.", "static", intVector,
                "TrailingZeroCount", $"{vector.TypeName} value",
                Int32BitFunctionExpression.Function(vector, "TrailingZeroCount", trailingZeroCount))));
        var findLeastSignificantBit = NewInt(vector, c => $"ScalarMath.FindLeastSignificantBit(value.{c})");
        members.Append(InlineRetainedVec4(vector, "FindLeastSignificantBit",
            NumericFunctionsEmitter.Method("Returns the least-significant set-bit index for each component, or -1 for zero components.",
                "static", intVector, "FindLeastSignificantBit", $"{vector.TypeName} value",
                Int32BitFunctionExpression.Function(vector, "FindLeastSignificantBit", findLeastSignificantBit))));
        var findMostSignificantBit = NewInt(vector, c => $"ScalarMath.FindMostSignificantBit(value.{c})");
        members.Append(InlineRetainedVec4(vector, "FindMostSignificantBit",
            NumericFunctionsEmitter.Method("Returns the most-significant set-bit index for each component, or -1 for zero components.",
                "static", intVector, "FindMostSignificantBit", $"{vector.TypeName} value",
                Int32BitFunctionExpression.Function(vector, "FindMostSignificantBit", findMostSignificantBit))));
        var isPowerOfTwo = NewBool(vector, c => $"ScalarMath.IsPowerOfTwo(value.{c})");
        members.Append(InlineRetainedVec4(vector, "IsPowerOfTwo",
            NumericFunctionsEmitter.Method("Returns whether each component is a positive power of two.", "static", vector.BoolTypeName,
                "IsPowerOfTwo", $"{vector.TypeName} value",
                Int32BitFunctionExpression.Function(vector, "IsPowerOfTwo", isPowerOfTwo))));
        if (Int32BitFunctionExpression.Supports(vector))
            members.Append(Int32BitFunctionExpression.Helpers(vector));
    }

    /// <summary>Returns a component-wise int vector constructor expression.</summary>
    private static string NewInt(VectorSpec vector, Func<string, string> expression) =>
        $"new {vector.IntTypeName}({Environment.NewLine}            {string.Join($",{Environment.NewLine}            ", vector.Components.Select(expression))})";

    /// <summary>Returns a component-wise bool vector constructor expression.</summary>
    private static string NewBool(VectorSpec vector, Func<string, string> expression) =>
        $"new {vector.BoolTypeName}({Environment.NewLine}            {string.Join($",{Environment.NewLine}            ", vector.Components.Select(expression))})";

    /// <summary>Marks only the measured retained Vec4 Int32 bit-function leaves for caller inlining.</summary>
    private static string InlineRetainedVec4(VectorSpec vector, string method, string declaration)
    {
        if (!Int32BitFunctionExpression.Supports(vector) ||
            method == "IsPowerOfTwo" && vector.Scalar.Kind == ScalarKind.UInt)
            return declaration;

        var firstLineEnd = declaration.IndexOf(Environment.NewLine, StringComparison.Ordinal);
        return declaration.Insert(firstLineEnd + Environment.NewLine.Length,
            $"    [MethodImpl(MethodImplOptions.AggressiveInlining)]{Environment.NewLine}");
    }

}

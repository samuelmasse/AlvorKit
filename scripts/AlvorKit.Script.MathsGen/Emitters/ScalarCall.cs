namespace AlvorKit;

/// <summary>Builds direct type-specific scalar calls for generated component expressions.</summary>
internal static class ScalarCall
{
    /// <summary>Returns a direct static call on the vector's concrete scalar type.</summary>
    public static string Function(ScalarSpec scalar, string method, params string[] arguments) =>
        $"{scalar.CSharpName}.{method}({string.Join(", ", arguments)})";

    /// <summary>Returns a direct population-count expression.</summary>
    public static string BitCount(ScalarSpec scalar, string value) => scalar.BitWidth switch
    {
        <= 32 => $"System.Numerics.BitOperations.PopCount({Unsigned32(scalar, value)})",
        64 => $"System.Numerics.BitOperations.PopCount({Unsigned64(scalar, value)})",
        128 => $"(int){scalar.CSharpName}.PopCount({value})",
        _ => throw new InvalidOperationException($"{scalar.CSharpName} does not support bit operations."),
    };

    /// <summary>Returns a direct leading-zero-count expression.</summary>
    public static string LeadingZeroCount(ScalarSpec scalar, string value)
    {
        var expression = scalar.BitWidth switch
        {
            <= 32 => $"System.Numerics.BitOperations.LeadingZeroCount({Unsigned32(scalar, value)})",
            64 => $"System.Numerics.BitOperations.LeadingZeroCount({Unsigned64(scalar, value)})",
            128 => $"(int){scalar.CSharpName}.LeadingZeroCount({value})",
            _ => throw new InvalidOperationException($"{scalar.CSharpName} does not support bit operations."),
        };
        return scalar.BitWidth is 8 or 16 ? $"{expression} - {32 - scalar.BitWidth}" : expression;
    }

    /// <summary>Returns a direct trailing-zero-count expression.</summary>
    public static string TrailingZeroCount(ScalarSpec scalar, string value) => scalar.BitWidth is 8 or 16
        ? $"{value} == {scalar.ZeroLiteral} ? {scalar.BitWidth} : {TrailingZeroCountNonZero(scalar, value)}"
        : TrailingZeroCountNonZero(scalar, value);

    /// <summary>Returns a direct least-significant-set-bit expression.</summary>
    public static string FindLeastSignificantBit(ScalarSpec scalar, string value) =>
        $"{value} == {scalar.ZeroLiteral} ? -1 : {TrailingZeroCountNonZero(scalar, value)}";

    /// <summary>Returns a direct most-significant-set-bit expression.</summary>
    public static string FindMostSignificantBit(ScalarSpec scalar, string value) =>
        $"{value} == {scalar.ZeroLiteral} ? -1 : {scalar.BitWidth - 1} - ({LeadingZeroCount(scalar, value)})";

    /// <summary>Returns a direct positive-power-of-two expression.</summary>
    public static string IsPowerOfTwo(ScalarSpec scalar, string value) =>
        $"{value} > {scalar.ZeroLiteral} && {scalar.CSharpName}.IsPow2({value})";

    private static string TrailingZeroCountNonZero(ScalarSpec scalar, string value) => scalar.BitWidth switch
    {
        <= 32 => $"System.Numerics.BitOperations.TrailingZeroCount({Unsigned32(scalar, value)})",
        64 => $"System.Numerics.BitOperations.TrailingZeroCount({Unsigned64(scalar, value)})",
        128 => $"(int){scalar.CSharpName}.TrailingZeroCount({value})",
        _ => throw new InvalidOperationException($"{scalar.CSharpName} does not support bit operations."),
    };

    private static string Unsigned32(ScalarSpec scalar, string value) => scalar.Kind switch
    {
        ScalarKind.Int8 => $"(uint)(byte){value}",
        ScalarKind.UInt8 => $"(uint){value}",
        ScalarKind.Int16 => $"(uint)(ushort){value}",
        ScalarKind.UInt16 => $"(uint){value}",
        ScalarKind.Int => $"(uint){value}",
        ScalarKind.UInt => value,
        _ => throw new InvalidOperationException($"{scalar.CSharpName} is not a 32-bit-or-smaller integer."),
    };

    private static string Unsigned64(ScalarSpec scalar, string value) => scalar.Kind switch
    {
        ScalarKind.Int64 => $"(ulong){value}",
        ScalarKind.UInt64 => value,
        _ => throw new InvalidOperationException($"{scalar.CSharpName} is not a 64-bit integer."),
    };
}

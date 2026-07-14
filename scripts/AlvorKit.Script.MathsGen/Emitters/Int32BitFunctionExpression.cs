namespace AlvorKit.Script.MathsGen;

/// <summary>Builds retained packed bit-function expressions for complete-register Int32 vectors.</summary>
internal static class Int32BitFunctionExpression
{
    /// <summary>Gets whether the vector has the retained Vec4 Int32 helper block.</summary>
    public static bool Supports(VectorSpec vector) =>
        vector.Dimension == 4 && vector.Scalar.Kind is ScalarKind.Int or ScalarKind.UInt;

    /// <summary>Returns a hardware-gated packed expression with the exact scalar fallback.</summary>
    public static string Function(VectorSpec vector, string method, string scalarFallback)
    {
        if (!Supports(vector) || method == "IsPowerOfTwo" && vector.Scalar.Kind == ScalarKind.UInt)
            return scalarFallback;

        var (condition, candidate) = method switch
        {
            "BitCount" => (
                "System.Runtime.Intrinsics.X86.Ssse3.IsSupported",
                "StoreInt32(BitCountPacked(PackUInt32(value)))"),
            "LeadingZeroCount" => (HasPackedLeadingZeroCount, "StoreInt32(LeadingZeroCountPacked(PackUInt32(value)))"),
            "TrailingZeroCount" => (HasPackedLeadingZeroCount, "StoreInt32(TrailingZeroCountPacked(PackUInt32(value)))"),
            "FindLeastSignificantBit" => (HasPackedLeadingZeroCount, "StoreInt32(FindLeastSignificantBitPacked(PackUInt32(value)))"),
            "FindMostSignificantBit" => (HasPackedLeadingZeroCount, "StoreInt32(FindMostSignificantBitPacked(PackUInt32(value)))"),
            "IsPowerOfTwo" => ("System.Runtime.Intrinsics.Vector128.IsHardwareAccelerated", "IsPowerOfTwoPacked(value)"),
            _ => throw new ArgumentOutOfRangeException(nameof(method)),
        };
        return $"{condition}{Environment.NewLine}            ? {candidate}{Environment.NewLine}            : {scalarFallback}";
    }

    /// <summary>Returns the private packed helper block for one signed or unsigned Vec4 Int32 type.</summary>
    public static string Helpers(VectorSpec vector)
    {
        var pack = vector.Scalar.Kind == ScalarKind.Int
            ? "Unsafe.BitCast<System.Runtime.Intrinsics.Vector128<int>, System.Runtime.Intrinsics.Vector128<uint>>(" +
                "Unsafe.BitCast<Vec4i, System.Runtime.Intrinsics.Vector128<int>>(value))"
            : "Unsafe.BitCast<Vec4u, System.Runtime.Intrinsics.Vector128<uint>>(value)";
        var powerOfTwo = vector.Scalar.Kind == ScalarKind.Int
            ? MathsTemplate.Fragment("int32-signed-power-of-two-helper.csfrag.tmpl")
            : "";
        return MathsTemplate.Fragment("int32-bit-functions-helper.csfrag.tmpl",
            ("TypeName", vector.TypeName),
            ("PackExpression", pack),
            ("PowerOfTwoHelper", powerOfTwo));
    }

    private const string HasPackedLeadingZeroCount =
        "System.Runtime.Intrinsics.X86.Avx512CD.VL.IsSupported || System.Runtime.Intrinsics.Arm.AdvSimd.IsSupported";
}

namespace AlvorKit.Script.MathsGen;

/// <summary>Builds exact AVX2 variable-count shift expressions for complete-register Int32 vectors.</summary>
internal static class Int32VariableShiftExpression
{
    private const int ShiftMask = 31;

    /// <summary>Gets whether the vector has the exact four-lane Int32 layout required by AVX2.</summary>
    public static bool Supports(VectorSpec vector) =>
        vector.Dimension == 4 && vector.Scalar.Kind is ScalarKind.Int or ScalarKind.UInt;

    /// <summary>Returns a portable AVX2-or-component expression with exact C# count masking.</summary>
    public static string Shift(VectorSpec vector, string op, string fallback)
    {
        var scalar = vector.Scalar.CSharpName;
        var register = $"System.Runtime.Intrinsics.Vector128<{scalar}>";
        var left = $"Unsafe.BitCast<{vector.TypeName}, {register}>(left)";
        var counts = "System.Runtime.Intrinsics.Vector128.AsUInt32(" +
            $"Unsafe.BitCast<Vec4i, System.Runtime.Intrinsics.Vector128<int>>(right) & " +
            $"System.Runtime.Intrinsics.Vector128.Create({ShiftMask.ToString(CultureInfo.InvariantCulture)}))";
        var method = op == "<<" ? "ShiftLeftLogicalVariable" :
            op == ">>" && vector.Scalar.Kind == ScalarKind.Int ? "ShiftRightArithmeticVariable" : "ShiftRightLogicalVariable";
        var intrinsic = $"System.Runtime.Intrinsics.X86.Avx2.{method}({left}, {counts})";
        return $"System.Runtime.Intrinsics.X86.Avx2.IsSupported ? Unsafe.BitCast<{register}, {vector.TypeName}>({intrinsic}) : {fallback}";
    }
}

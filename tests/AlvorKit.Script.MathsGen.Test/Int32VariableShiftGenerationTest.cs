namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests AVX2 variable-count Int32 shift generation.</summary>
[TestClass]
public sealed class Int32VariableShiftGenerationTest
{
    /// <summary>Four-lane Int32 shifts use exact masked AVX2 operations with component fallbacks.</summary>
    [TestMethod]
    public void VectorEmitter_UsesMaskedAvx2ForVec4Int32VariableShifts()
    {
        var signed = VectorFileEmitter.Emit(new(4, VectorCatalog.Int));
        var unsigned = VectorFileEmitter.Emit(new(4, VectorCatalog.UInt));

        StringAssert.Contains(signed, "Avx2.ShiftLeftLogicalVariable");
        StringAssert.Contains(signed, "Avx2.ShiftRightArithmeticVariable");
        StringAssert.Contains(signed, "Avx2.ShiftRightLogicalVariable");
        StringAssert.Contains(unsigned, "Avx2.ShiftLeftLogicalVariable");
        StringAssert.Contains(unsigned, "Avx2.ShiftRightLogicalVariable");
        Assert.IsFalse(unsigned.Contains("Avx2.ShiftRightArithmeticVariable", StringComparison.Ordinal));
        StringAssert.Contains(signed, "Vector128.AsUInt32(");
        StringAssert.Contains(unsigned, "Vector128.AsUInt32(");
        StringAssert.Contains(signed, "Vector128.Create(31))");
        StringAssert.Contains(unsigned, "Vector128.Create(31))");
        StringAssert.Contains(signed, "new(");
        StringAssert.Contains(unsigned, "new(");
    }

    /// <summary>Partial-register Int32 vectors retain their exact component implementation.</summary>
    [TestMethod]
    public void VectorEmitter_KeepsPartialInt32VariableShiftsScalar()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Int));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.UInt));

        Assert.IsFalse(vec2.Contains("ShiftLeftLogicalVariable", StringComparison.Ordinal));
        Assert.IsFalse(vec3.Contains("ShiftLeftLogicalVariable", StringComparison.Ordinal));
    }
}

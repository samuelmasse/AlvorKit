namespace AlvorKit.Interception.CoreClr.Test;

/// <summary>Verifies raw instruction operands and immutable baseline coordinates.</summary>
[TestClass]
public sealed class LoadedIlInstructionDecoderTest
{
    /// <summary>Converts short, long, and switch displacements to baseline offsets.</summary>
    [TestMethod]
    public void DecodeBranchesAndSwitchesUseBaselineInstructionOffsets()
    {
        var body = LoadedMethodBodyFixture.Tiny(
            0x2B, 0x05,
            0x20, 0x00, 0x00, 0x00, 0x00,
            0x38, 0x05, 0x00, 0x00, 0x00,
            0x20, 0x00, 0x00, 0x00, 0x00,
            0x45, 0x02, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00,
            0x2A,
            0x2A);

        var instructions = LoadedMethodBodyDecoder.Decode(body).Instructions;

        CollectionAssert.AreEqual(
            new[] { 0, 2, 7, 12, 17, 30, 31 },
            instructions
                .Select(instruction => instruction.BaselineOffset)
                .ToArray());
        CollectionAssert.AreEqual(
            new[] { 7 },
            instructions[0].Operand.BranchTargets.ToArray());
        CollectionAssert.AreEqual(
            new[] { 17 },
            instructions[2].Operand.BranchTargets.ToArray());
        CollectionAssert.AreEqual(
            new[] { 30, 31 },
            instructions[4].Operand.BranchTargets.ToArray());
    }

    /// <summary>Classifies supported replay prefixes while retaining exact token operands.</summary>
    [TestMethod]
    public void DecodePrefixesMarksOnlyConstrainedAndVolatileAsAccepted()
    {
        var body = LoadedMethodBodyFixture.Tiny(
            0xFE, 0x13,
            0xFE, 0x16, 0x03, 0x00, 0x00, 0x02,
            0x6F, 0x04, 0x00, 0x00, 0x0A,
            0xFE, 0x14,
            0x2A);

        var instructions = LoadedMethodBodyDecoder.Decode(body).Instructions;

        Assert.AreEqual("volatile.", instructions[0].OpCode.Name);
        Assert.IsTrue(instructions[0].IsPrefix);
        Assert.IsTrue(instructions[0].IsAcceptedPrefix);
        Assert.AreEqual("constrained.", instructions[1].OpCode.Name);
        Assert.IsTrue(instructions[1].IsAcceptedPrefix);
        Assert.AreEqual(
            0x02000003,
            instructions[1].Operand.IntegerValue);
        Assert.AreEqual("callvirt", instructions[2].OpCode.Name);
        Assert.AreEqual(
            LoadedIlOperandKind.MetadataToken,
            instructions[2].Operand.Kind);
        Assert.AreEqual(0x0A000004, instructions[2].Operand.IntegerValue);
        Assert.AreEqual("tail.", instructions[3].OpCode.Name);
        Assert.IsTrue(instructions[3].IsPrefix);
        Assert.IsFalse(instructions[3].IsAcceptedPrefix);
    }

    /// <summary>Decodes signed integers, floating point, and variable indices by operand kind.</summary>
    [TestMethod]
    public void DecodeScalarOperandsRetainsExactValues()
    {
        var body = LoadedMethodBodyFixture.Tiny(
            0x1F, 0xFB,
            0x21, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01,
            0x22, 0x00, 0x00, 0xC0, 0x3F,
            0x0E, 0x07,
            0xFE, 0x09, 0x01, 0x01,
            0x2A);

        var instructions = LoadedMethodBodyDecoder.Decode(body).Instructions;

        Assert.AreEqual(-5, instructions[0].Operand.IntegerValue);
        Assert.AreEqual(
            0x0102030405060708L,
            instructions[1].Operand.IntegerValue);
        Assert.AreEqual(
            1.5,
            instructions[2].Operand.FloatingPointValue,
            0.0001);
        Assert.AreEqual(7, instructions[3].Operand.IntegerValue);
        Assert.AreEqual(
            LoadedIlOperandKind.VariableIndex,
            instructions[4].Operand.Kind);
        Assert.AreEqual(257, instructions[4].Operand.IntegerValue);
    }
}

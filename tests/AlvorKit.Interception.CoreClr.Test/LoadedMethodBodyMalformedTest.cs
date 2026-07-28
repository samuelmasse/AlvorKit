namespace AlvorKit.Interception.CoreClr.Test;

/// <summary>Verifies deterministic rejection of structurally unsafe loaded bodies.</summary>
[TestClass]
public sealed class LoadedMethodBodyMalformedTest
{
    /// <summary>Rejects empty and incomplete method headers.</summary>
    [TestMethod]
    public void DecodeRejectsMissingHeaderData()
    {
        Assert.ThrowsExactly<InvalidDataException>(
            () => LoadedMethodBodyDecoder.Decode([]));
        Assert.ThrowsExactly<InvalidDataException>(
            () => LoadedMethodBodyDecoder.Decode([0x03]));
    }

    /// <summary>Rejects an instruction whose encoded operand is truncated.</summary>
    [TestMethod]
    public void DecodeRejectsTruncatedInstructionOperand()
    {
        var body = LoadedMethodBodyFixture.Tiny(" "u8.ToArray());

        var exception = Assert.ThrowsExactly<InvalidDataException>(
            () => LoadedMethodBodyDecoder.Decode(body));

        StringAssert.Contains(exception.Message, "IL_0000");
        StringAssert.Contains(exception.Message, "truncated");
    }

    /// <summary>Rejects a branch into the middle of another instruction.</summary>
    [TestMethod]
    public void DecodeRejectsBranchTargetOutsideInstructionBoundary()
    {
        var body = LoadedMethodBodyFixture.Tiny(
            0x2B, 0x01,
            0x20, 0x00, 0x00, 0x00, 0x00,
            0x2A);

        var exception = Assert.ThrowsExactly<InvalidDataException>(
            () => LoadedMethodBodyDecoder.Decode(body));

        StringAssert.Contains(exception.Message, "not an instruction boundary");
    }

    /// <summary>Rejects an exception clause whose region cuts through an instruction.</summary>
    [TestMethod]
    public void DecodeRejectsExceptionRangeOutsideInstructionBoundary()
    {
        var section = LoadedMethodBodyFixture.SmallCatch(
            tryOffset: 0,
            tryLength: 1,
            handlerOffset: 1,
            handlerLength: 1,
            catchToken: 0x02000003);
        var body = LoadedMethodBodyFixture.Fat(
            [0x20, 0x00, 0x00, 0x00, 0x00, 0x2A],
            section: section);

        var exception = Assert.ThrowsExactly<InvalidDataException>(
            () => LoadedMethodBodyDecoder.Decode(body));

        StringAssert.Contains(exception.Message, "instruction boundaries");
    }

    /// <summary>Rejects malformed exception-section sizes before reading a clause.</summary>
    [TestMethod]
    public void DecodeRejectsPartialExceptionClause()
    {
        var section = LoadedMethodBodyFixture.SmallCatch(
            tryOffset: 0,
            tryLength: 1,
            handlerOffset: 1,
            handlerLength: 1,
            catchToken: 0x02000003);
        section[1] = 15;
        var body = LoadedMethodBodyFixture.Fat(
            [0x00, 0x00, 0x2A],
            section: section);

        var exception = Assert.ThrowsExactly<InvalidDataException>(
            () => LoadedMethodBodyDecoder.Decode(body));

        StringAssert.Contains(exception.Message, "whole 12-byte clauses");
    }
}

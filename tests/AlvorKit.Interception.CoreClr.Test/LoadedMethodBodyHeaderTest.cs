using System.Security.Cryptography;

namespace AlvorKit.Interception.CoreClr.Test;

/// <summary>Verifies immutable body ownership, identity, and header metadata.</summary>
[TestClass]
public sealed class LoadedMethodBodyHeaderTest
{
    /// <summary>Decodes tiny metadata and owns bytes independently of the native buffer.</summary>
    [TestMethod]
    public void DecodeTinyCopiesAuthoritativeBytesAndComputesStableIdentity()
    {
        var nativeBuffer = LoadedMethodBodyFixture.Tiny("*"u8.ToArray());
        var expectedBytes = nativeBuffer.ToArray();
        var expectedIdentity = Convert.ToHexString(
            SHA256.HashData(expectedBytes));

        var snapshot = LoadedMethodBodyDecoder.Decode(nativeBuffer);
        nativeBuffer[1] = 0x00;
        var second = LoadedMethodBodyDecoder.Decode(expectedBytes);

        Assert.AreEqual(LoadedMethodBodyHeaderKind.Tiny, snapshot.HeaderKind);
        Assert.AreEqual(1, snapshot.HeaderSize);
        Assert.AreEqual(1, snapshot.CodeSize);
        Assert.AreEqual((ushort)8, snapshot.MaxStack);
        Assert.IsFalse(snapshot.InitLocals);
        Assert.AreEqual(0, snapshot.LocalSignatureToken);
        CollectionAssert.AreEqual(expectedBytes, snapshot.Bytes.ToArray());
        Assert.AreEqual(expectedIdentity, snapshot.Identity.Value);
        Assert.AreEqual(snapshot.Identity, second.Identity);
        Assert.AreEqual("ret", snapshot.Instructions.Single().OpCode.Name);
    }

    /// <summary>Decodes fat max-stack, local initialization, and local-signature metadata.</summary>
    [TestMethod]
    public void DecodeFatPreservesExecutionHeaderMetadata()
    {
        var body = LoadedMethodBodyFixture.Fat(
            [0x16, 0x2A],
            maxStack: 19,
            initLocals: true,
            localSignatureToken: 0x11000007);

        var snapshot = LoadedMethodBodyDecoder.Decode(body);

        Assert.AreEqual(LoadedMethodBodyHeaderKind.Fat, snapshot.HeaderKind);
        Assert.AreEqual(12, snapshot.HeaderSize);
        Assert.AreEqual(2, snapshot.CodeSize);
        Assert.AreEqual((ushort)19, snapshot.MaxStack);
        Assert.IsTrue(snapshot.InitLocals);
        Assert.AreEqual(0x11000007, snapshot.LocalSignatureToken);
        CollectionAssert.AreEqual(
            new[] { 0, 1 },
            snapshot.Instructions
                .Select(instruction => instruction.BaselineOffset)
                .ToArray());
    }
}

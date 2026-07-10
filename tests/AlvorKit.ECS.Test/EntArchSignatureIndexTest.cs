namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchSignatureIndexTest
{
    /// <summary>Verifies a shared hash never replaces exact signature identity.</summary>
    [TestMethod]
    public void SignatureIndex_EqualHashes_UseExactSignatureIdentity()
    {
        const int firstArchId = 2;
        const int secondArchId = 3;
        const ulong sharedHash = 0;
        int[] archIds = new int[8];
        int[] packedFieldIds = [2, 4, 2, 5];
        int[] signatureEnds = [0, 0, 2, 4];

        EntArchSignatureIndex.Insert(archIds, sharedHash, firstArchId);
        EntArchSignatureIndex.Insert(archIds, sharedHash, secondArchId);

        Assert.AreEqual(
            firstArchId,
            EntArchSignatureIndex.Find(archIds, sharedHash, [2, 4], packedFieldIds, signatureEnds));
        Assert.AreEqual(
            secondArchId,
            EntArchSignatureIndex.Find(archIds, sharedHash, [2, 5], packedFieldIds, signatureEnds));
        Assert.AreEqual(
            EntArchSignatureIndex.EmptySlot,
            EntArchSignatureIndex.Find(archIds, sharedHash, [2, 6], packedFieldIds, signatureEnds));
    }
}

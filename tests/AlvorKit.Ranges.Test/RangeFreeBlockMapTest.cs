namespace AlvorKit.Ranges.Test;

[TestClass]
public sealed class RangeFreeBlockMapTest
{
    /// <summary>Taking a block that exactly consumes the sole free range leaves no free blocks.</summary>
    [TestMethod]
    public void TryTakeBestFit_WhenSoleBlockIsConsumed_ClearsMap()
    {
        var blocks = new RangeFreeBlockMap(1, 10);

        var found = blocks.TryTakeBestFit(10, out var index);

        Assert.IsTrue(found);
        Assert.AreEqual(1, index);
        Assert.AreEqual(0, blocks.BlockCount);
    }

    /// <summary>Taking a block from a non-sole free range keeps the remainder indexed.</summary>
    [TestMethod]
    public void TryTakeBestFit_WhenRemainderExists_AddsRemainder()
    {
        var blocks = new RangeFreeBlockMap(1, 10);

        var found = blocks.TryTakeBestFit(4, out var index);

        Assert.IsTrue(found);
        Assert.AreEqual(1, index);
        Assert.AreEqual(1, blocks.BlockCount);
        Assert.IsTrue(blocks.TryTakeBestFit(6, out var remainderIndex));
        Assert.AreEqual(5, remainderIndex);
    }

    /// <summary>Taking from a multi-block map returns false when no block is large enough.</summary>
    [TestMethod]
    public void TryTakeBestFit_WhenNoBlockFits_ReturnsFalse()
    {
        var blocks = new RangeFreeBlockMap(1, 2);
        blocks.Add(10, 3);

        var found = blocks.TryTakeBestFit(4, out var index);

        Assert.IsFalse(found);
        Assert.AreEqual(0, index);
    }

    /// <summary>Merging immediately after the only free block uses the sole-block left merge path.</summary>
    [TestMethod]
    public void Merge_WhenFreedBlockTouchesSoleBlockTail_MergesLeft()
    {
        var blocks = new RangeFreeBlockMap(1, 10);

        blocks.Merge(11, 5);

        Assert.AreEqual(1, blocks.BlockCount);
        Assert.IsTrue(blocks.TryTakeBestFit(15, out var index));
        Assert.AreEqual(1, index);
    }

    /// <summary>Merging immediately before the only free block uses the sole-block right merge path.</summary>
    [TestMethod]
    public void Merge_WhenFreedBlockTouchesSoleBlockHead_MergesRight()
    {
        var blocks = new RangeFreeBlockMap(10, 5);

        blocks.Merge(1, 9);

        Assert.AreEqual(1, blocks.BlockCount);
        Assert.IsTrue(blocks.TryTakeBestFit(14, out var index));
        Assert.AreEqual(1, index);
    }

    /// <summary>Extending a multi-block map merges with the free block connected to the old tail.</summary>
    [TestMethod]
    public void Extend_WhenLeftBlockConnectedInMultiBlockMap_MergesWithTail()
    {
        var blocks = new RangeFreeBlockMap(1, 1);
        blocks.Add(10, 5);

        blocks.Extend(15, 25);

        Assert.AreEqual(2, blocks.BlockCount);
        Assert.IsTrue(blocks.TryTakeBestFit(15, out var index));
        Assert.AreEqual(10, index);
    }

    /// <summary>Extending a sole map that does not touch the old tail adds a new free block.</summary>
    [TestMethod]
    public void Extend_WhenSoleBlockIsDisconnected_AddsTailBlock()
    {
        var blocks = new RangeFreeBlockMap(1, 5);

        blocks.Extend(10, 20);

        Assert.AreEqual(2, blocks.BlockCount);
        Assert.IsTrue(blocks.TryTakeBestFit(10, out var index));
        Assert.AreEqual(10, index);
    }
}

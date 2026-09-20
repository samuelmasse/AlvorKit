namespace AlvorKit;

/// <summary>Verifies image row transformations and pixel-buffer ownership.</summary>
[TestClass]
public class ImageDataTest
{
    /// <summary>Flipping reverses rows while preserving columns, alpha, dimensions, and the source buffer.</summary>
    [TestMethod]
    public void FlippedVertically_ReversesRowsWithoutChangingSource()
    {
        Vec4u8[] original =
        [
            (1, 2, 3, 0), (4, 5, 6, 255),
            (7, 8, 9, 127), (10, 11, 12, 64),
            (13, 14, 15, 32), (16, 17, 18, 16),
        ];
        var source = original.AsSpan().ToArray();
        var image = new ImageData((2, 3), source);

        var flipped = image.FlippedVertically();

        Assert.AreEqual(image.Size, flipped.Size);
        CollectionAssert.AreEqual(new Vec4u8[]
        {
            (13, 14, 15, 32), (16, 17, 18, 16),
            (7, 8, 9, 127), (10, 11, 12, 64),
            (1, 2, 3, 0), (4, 5, 6, 255),
        }, flipped.Pixels.ToArray());
        CollectionAssert.AreEqual(original, source);
        CollectionAssert.AreEqual(original, flipped.FlippedVertically().Pixels.ToArray());
    }

    /// <summary>A single row retains its order and is copied into independent storage.</summary>
    [TestMethod]
    public void FlippedVertically_SingleRowDoesNotAliasSource()
    {
        Vec4u8[] source = [(1, 2, 3, 4), (5, 6, 7, 8), (9, 10, 11, 12)];
        var flipped = new ImageData((3, 1), source).FlippedVertically();
        source[0] = default;

        CollectionAssert.AreEqual(
            new Vec4u8[] { (1, 2, 3, 4), (5, 6, 7, 8), (9, 10, 11, 12) }, flipped.Pixels.ToArray());
    }

    /// <summary>A one-pixel-wide image reverses its vertical sequence.</summary>
    [TestMethod]
    public void FlippedVertically_SingleColumnReversesPixels()
    {
        Vec4u8[] source = [(1, 2, 3, 4), (5, 6, 7, 8)];

        var flipped = new ImageData((1, 2), source).FlippedVertically();

        CollectionAssert.AreEqual(new Vec4u8[] { (5, 6, 7, 8), (1, 2, 3, 4) }, flipped.Pixels.ToArray());
    }

    /// <summary>The default empty image remains empty when flipped.</summary>
    [TestMethod]
    public void FlippedVertically_EmptyImageRemainsEmpty()
    {
        var flipped = default(ImageData).FlippedVertically();

        Assert.AreEqual(default, flipped.Size);
        Assert.IsTrue(flipped.Pixels.IsEmpty);
    }
}

namespace AlvorKit.Maths.Test;

/// <summary>Tests generated box spatial helpers.</summary>
[TestClass]
public sealed class GeneratedBoxTest
{
    /// <summary>Generated boxes expose inclusive, half-open, and exclusive containment semantics.</summary>
    [TestMethod]
    public void GeneratedBoxContainmentModes_Work()
    {
        var box = Box3.CreateFromCorners(Vec3.Zero, new Vec3(10f));

        Assert.IsTrue(box.ContainsInclusive(new Vec3(10f, 10f, 10f)));
        Assert.IsFalse(box.ContainsHalfOpen(new Vec3(10f, 5f, 5f)));
        Assert.IsTrue(box.ContainsHalfOpen(new Vec3(9.999f, 5f, 5f)));
        Assert.IsFalse(box.ContainsExclusive(Vec3.Zero));
        Assert.IsTrue(box.ContainsExclusive(new Vec3(5f)));
    }

    /// <summary>Generated boxes expose vector-shaped transform and relationship helpers.</summary>
    [TestMethod]
    public void GeneratedBoxRelationships_Work()
    {
        var box = Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(2f));
        var segment = new Segment3(new Vec3(-2f, 0f, 0f), new Vec3(2f, 0f, 0f));
        var sphere = new Sphere3(Vec3.Zero, 0.5f);

        Assert.AreEqual(new Vec3(2f), box.Size);
        Assert.AreEqual(new Box3(new Vec3(-2f), new Vec3(2f)), box.Inflated(new Vec3(1f)));
        Assert.IsTrue(box.Intersects(segment));
        Assert.IsTrue(box.Contains(sphere));
        Assert.AreEqual(new Box3(new Vec3(1f), new Vec3(3f)), box.Translated(new Vec3(2f)));
    }

    /// <summary>Double boxes mirror the single-precision spatial helpers.</summary>
    [TestMethod]
    public void GeneratedBoxDoubleRelationships_Work()
    {
        var box = Box3d.CreateFromCorners(new Vec3d(-1d), new Vec3d(1d));
        var segment = new Segment3d(new Vec3d(0d, -2d, 0d), new Vec3d(0d, 2d, 0d));

        Assert.IsTrue(box.ContainsInclusive(new Vec3d(1d, 0d, 0d)));
        Assert.IsFalse(box.ContainsExclusive(new Vec3d(1d, 0d, 0d)));
        Assert.IsTrue(segment.Intersects(box));
    }
}

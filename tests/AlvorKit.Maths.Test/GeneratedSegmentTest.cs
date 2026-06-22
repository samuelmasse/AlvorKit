namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D segment types.</summary>
[TestClass]
public sealed class GeneratedSegmentTest
{
    /// <summary>Segments expose finite-line closest point and intersection helpers.</summary>
    [TestMethod]
    public void GeneratedSegmentSpatialHelpers_Work()
    {
        var segment = new Segment3(new Vec3(-2f, 0f, 0f), new Vec3(2f, 0f, 0f));
        var box = Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(2f));
        var sphere = new Sphere3(new Vec3(0f, 1f, 0f), 1f);
        var plane = Plane3.CreateFromPointNormal(Vec3.Zero, Vec3.UnitX);

        Assert.AreEqual(new Vec3(4f, 0f, 0f), segment.Direction);
        Assert.AreEqual(new Vec3(0f), segment.Center);
        Assert.AreEqual(new Vec3(0.5f, 0f, 0f), segment.ClosestPoint(new Vec3(0.5f, 4f, 0f)));
        Assert.IsTrue(segment.Intersects(box));
        Assert.IsTrue(segment.Intersects(sphere));
        Assert.IsTrue(segment.TryIntersect(plane, out var amount));
        Assert.AreEqual(0.5f, amount);
    }

    /// <summary>Double segments mirror the single-precision helpers.</summary>
    [TestMethod]
    public void GeneratedSegmentDoubleSpatialHelpers_Work()
    {
        var segment = new Segment3d(new Vec3d(0d, -2d, 0d), new Vec3d(0d, 2d, 0d));
        var box = Box3d.CreateFromCenterSize(Vec3d.Zero, new Vec3d(1d));

        Assert.IsTrue(segment.Intersects(box));
        Assert.AreEqual(new Vec3d(0d, 1d, 0d), segment.ClosestPoint(new Vec3d(2d, 1d, 0d)));
    }
}

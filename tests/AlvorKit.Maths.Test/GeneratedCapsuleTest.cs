namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D capsule types.</summary>
[TestClass]
public sealed class GeneratedCapsuleTest
{
    /// <summary>Capsules expose point, sphere, box, plane, and capsule relationships.</summary>
    [TestMethod]
    public void GeneratedCapsuleSpatialHelpers_Work()
    {
        var capsule = new Capsule3(new Segment3(new Vec3(-1f, 0f, 0f), new Vec3(1f, 0f, 0f)), 0.5f);
        var box = Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(1f));
        var largerBox = Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(2f));
        var nearCornerMiss = new Capsule3(new Segment3(new Vec3(1.15f, 1.15f, -0.5f), new Vec3(1.15f, 1.15f, 0.5f)), 0.2f);
        var sphere = new Sphere3(new Vec3(1.5f, 0f, 0f), 0.5f);
        var plane = Plane3.CreateFromPointNormal(Vec3.Zero, Vec3.UnitY);

        Assert.IsTrue(capsule.Contains(Vec3.Zero));
        Assert.AreEqual(new Vec3(-1f, 0f, 0f), capsule.Segment.Start);
        Assert.IsTrue(capsule.Intersects(box));
        Assert.IsFalse(nearCornerMiss.Intersects(largerBox));
        Assert.IsTrue(capsule.Intersects(sphere));
        Assert.IsTrue(capsule.Intersects(plane));
        Assert.IsTrue(Capsule3.Intersects(capsule, new Capsule3(new Segment3(new Vec3(0f), new Vec3(0f, 1f, 0f)), 0.25f)));
    }

    /// <summary>Capsule distance clamps to the capsule surface.</summary>
    [TestMethod]
    public void GeneratedCapsuleDistance_Work()
    {
        var capsule = new Capsule3(new Segment3(Vec3.Zero, Vec3.UnitX), 0.25f);

        Assert.AreEqual(0f, capsule.DistanceTo(new Vec3(0.5f, 0f, 0f)));
        Assert.AreEqual(0.75f, capsule.DistanceTo(new Vec3(0.5f, 1f, 0f)), 0.0001f);
    }

    /// <summary>Double capsules mirror the single-precision helpers.</summary>
    [TestMethod]
    public void GeneratedCapsuleDoubleSpatialHelpers_Work()
    {
        var capsule = new Capsule3d(new Segment3d(new Vec3d(0d, -1d, 0d), new Vec3d(0d, 1d, 0d)), 0.25d);
        var box = Box3d.CreateFromCenterSize(Vec3d.Zero, new Vec3d(0.5d));

        Assert.IsTrue(capsule.Intersects(box));
        Assert.IsFalse(capsule.Contains(new Vec3d(2d, 0d, 0d)));
    }
}

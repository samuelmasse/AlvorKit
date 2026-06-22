namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D oriented bounding box types.</summary>
[TestClass]
public sealed class GeneratedObbTest
{
    /// <summary>OBBs expose point, sphere, box, plane, and OBB relationships.</summary>
    [TestMethod]
    public void GeneratedObbSpatialHelpers_Work()
    {
        var obb = new Obb3(Vec3.Zero, new Vec3(1f), Quat.Identity);
        var box = Box3.CreateFromCenterSize(new Vec3(0.5f, 0f, 0f), new Vec3(1f));
        var sphere = new Sphere3(Vec3.Zero, 0.5f);
        var plane = Plane3.CreateFromPointNormal(Vec3.Zero, Vec3.UnitY);

        Assert.IsTrue(obb.Contains(Vec3.Zero));
        Assert.IsTrue(obb.Contains(sphere));
        Assert.IsTrue(obb.Intersects(box));
        Assert.IsTrue(obb.Intersects(sphere));
        Assert.IsTrue(obb.Intersects(plane));
        Assert.IsTrue(obb.Intersects(Obb3.CreateFromBox(box)));
    }

    /// <summary>OBB corner copying and transforms preserve useful spatial state.</summary>
    [TestMethod]
    public void GeneratedObbCornersAndTransforms_Work()
    {
        var obb = new Obb3(Vec3.Zero, new Vec3(1f), Quat.Identity);
        Span<Vec3> corners = stackalloc Vec3[Obb3.CornerCount];
        var transformed = Obb3.Transform(Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(2f)), Mat4.Translate(Mat4.Identity, new Vec3(3f, 0f, 0f)));

        Assert.IsTrue(obb.TryCopyCornersTo(corners));
        Assert.AreEqual(new Vec3(3f, 0f, 0f), transformed.Center);
        Assert.AreEqual(new Vec3(1f), transformed.HalfSize);
    }

    /// <summary>Double OBBs mirror the single-precision helpers.</summary>
    [TestMethod]
    public void GeneratedObbDoubleSpatialHelpers_Work()
    {
        var obb = new Obb3d(Vec3d.Zero, new Vec3d(1d), Quatd.Identity);
        var box = Box3d.CreateFromCenterSize(new Vec3d(0.5d, 0d, 0d), new Vec3d(1d));

        Assert.IsTrue(obb.Intersects(box));
        Assert.IsTrue(obb.Contains(new Vec3d(0.25d, 0d, 0d)));
    }
}

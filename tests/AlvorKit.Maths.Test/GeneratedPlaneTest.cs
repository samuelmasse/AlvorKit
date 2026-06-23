namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D plane types.</summary>
[TestClass]
public sealed class GeneratedPlaneTest
{
    /// <summary>Planes evaluate points, normalize, and classify common shapes.</summary>
    [TestMethod]
    public void GeneratedPlaneSpatialHelpers_Work()
    {
        var plane = Plane3.CreateFromPointNormal(Vec3.Zero, new Vec3(0f, 2f, 0f));
        var normalized = plane.Normalized;
        var box = Box3.CreateFromCenterSize(new Vec3(0f, 2f, 0f), new Vec3(1f));
        var obb = Obb3.CreateFromBox(box);
        var crossingObb = Obb3.CreateFromBox(Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(1f)));
        var negativeObb = Obb3.CreateFromBox(Box3.CreateFromCenterSize(new Vec3(0f, -2f, 0f), new Vec3(1f)));
        var crossingSphere = new Sphere3(Vec3.Zero, 1f);

        Assert.IsTrue(normalized.IsNormalized(0.0001f));
        Assert.AreEqual(2f, normalized.Evaluate(new Vec3(0f, 2f, 0f)));
        Assert.AreEqual(PlaneIntersectionKind.Positive, normalized.Classify(box));
        Assert.AreEqual(PlaneIntersectionKind.Positive, normalized.Classify(obb));
        Assert.AreEqual(PlaneIntersectionKind.Intersecting, normalized.Classify(crossingObb));
        Assert.AreEqual(PlaneIntersectionKind.Negative, normalized.Classify(negativeObb));
        Assert.AreEqual(PlaneIntersectionKind.Intersecting, normalized.Classify(crossingSphere));
        Assert.AreEqual(new Vec3(1f, 0f, 3f), normalized.ProjectPoint(new Vec3(1f, 4f, 3f)));
    }

    /// <summary>Planes can be created from three non-collinear points.</summary>
    [TestMethod]
    public void GeneratedPlaneCreateFromPoints_Work()
    {
        Assert.IsTrue(Plane3.TryCreateFromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitZ, out var plane));
        Assert.AreEqual(0f, plane.Evaluate(Vec3.Zero));
        Assert.IsTrue(plane.Normal == -Vec3.UnitY || plane.Normal == Vec3.UnitY);
        Assert.IsFalse(Plane3.TryCreateFromPoints(Vec3.Zero, Vec3.UnitX, new Vec3(2f, 0f, 0f), out _));
    }

    /// <summary>Double planes mirror the single-precision helpers.</summary>
    [TestMethod]
    public void GeneratedPlaneDoubleSpatialHelpers_Work()
    {
        var plane = Plane3d.CreateFromPointNormal(Vec3d.Zero, Vec3d.UnitZ);

        Assert.AreEqual(PlaneIntersectionKind.Positive, plane.Classify(new Vec3d(0d, 0d, 2d)));
        Assert.AreEqual(new Vec3d(1d, 2d, 0d), plane.ProjectPoint(new Vec3d(1d, 2d, 3d)));
    }
}

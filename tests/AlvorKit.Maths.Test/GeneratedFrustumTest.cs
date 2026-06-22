namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D frustum types.</summary>
[TestClass]
public sealed class GeneratedFrustumTest
{
    /// <summary>Frustums created from projection matrices expose finite corners and normalized planes.</summary>
    [TestMethod]
    public void GeneratedFrustumFiniteHelpers_Work()
    {
        var projection = Mat4.CreatePerspectiveFieldOfView(float.Pi / 2f, 1f, 1f, 10f);
        var frustum = Frustum3.CreateFromClipTransform(projection);
        Span<Vec3> corners = stackalloc Vec3[Frustum3.CornerCount];
        Span<Plane3> planes = stackalloc Plane3[Frustum3.PlaneCount];

        Assert.IsTrue(frustum.HasFiniteCorners);
        Assert.IsTrue(frustum.TryCopyCornersTo(corners));
        Assert.IsTrue(frustum.TryCopyNormalizedPlanesTo(planes));
        Assert.IsTrue(planes[0].IsNormalized(0.0001f));
        Assert.IsTrue(frustum.TryCreateBoundingBox(out var bounds));
        Assert.IsTrue(bounds.Contains(new Vec3(0f, 0f, -5f)));
    }

    /// <summary>Frustums classify boxes, spheres, OBBs, capsules, and other frustums.</summary>
    [TestMethod]
    public void GeneratedFrustumRelationships_Work()
    {
        var projection = Mat4.CreatePerspectiveFieldOfView(float.Pi / 2f, 1f, 1f, 10f);
        var frustum = Frustum3.CreateFromClipTransform(projection);
        var insideBox = Box3.CreateFromCenterSize(new Vec3(0f, 0f, -5f), new Vec3(1f));
        var outsideBox = Box3.CreateFromCenterSize(new Vec3(100f), new Vec3(1f));
        var sphere = new Sphere3(new Vec3(0f, 0f, -5f), 0.5f);
        var obb = Obb3.CreateFromBox(insideBox);
        var capsule = new Capsule3(new Segment3(new Vec3(0f, 0f, -4f), new Vec3(0f, 0f, -6f)), 0.25f);

        Assert.IsTrue(frustum.Contains(new Vec3(0f, 0f, -5f)));
        Assert.AreEqual(ContainmentKind.Contains, frustum.Classify(insideBox));
        Assert.AreEqual(ContainmentKind.Disjoint, frustum.ClassifyPrecise(outsideBox));
        Assert.IsTrue(frustum.IntersectsPrecise(insideBox));
        Assert.IsTrue(frustum.Intersects(sphere));
        Assert.IsTrue(frustum.Intersects(obb));
        Assert.IsTrue(frustum.Intersects(capsule));
        Assert.IsTrue(frustum.Contains(frustum));
        Assert.IsTrue(frustum.TryClassify(frustum, out var selfClassification));
        Assert.AreEqual(ContainmentKind.Contains, selfClassification);
    }

    /// <summary>Double frustums mirror the single-precision helpers.</summary>
    [TestMethod]
    public void GeneratedFrustumDoubleFiniteHelpers_Work()
    {
        var projection = Mat4d.CreatePerspectiveFieldOfView(double.Pi / 2d, 1d, 1d, 10d);
        var frustum = Frustum3d.CreateFromClipTransform(projection);

        Assert.IsTrue(frustum.HasFiniteCorners);
        Assert.IsTrue(frustum.TryCreateBoundingBox(out var bounds));
        Assert.IsTrue(bounds.Contains(new Vec3d(0d, 0d, -5d)));
    }
}

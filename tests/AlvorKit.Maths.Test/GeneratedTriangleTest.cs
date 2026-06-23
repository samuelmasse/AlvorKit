namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D triangle types.</summary>
[TestClass]
public sealed class GeneratedTriangleTest
{
    /// <summary>Triangles expose normal, plane, barycentric, and containment helpers.</summary>
    [TestMethod]
    public void GeneratedTriangleSurfaceHelpers_Work()
    {
        var triangle = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);

        Assert.AreEqual(Vec3.UnitZ, triangle.Normal);
        Assert.AreEqual(0.5f, triangle.Area);
        Assert.AreEqual(0f, triangle.Plane.Evaluate(Vec3.Zero));
        Assert.IsTrue(triangle.Contains(new Vec3(0.25f, 0.25f, 0f)));
        Assert.IsFalse(triangle.Contains(new Vec3(0.75f, 0.75f, 0f)));
        Assert.AreEqual(new Vec3(0.5f, 0.25f, 0.25f), triangle.Barycentric(new Vec3(0.25f, 0.25f, 0f)));
    }

    /// <summary>Triangles expose direct spatial relationships with boxes, spheres, and rays.</summary>
    [TestMethod]
    public void GeneratedTriangleRelationships_Work()
    {
        var triangle = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var box = Box3.CreateFromCenterSize(new Vec3(0.25f, 0.25f, 0f), new Vec3(0.25f));
        var cornerCrossingBox = Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(1f));
        var cornerCrossingTriangle = new Triangle3(
            new Vec3(0.4f, 0.6f, 0f),
            new Vec3(0.6f, 0.4f, 0f),
            new Vec3(0.6f, 0.6f, 0f));
        var sphere = new Sphere3(new Vec3(0.25f, 0.25f, 0.25f), 0.25f);
        var ray = new Ray3(new Vec3(0.25f, 0.25f, -1f), Vec3.UnitZ);

        Assert.IsTrue(triangle.Intersects(box));
        Assert.IsTrue(cornerCrossingTriangle.Intersects(cornerCrossingBox));
        Assert.IsTrue(triangle.Intersects(sphere));
        Assert.IsTrue(triangle.TryIntersect(ray, out var distance));
        Assert.AreEqual(1f, distance);
    }

    /// <summary>Degenerate triangles report missing surface data cleanly.</summary>
    [TestMethod]
    public void GeneratedTriangleDegenerateSurfaceHelpers_Work()
    {
        var triangle = new Triangle3(Vec3.Zero, Vec3.UnitX, new Vec3(2f, 0f, 0f));

        Assert.IsTrue(triangle.IsDegenerate);
        Assert.IsFalse(triangle.TryGetNormal(out _));
        Assert.ThrowsException<InvalidOperationException>(() => _ = triangle.Normal);
        Assert.ThrowsException<InvalidOperationException>(() => _ = triangle.Plane);
    }

    /// <summary>Double triangles mirror the single-precision helpers.</summary>
    [TestMethod]
    public void GeneratedTriangleDoubleSurfaceHelpers_Work()
    {
        var triangle = new Triangle3d(Vec3d.Zero, Vec3d.UnitX, Vec3d.UnitY);

        Assert.AreEqual(Vec3d.UnitZ, triangle.Normal);
        Assert.IsTrue(triangle.Contains(new Vec3d(0.25d, 0.25d, 0d)));
        Assert.IsTrue(triangle.Intersects(new Sphere3d(new Vec3d(0.25d, 0.25d, 0.25d), 0.25d)));
    }
}

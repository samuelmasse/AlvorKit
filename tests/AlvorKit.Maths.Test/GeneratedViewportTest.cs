namespace AlvorKit.Maths.Test;

/// <summary>Tests generated viewport projection and picking helpers.</summary>
[TestClass]
public sealed class GeneratedViewportTest
{
    /// <summary>Viewports project and unproject through existing matrix conventions while honoring depth intervals.</summary>
    [TestMethod]
    public void GeneratedViewportProjectUnProject_Work()
    {
        var viewport = new Viewport(
            Box2.CreateFromCorners(Vec2.Zero, new Vec2(100f)),
            new Intervalf(0.2f, 0.8f));
        Span<float> copied = stackalloc float[Viewport.ComponentCount];

        var projected = viewport.Project(new Vec3(0f, 0f, 0f), Mat4.Identity);
        var unprojected = viewport.UnProject(projected, Mat4.Identity);
        viewport.CopyTo(copied);
        Viewport.ComponentRef(ref viewport, 4) = 0f;

        Assert.AreEqual(new Vec3(50f, 50f, 0.5f), projected);
        Assert.AreEqual(new Vec3(0f, 0f, 0f), unprojected);
        CollectionAssert.AreEqual(new[] { 0f, 0f, 100f, 100f, 0.2f, 0.8f }, copied.ToArray());
        Assert.AreEqual(0f, viewport.Depth.Min);
    }

    /// <summary>Viewports create pick rays and pick matrices from vector and box inputs.</summary>
    [TestMethod]
    public void GeneratedViewportPicking_Work()
    {
        var viewport = new Viewport(Box2.CreateFromCorners(Vec2.Zero, new Vec2(100f)));
        var ray = viewport.CreatePickRay(new Vec2(50f), Mat4.Identity);
        var pick = viewport.CreatePickMatrix(Box2.CreateFromCenterSize(new Vec2(50f), new Vec2(50f)));

        Assert.AreEqual(new Vec3(0f, 0f, -1f), ray.Origin);
        Assert.AreEqual(Vec3.UnitZ, ray.Direction);
        Assert.AreNotEqual(Mat4.Identity, pick);
    }

    /// <summary>Double viewports mirror the single-precision helpers.</summary>
    [TestMethod]
    public void GeneratedViewportDoubleProjectUnProject_Work()
    {
        var viewport = new Viewportd(Box2d.CreateFromCorners(Vec2d.Zero, new Vec2d(100d)));

        var projected = viewport.Project(new Vec3d(0d, 0d, 0d), Mat4d.Identity);
        var unprojected = viewport.UnProject(projected, Mat4d.Identity);

        Assert.AreEqual(new Vec3d(50d, 50d, 0.5d), projected);
        Assert.AreEqual(new Vec3d(0d, 0d, 0d), unprojected);
    }
}

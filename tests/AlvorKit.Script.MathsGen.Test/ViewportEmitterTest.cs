namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated viewport source planning and emission.</summary>
[TestClass]
public sealed class ViewportEmitterTest
{
    /// <summary>Viewport catalogs use the float and double viewport names.</summary>
    [TestMethod]
    public void ViewportCatalog_UsesExpectedNames()
    {
        var viewport = new ViewportSpec(VectorCatalog.Float);
        var names = ViewportCatalog.Viewports.Select(spec => spec.TypeName).ToArray();

        Assert.AreEqual(2, names.Length);
        CollectionAssert.AreEqual(new[] { "Viewport", "Viewportd" }, names);
        Assert.AreEqual("Viewport", VectorCatalog.Float.ViewportName());
        Assert.AreEqual("Viewportd", VectorCatalog.Double.ViewportName());
        Assert.AreEqual("Box2", viewport.Box2TypeName);
        Assert.AreEqual("Intervalf", viewport.IntervalTypeName);
        Assert.AreEqual("Mat4", viewport.Matrix4TypeName);
        Assert.AreEqual(24, viewport.SizeBytes);
    }

    /// <summary>Viewport source includes projection helpers, span behavior, parsing, and conversions.</summary>
    [TestMethod]
    public void ViewportEmitter_EmitsExpectedViewportFeatures()
    {
        var viewport = ViewportFileEmitter.Emit(new(VectorCatalog.Float));
        var viewportd = ViewportFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(viewport, "public struct Viewport(Box2 bounds, Intervalf depth)");
        StringAssert.Contains(viewport, "public Viewport(Box2 bounds)");
        StringAssert.Contains(viewport, "public const int ComponentCount = 6;");
        StringAssert.Contains(viewport, "public readonly Vec4 ToViewportVector()");
        StringAssert.Contains(viewport, "public readonly Mat4 CreateTransform(ProjectionDepthRange depthRange)");
        StringAssert.Contains(viewport, "public readonly Vec3 Project(Vec3 source, Mat4 clipFromSource)");
        StringAssert.Contains(viewport, "public readonly Vec3 UnProject(Vec3 screen, Mat4 sourceFromClip)");
        StringAssert.Contains(viewport, "public readonly Ray3 CreatePickRay(Vec2 screen, Mat4 worldFromClip)");
        StringAssert.Contains(viewport, "return ref Intervalf.ComponentRef(ref value.Depth, index - 4);");
        StringAssert.Contains(viewport, "public static implicit operator Viewportd(Viewport value)");
        StringAssert.Contains(viewportd, "public static explicit operator Viewport(Viewportd value)");
        StringAssert.Contains(viewportd, "public const int SizeInBytes = 48;");
    }
}

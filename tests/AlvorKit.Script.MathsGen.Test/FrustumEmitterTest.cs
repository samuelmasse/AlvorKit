namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated frustum source emission.</summary>
[TestClass]
public sealed class FrustumEmitterTest
{
    /// <summary>Frustum source includes sphere query APIs for both scalar families.</summary>
    [TestMethod]
    public void FrustumEmitter_EmitsSphereQueries()
    {
        var frustum = FrustumFileEmitter.Emit(new(VectorCatalog.Float));
        var frustumd = FrustumFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(frustum, "IFrustum3Sphere<Frustum3, float, Vec3, Vec4, Plane3, Box3, Sphere3>");
        StringAssert.Contains(frustum, "public readonly bool Contains(Sphere3 sphere)");
        StringAssert.Contains(frustum, "public readonly bool Intersects(Sphere3 sphere)");
        StringAssert.Contains(frustum, "public readonly ContainmentKind Classify(Sphere3 sphere)");
        StringAssert.Contains(frustum, "sphere.Radius * plane.NormalLength");
        StringAssert.Contains(frustumd, "IFrustum3Sphere<Frustum3d, double, Vec3d, Vec4d, Plane3d, Box3d, Sphere3d>");
        StringAssert.Contains(frustumd, "public readonly bool Contains(Sphere3d sphere)");
        StringAssert.Contains(frustumd, "public readonly bool Intersects(Sphere3d sphere)");
        StringAssert.Contains(frustumd, "public readonly ContainmentKind Classify(Sphere3d sphere)");
    }
}

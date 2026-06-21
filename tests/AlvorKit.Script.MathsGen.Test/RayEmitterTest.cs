namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated 3D ray source planning and emission.</summary>
[TestClass]
public sealed class RayEmitterTest
{
    /// <summary>Ray catalogs use the float and double 3D ray names.</summary>
    [TestMethod]
    public void RayCatalog_UsesExpectedNames()
    {
        var names = RayCatalog.Rays.Select(ray => ray.TypeName).ToArray();

        Assert.AreEqual(2, names.Length);
        CollectionAssert.AreEqual(new[] { "Ray3", "Ray3d" }, names);
        Assert.AreEqual("Ray3", VectorCatalog.Float.RayName());
        Assert.AreEqual("Ray3d", VectorCatalog.Double.RayName());
    }

    /// <summary>Ray source includes point helpers, intersection queries, formatting, parsing, and scalar conversions.</summary>
    [TestMethod]
    public void RayEmitter_EmitsExpectedRayFeatures()
    {
        var ray = RayFileEmitter.Emit(new(VectorCatalog.Float));
        var rayd = RayFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(ray, "/// <summary>Single-precision floating-point 3D ray for spatial queries.");
        StringAssert.Contains(ray, "public struct Ray3(Vec3 origin, Vec3 direction)");
        StringAssert.Contains(ray, "IRay3<Ray3, float, Vec3, Vec4, Plane3, Box3, Sphere3, Frustum3, Intervalf>");
        StringAssert.Contains(ray, "public readonly Vec3 PointAt(float distance)");
        StringAssert.Contains(ray, "public readonly bool TryNormalize(out Ray3 result)");
        StringAssert.Contains(ray, "public readonly bool TryIntersect(Plane3 plane, out float distance)");
        StringAssert.Contains(ray, "public readonly bool TryIntersect(Box3 box, out Intervalf distances)");
        StringAssert.Contains(ray, "public readonly bool TryIntersect(Sphere3 sphere, out float distance)");
        StringAssert.Contains(ray, "public readonly bool TryIntersect(Frustum3 frustum, out Intervalf distances)");
        StringAssert.Contains(ray, "public static implicit operator Ray3d(Ray3 value)");
        StringAssert.Contains(ray, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Ray3 result)");
        StringAssert.Contains(rayd, "IRay3<Ray3d, double, Vec3d, Vec4d, Plane3d, Box3d, Sphere3d, Frustum3d, Intervald>");
        StringAssert.Contains(rayd, "public static explicit operator Ray3(Ray3d value)");
        Assert.IsFalse(ray.Contains("public Ray3(float", StringComparison.Ordinal));
        Assert.IsFalse(ray.Contains("public static Ray3 Create(float", StringComparison.Ordinal));
    }
}

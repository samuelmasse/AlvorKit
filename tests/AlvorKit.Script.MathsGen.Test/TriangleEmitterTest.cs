namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated 3D triangle source planning and emission.</summary>
[TestClass]
public sealed class TriangleEmitterTest
{
    /// <summary>Triangle catalogs use the float and double 3D triangle names.</summary>
    [TestMethod]
    public void TriangleCatalog_UsesExpectedNames()
    {
        var names = TriangleCatalog.Triangles.Select(triangle => triangle.TypeName).ToArray();

        Assert.AreEqual(2, names.Length);
        CollectionAssert.AreEqual(new[] { "Triangle3", "Triangle3d" }, names);
        Assert.AreEqual("Triangle3", VectorCatalog.Float.TriangleName());
        Assert.AreEqual("Triangle3d", VectorCatalog.Double.TriangleName());
    }

    /// <summary>Triangle source includes surface helpers, relationships, formatting, parsing, and conversions.</summary>
    [TestMethod]
    public void TriangleEmitter_EmitsExpectedTriangleFeatures()
    {
        var triangle = TriangleFileEmitter.Emit(new(VectorCatalog.Float));
        var triangled = TriangleFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(triangle, "public struct Triangle3(Vec3 a, Vec3 b, Vec3 c)");
        StringAssert.Contains(triangle, "ITriangle3<Triangle3, float, Vec3, Vec4, Plane3, Ray3, Box3, Sphere3, Frustum3, Intervalf>");
        StringAssert.Contains(triangle, "public readonly Vec3 UnnormalizedNormal =>");
        StringAssert.Contains(triangle, "public readonly bool TryGetNormal(out Vec3 normal)");
        StringAssert.Contains(triangle, "public readonly Vec3 Barycentric(Vec3 point)");
        StringAssert.Contains(triangle, "private readonly bool IntersectsTriangleBox(Box3 box)");
        StringAssert.Contains(triangle, "private static bool SeparatesOnAxis(");
        StringAssert.Contains(triangle, "public readonly bool TryIntersect(Ray3 ray, out float distance)");
        StringAssert.Contains(triangle, "public readonly Vec3 ClosestPoint(Vec3 point)");
        StringAssert.Contains(triangle, "public static implicit operator Triangle3d(Triangle3 value)");
        StringAssert.Contains(triangled, "ITriangle3<Triangle3d, double, Vec3d, Vec4d, Plane3d, Ray3d, Box3d, Sphere3d, Frustum3d, Intervald>");
        StringAssert.Contains(triangled, "public static explicit operator Triangle3(Triangle3d value)");
    }
}

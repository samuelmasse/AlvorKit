namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated 3D oriented bounding box source planning and emission.</summary>
[TestClass]
public sealed class ObbEmitterTest
{
    /// <summary>OBB catalogs use the float and double 3D OBB names.</summary>
    [TestMethod]
    public void ObbCatalog_UsesExpectedNames()
    {
        var names = ObbCatalog.Obbs.Select(obb => obb.TypeName).ToArray();

        Assert.AreEqual(2, names.Length);
        CollectionAssert.AreEqual(new[] { "Obb3", "Obb3d" }, names);
        Assert.AreEqual("Obb3", VectorCatalog.Float.ObbName());
        Assert.AreEqual("Obb3d", VectorCatalog.Double.ObbName());
    }

    /// <summary>OBB source includes storage, corners, relationships, formatting, parsing, and conversions.</summary>
    [TestMethod]
    public void ObbEmitter_EmitsExpectedObbFeatures()
    {
        var obb = ObbFileEmitter.Emit(new(VectorCatalog.Float));
        var obbd = ObbFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(obb, "public struct Obb3(Vec3 center, Vec3 halfSize, Quat orientation)");
        StringAssert.Contains(obb, "IObb3<Obb3, float, Vec3, Vec4, Quat, Plane3, Box3, Sphere3, Frustum3>");
        StringAssert.Contains(obb, "public const int CornerCount = 8;");
        StringAssert.Contains(obb, "public static Obb3 Transform(Box3 box, Mat4 transform)");
        StringAssert.Contains(obb, "public readonly bool TryCopyCornersTo(Span<Vec3> destination)");
        StringAssert.Contains(obb, "public readonly bool Contains(Vec3 point)");
        StringAssert.Contains(obb, "public readonly bool Intersects(Obb3 other)");
        StringAssert.Contains(obb, "public readonly bool Intersects(Frustum3 frustum)");
        StringAssert.Contains(obb, "public static implicit operator Obb3d(Obb3 value)");
        StringAssert.Contains(obb, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Obb3 result)");
        StringAssert.Contains(obbd, "IObb3<Obb3d, double, Vec3d, Vec4d, Quatd, Plane3d, Box3d, Sphere3d, Frustum3d>");
        StringAssert.Contains(obbd, "public static explicit operator Obb3(Obb3d value)");
    }
}

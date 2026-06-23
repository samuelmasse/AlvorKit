namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated 3D capsule source planning and emission.</summary>
[TestClass]
public sealed class CapsuleEmitterTest
{
    /// <summary>Capsule catalogs use the float and double 3D capsule names.</summary>
    [TestMethod]
    public void CapsuleCatalog_UsesExpectedNames()
    {
        var names = CapsuleCatalog.Capsules.Select(capsule => capsule.TypeName).ToArray();

        Assert.AreEqual(2, names.Length);
        CollectionAssert.AreEqual(new[] { "Capsule3", "Capsule3d" }, names);
        Assert.AreEqual("Capsule3", VectorCatalog.Float.CapsuleName());
        Assert.AreEqual("Capsule3d", VectorCatalog.Double.CapsuleName());
    }

    /// <summary>Capsule source includes segment helpers, relationships, formatting, parsing, and conversions.</summary>
    [TestMethod]
    public void CapsuleEmitter_EmitsExpectedCapsuleFeatures()
    {
        var capsule = CapsuleFileEmitter.Emit(new(VectorCatalog.Float));
        var capsuled = CapsuleFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(capsule, "public struct Capsule3(Segment3 segment, float radius)");
        StringAssert.Contains(capsule, "public Segment3 Segment = segment;");
        StringAssert.Contains(capsule, "ICapsule3<Capsule3, float, Vec3, Vec4, Segment3, Plane3, Ray3, Box3, Sphere3, Frustum3, Intervalf>");
        StringAssert.Contains(capsule, "public static Capsule3 Empty =>");
        StringAssert.Contains(capsule, "public readonly bool Contains(Vec3 point)");
        StringAssert.Contains(capsule, "public readonly bool Intersects(Box3 box)");
        StringAssert.Contains(capsule, "private static float SegmentBoxDistanceSquared(Segment3 segment, Box3 box)");
        StringAssert.Contains(capsule, "private static float SegmentRectangleDistanceSquared(");
        StringAssert.Contains(capsule, "public readonly bool TryIntersect(Ray3 ray, out float distance)");
        StringAssert.Contains(capsule, "public readonly ContainmentKind Classify(Frustum3 frustum)");
        StringAssert.Contains(capsule, "public static implicit operator Capsule3d(Capsule3 value)");
        StringAssert.Contains(capsuled, "ICapsule3<Capsule3d, double, Vec3d, Vec4d, Segment3d, Plane3d, Ray3d, Box3d, Sphere3d, Frustum3d, Intervald>");
        StringAssert.Contains(capsuled, "public static explicit operator Capsule3(Capsule3d value)");
    }
}

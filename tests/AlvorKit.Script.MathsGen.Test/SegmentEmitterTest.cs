namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated 3D segment source planning and emission.</summary>
[TestClass]
public sealed class SegmentEmitterTest
{
    /// <summary>Segment catalogs use the float and double 3D segment names.</summary>
    [TestMethod]
    public void SegmentCatalog_UsesExpectedNames()
    {
        var names = SegmentCatalog.Segments.Select(segment => segment.TypeName).ToArray();

        Assert.AreEqual(2, names.Length);
        CollectionAssert.AreEqual(new[] { "Segment3", "Segment3d" }, names);
        Assert.AreEqual("Segment3", VectorCatalog.Float.SegmentName());
        Assert.AreEqual("Segment3d", VectorCatalog.Double.SegmentName());
    }

    /// <summary>Segment source includes finite-line helpers, relationships, formatting, parsing, and conversions.</summary>
    [TestMethod]
    public void SegmentEmitter_EmitsExpectedSegmentFeatures()
    {
        var segment = SegmentFileEmitter.Emit(new(VectorCatalog.Float));
        var segmentd = SegmentFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(segment, "public struct Segment3(Vec3 start, Vec3 end)");
        StringAssert.Contains(segment, "ISegment3<Segment3, float, Vec3, Vec4, Plane3, Box3, Sphere3>");
        StringAssert.Contains(segment, "public readonly Vec3 PointAt(float amount)");
        StringAssert.Contains(segment, "public readonly Vec3 ClosestPoint(Vec3 point)");
        StringAssert.Contains(segment, "public readonly bool Intersects(Sphere3 sphere)");
        StringAssert.Contains(segment, "public readonly bool Intersects(Box3 box)");
        StringAssert.Contains(segment, "public readonly bool TryIntersect(Plane3 plane, out float amount)");
        StringAssert.Contains(segment, "public static implicit operator Segment3d(Segment3 value)");
        StringAssert.Contains(segment, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Segment3 result)");
        StringAssert.Contains(segmentd, "ISegment3<Segment3d, double, Vec3d, Vec4d, Plane3d, Box3d, Sphere3d>");
        StringAssert.Contains(segmentd, "public static explicit operator Segment3(Segment3d value)");
    }
}

namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated plane source emission.</summary>
[TestClass]
public sealed class PlaneEmitterTest
{
    /// <summary>Plane source emits exact-sign classifiers for points, boxes, and spheres.</summary>
    [TestMethod]
    public void PlaneEmitter_EmitsClassificationFeatures()
    {
        var plane = PlaneFileEmitter.Emit(new(VectorCatalog.Float));
        var planed = PlaneFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(plane, "public readonly PlaneIntersectionKind Classify(Vec3 point)");
        StringAssert.Contains(plane, "public readonly PlaneIntersectionKind Classify(Box3 box)");
        StringAssert.Contains(plane, "public readonly PlaneIntersectionKind Classify(Sphere3 sphere)");
        StringAssert.Contains(plane, "public static PlaneIntersectionKind Classify(Plane3 plane, Vec3 point)");
        StringAssert.Contains(planed, "public readonly PlaneIntersectionKind Classify(Box3d box)");
        StringAssert.Contains(planed, "public readonly PlaneIntersectionKind Classify(Sphere3d sphere)");
    }
}

namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated 3D sphere source planning and emission.</summary>
[TestClass]
public sealed class SphereEmitterTest
{
    /// <summary>Sphere catalogs use the float and double 3D sphere names.</summary>
    [TestMethod]
    public void SphereCatalog_UsesExpectedNames()
    {
        var names = SphereCatalog.Spheres.Select(sphere => sphere.TypeName).ToArray();

        Assert.AreEqual(2, names.Length);
        CollectionAssert.AreEqual(new[] { "Sphere3", "Sphere3d" }, names);
        Assert.AreEqual("Sphere3", VectorCatalog.Float.SphereName());
        Assert.AreEqual("Sphere3d", VectorCatalog.Double.SphereName());
    }

    /// <summary>Sphere source includes spatial helpers, formatting, parsing, and scalar conversions.</summary>
    [TestMethod]
    public void SphereEmitter_EmitsExpectedSphereFeatures()
    {
        var sphere = SphereFileEmitter.Emit(new(VectorCatalog.Float));
        var sphered = SphereFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(sphere, "/// <summary>Single-precision floating-point 3D sphere for spatial queries.");
        StringAssert.Contains(sphere, "public struct Sphere3(Vec3 center, float radius)");
        StringAssert.Contains(sphere, "ISphere3<Sphere3, float, Vec3, Box3>");
        StringAssert.Contains(sphere, "public static Sphere3 CreateFromBox(Box3 box)");
        StringAssert.Contains(sphere, "public readonly bool Contains(Vec3 point)");
        StringAssert.Contains(sphere, "public readonly Vec3 ClosestPoint(Vec3 point)");
        StringAssert.Contains(sphere, "public static implicit operator Sphere3d(Sphere3 value)");
        StringAssert.Contains(sphere, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Sphere3 result)");
        StringAssert.Contains(sphered, "/// <summary>Double-precision floating-point 3D sphere for spatial queries.");
        StringAssert.Contains(sphered, "ISphere3<Sphere3d, double, Vec3d, Box3d>");
        StringAssert.Contains(sphered, "public static explicit operator Sphere3(Sphere3d value)");
        Assert.IsFalse(sphere.Contains("public Sphere3(float", StringComparison.Ordinal));
        Assert.IsFalse(sphere.Contains("public static Sphere3 Create(float", StringComparison.Ordinal));
    }
}

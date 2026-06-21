namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D sphere types.</summary>
[TestClass]
public sealed class GeneratedSphereTest
{
    /// <summary>Generated sphere core members expose center, radius, component refs, and span copies.</summary>
    [TestMethod]
    public void GeneratedSphereCoreMembers_Work()
    {
        var sphere = new Sphere3(new Vec3(1f, 2f, 3f), 4f);
        Span<float> copied = stackalloc float[Sphere3.ComponentCount];
        sphere.CopyTo(copied);
        Sphere3.ComponentRef(ref sphere, 3) = 5f;
        sphere[0] = 10f;
        var fromSpan = Sphere3.Create(copied);
        var fromBox = Sphere3.CreateFromBox(Box3.CreateFromCenterSize(new Vec3(1f, 2f, 3f), new Vec3(2f, 4f, 4f)));

        Assert.AreEqual(4, Sphere3.ComponentCount);
        Assert.AreEqual(16, Sphere3.SizeInBytes);
        CollectionAssert.AreEqual(new[] { 1f, 2f, 3f, 4f }, copied.ToArray());
        Assert.AreEqual(new Sphere3(new Vec3(10f, 2f, 3f), 5f), sphere);
        Assert.AreEqual(new Sphere3(new Vec3(1f, 2f, 3f), 4f), fromSpan);
        Assert.AreEqual(new Sphere3(new Vec3(1f, 2f, 3f), 3f), fromBox);
        Assert.AreEqual(10f, sphere.Diameter);
        Assert.AreEqual(25f, sphere.RadiusSquared);
        Assert.IsFalse(sphere.IsEmpty);
        Assert.ThrowsException<ArgumentException>(() => _ = Sphere3.Create(new float[3]));
        Assert.ThrowsException<ArgumentException>(() => sphere.CopyTo(new float[3]));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = sphere[4]);
    }

    /// <summary>Generated spheres deliberately model negative radii as empty spheres.</summary>
    [TestMethod]
    public void GeneratedSphereEmptyHandling_UsesNegativeRadius()
    {
        var empty = new Sphere3(new Vec3(1f, 2f, 3f), -1f);
        var full = new Sphere3(Vec3.Zero, 2f);

        Assert.IsTrue(Sphere3.Empty.IsEmpty);
        Assert.IsTrue(empty.IsEmpty);
        Assert.IsTrue(Sphere3.CreateFromBox(Box3.Empty).IsEmpty);
        Assert.IsFalse(empty.Contains(Vec3.Zero));
        Assert.IsFalse(full.Contains(empty));
        Assert.IsFalse(empty.Intersects(full));
        Assert.AreEqual(new Vec3(1f, 2f, 3f), empty.ClosestPoint(Vec3.Zero));
        Assert.AreEqual(float.PositiveInfinity, empty.DistanceTo(Vec3.Zero));
        Assert.AreEqual(float.PositiveInfinity, empty.DistanceSquaredTo(Vec3.Zero));
    }

    /// <summary>Generated spheres support point and sphere containment, intersections, closest points, and distances.</summary>
    [TestMethod]
    public void GeneratedSphereSpatialQueries_Work()
    {
        var sphere = new Sphere3(Vec3.Zero, 5f);
        var nested = new Sphere3(new Vec3(1f, 0f, 0f), 2f);
        var containedTouching = new Sphere3(new Vec3(2f, 0f, 0f), 3f);
        var overlapping = new Sphere3(new Vec3(8f, 0f, 0f), 4f);
        var touching = new Sphere3(new Vec3(10f, 0f, 0f), 5f);
        var separate = new Sphere3(new Vec3(11f, 0f, 0f), 5f);

        Assert.IsTrue(sphere.Contains(new Vec3(5f, 0f, 0f)));
        Assert.IsFalse(sphere.Contains(new Vec3(5.1f, 0f, 0f)));
        Assert.IsTrue(sphere.Contains(nested));
        Assert.IsTrue(sphere.Contains(containedTouching));
        Assert.IsFalse(sphere.Contains(overlapping));
        Assert.IsTrue(sphere.Intersects(overlapping));
        Assert.IsTrue(Sphere3.Intersects(sphere, touching));
        Assert.IsFalse(sphere.Intersects(separate));
        Assert.AreEqual(new Vec3(5f, 0f, 0f), sphere.ClosestPoint(new Vec3(10f, 0f, 0f)));
        Assert.AreEqual(new Vec3(1f, 0f, 0f), sphere.ClosestPoint(new Vec3(1f, 0f, 0f)));
        Assert.AreEqual(Vec3.Zero, new Sphere3(Vec3.Zero, 0f).ClosestPoint(new Vec3(1f, 0f, 0f)));
        Assert.AreEqual(5f, sphere.DistanceTo(new Vec3(10f, 0f, 0f)));
        Assert.AreEqual(25f, sphere.DistanceSquaredTo(new Vec3(10f, 0f, 0f)));
        Assert.AreEqual(0f, sphere.DistanceTo(new Vec3(1f, 0f, 0f)));
        Assert.AreEqual(0f, sphere.DistanceSquaredTo(new Vec3(1f, 0f, 0f)));
    }

    /// <summary>Generated sphere formatting and parsing mirror box-style nested component text.</summary>
    [TestMethod]
    public void GeneratedSphereFormattingAndParsing_UsesVectorStyle()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;
        var value = new Sphere3(new Vec3(1f, 2f, 3f), 4.5f);
        Span<char> destination = stackalloc char[22];
        Span<byte> utf8Destination = stackalloc byte[22];

        Assert.IsTrue(value.TryFormat(destination, out var charsWritten, default, formatProvider));
        Assert.AreEqual("((1, 2, 3), 4.5)", destination[..charsWritten].ToString());
        Assert.IsTrue(value.TryFormat(utf8Destination, out var bytesWritten, default, formatProvider));
        Assert.AreEqual("((1, 2, 3), 4.5)", Encoding.UTF8.GetString(utf8Destination[..bytesWritten]));
        Assert.AreEqual("((1.0, 2.0, 3.0), 4.5)", value.ToString("0.0", formatProvider));
        Assert.AreEqual(value, Sphere3.Parse("((1, 2, 3), 4.5)", formatProvider));
        Assert.AreEqual(value, ParseUtf8<Sphere3>("((1, 2, 3), 4.5)"u8));
        Assert.IsFalse(Sphere3.TryParse((string?)null, formatProvider, out _));
        Assert.IsFalse(Sphere3.TryParse("not a sphere", formatProvider, out _));
    }

    /// <summary>Generated spheres convert across scalar families and support center/radius tuple conversions.</summary>
    [TestMethod]
    public void GeneratedSphereConversions_Work()
    {
        var value = new Sphere3(new Vec3(1f, 2f, 3f), 4f);
        Sphere3d doubleValue = value;
        var floatValue = (Sphere3)doubleValue;
        (Vec3 center, float radius) = value;
        Sphere3 tupleValue = (center, radius);

        Assert.AreEqual(new Sphere3d(new Vec3d(1d, 2d, 3d), 4d), doubleValue);
        Assert.AreEqual(value, floatValue);
        Assert.AreEqual(value, tupleValue);
        Assert.IsTrue(((Sphere3d)Sphere3.Empty).IsEmpty);
    }

    /// <summary>Generated double-precision spheres mirror the single-precision API.</summary>
    [TestMethod]
    public void GeneratedSphere3dHelpers_Work()
    {
        var sphere = new Sphere3d(new Vec3d(0d, 0d, 0d), 2d);
        var fromBox = Sphere3d.CreateFromBox(Box3d.CreateFromCenterSize(Vec3d.Zero, new Vec3d(2d, 4d, 4d)));

        Assert.IsTrue(sphere.Contains(new Vec3d(0d, 2d, 0d)));
        Assert.AreEqual(new Vec3d(0d, 2d, 0d), sphere.ClosestPoint(new Vec3d(0d, 5d, 0d)));
        Assert.AreEqual(3d, fromBox.Radius);
        Assert.AreEqual(sphere, ParseUtf8<Sphere3d>("((0, 0, 0), 2)"u8));
    }

    /// <summary>Generated sphere interfaces expose the types through static abstract members.</summary>
    [TestMethod]
    public void GeneratedSphereInterfaces_Work()
    {
        var sphere = CreateGeneric<Sphere3, float, Vec3, Box3>(Vec3.Zero, 2f);
        var fromBox = CreateFromBoxGeneric<Sphere3, float, Vec3, Box3>(
            Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(2f)));
        var intersects = IntersectsGeneric<Sphere3, float, Vec3, Box3>(sphere, new Sphere3(new Vec3(3f, 0f, 0f), 1f));

        Assert.AreEqual(new Sphere3(Vec3.Zero, 2f), sphere);
        Assert.AreEqual(new Sphere3(Vec3.Zero, ScalarMath.Sqrt(3f)), fromBox);
        Assert.IsTrue(intersects);
        Assert.IsTrue(ContainsGeneric<Sphere3, float, Vec3, Box3>(sphere, Vec3.UnitX));
    }

    private static TSphere CreateGeneric<TSphere, TScalar, TVector3, TBox3>(TVector3 center, TScalar radius)
        where TSphere : struct, ISphere3<TSphere, TScalar, TVector3, TBox3>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TBox3 : struct, IBox3<TBox3, TScalar, TVector3> =>
        TSphere.Create(center, radius);

    private static TSphere CreateFromBoxGeneric<TSphere, TScalar, TVector3, TBox3>(TBox3 box)
        where TSphere : struct, ISphere3<TSphere, TScalar, TVector3, TBox3>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TBox3 : struct, IBox3<TBox3, TScalar, TVector3> =>
        TSphere.CreateFromBox(box);

    private static bool IntersectsGeneric<TSphere, TScalar, TVector3, TBox3>(TSphere left, TSphere right)
        where TSphere : struct, ISphere3<TSphere, TScalar, TVector3, TBox3>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TBox3 : struct, IBox3<TBox3, TScalar, TVector3> =>
        TSphere.Intersects(left, right);

    private static bool ContainsGeneric<TSphere, TScalar, TVector3, TBox3>(TSphere sphere, TVector3 point)
        where TSphere : struct, ISphere3<TSphere, TScalar, TVector3, TBox3>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TBox3 : struct, IBox3<TBox3, TScalar, TVector3> =>
        sphere.Contains(point);

    private static T ParseUtf8<T>(ReadOnlySpan<byte> text)
        where T : IUtf8SpanParsable<T> =>
        T.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}

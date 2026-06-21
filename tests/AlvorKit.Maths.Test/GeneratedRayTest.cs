namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D ray types.</summary>
[TestClass]
public sealed class GeneratedRayTest
{
    /// <summary>Generated ray core members expose origin, direction, component refs, span copies, and normalization helpers.</summary>
    [TestMethod]
    public void GeneratedRayCoreMembers_Work()
    {
        var ray = new Ray3(new Vec3(1f, 2f, 3f), new Vec3(0f, 0f, 2f));
        Span<float> copied = stackalloc float[Ray3.ComponentCount];
        ray.CopyTo(copied);
        Ray3.ComponentRef(ref ray, 5) = 4f;
        ray[0] = 10f;
        var fromSpan = Ray3.Create(copied);
        var translated = ray.Translated(new Vec3(1f, 0f, -1f));
        var normalized = ray.Normalized();

        Assert.AreEqual(6, Ray3.ComponentCount);
        Assert.AreEqual(24, Ray3.SizeInBytes);
        CollectionAssert.AreEqual(new[] { 1f, 2f, 3f, 0f, 0f, 2f }, copied.ToArray());
        Assert.AreEqual(new Ray3(new Vec3(10f, 2f, 3f), new Vec3(0f, 0f, 4f)), ray);
        Assert.AreEqual(new Ray3(new Vec3(1f, 2f, 3f), new Vec3(0f, 0f, 2f)), fromSpan);
        Assert.AreEqual(new Ray3(new Vec3(11f, 2f, 2f), new Vec3(0f, 0f, 4f)), translated);
        Assert.AreEqual(new Vec3(10f, 2f, 11f), ray.PointAt(2f));
        Assert.AreEqual(Vec3.UnitZ, normalized.Direction);
        Assert.IsTrue(ray.TryNormalize(out var tryNormalized));
        Assert.AreEqual(Vec3.UnitZ, tryNormalized.Direction);
        Assert.IsFalse(new Ray3(Vec3.Zero, Vec3.Zero).TryNormalize(out var zeroNormalized));
        Assert.AreEqual(default, zeroNormalized);
        Assert.ThrowsException<ArgumentException>(() => _ = Ray3.Create(new float[5]));
        Assert.ThrowsException<ArgumentException>(() => ray.CopyTo(new float[5]));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = ray[6]);
    }

    /// <summary>Generated rays intersect planes using the stored direction length without normalizing it.</summary>
    [TestMethod]
    public void GeneratedRayPlaneIntersections_Work()
    {
        var ray = new Ray3(Vec3.Zero, new Vec3(0f, 0f, 2f));
        var plane = Plane3.CreateFromPointNormal(new Vec3(0f, 0f, 10f), Vec3.UnitZ);
        var parallel = Plane3.CreateFromPointNormal(new Vec3(0f, 1f, 0f), Vec3.UnitY);
        var behind = Plane3.CreateFromPointNormal(new Vec3(0f, 0f, -2f), Vec3.UnitZ);

        Assert.IsTrue(ray.Intersects(plane));
        Assert.IsTrue(ray.TryIntersect(plane, out var distance));
        Assert.AreEqual(5f, distance);
        Assert.AreEqual(new Vec3(0f, 0f, 10f), ray.PointAt(distance));
        Assert.IsFalse(ray.TryIntersect(parallel, out _));
        Assert.IsFalse(ray.TryIntersect(behind, out _));
        Assert.IsFalse(new Ray3(Vec3.Zero, Vec3.Zero).Intersects(plane));
    }

    /// <summary>Generated rays intersect boxes through nearest distances and clipped nonnegative intervals.</summary>
    [TestMethod]
    public void GeneratedRayBoxIntersections_Work()
    {
        var ray = new Ray3(Vec3.Zero, new Vec3(2f, 0f, 0f));
        var box = new Box3(new Vec3(5f, -1f, -1f), new Vec3(7f, 1f, 1f));
        var inside = new Ray3(new Vec3(6f, 0f, 0f), new Vec3(2f, 0f, 0f));
        var parallelMiss = new Ray3(new Vec3(0f, 2f, 0f), new Vec3(2f, 0f, 0f));

        Assert.IsTrue(ray.Intersects(box));
        Assert.IsTrue(ray.TryIntersect(box, out float distance));
        Assert.IsTrue(ray.TryIntersect(box, out Intervalf distances));
        Assert.AreEqual(2.5f, distance);
        Assert.AreEqual(new Intervalf(2.5f, 3.5f), distances);
        Assert.IsTrue(inside.TryIntersect(box, out distance));
        Assert.AreEqual(0f, distance);
        Assert.IsFalse(parallelMiss.Intersects(box));
        Assert.IsFalse(new Ray3(Vec3.Zero, Vec3.Zero).Intersects(new Box3(new Vec3(-1f), new Vec3(1f))));
    }

    /// <summary>Generated rays intersect spheres through nearest nonnegative distances.</summary>
    [TestMethod]
    public void GeneratedRaySphereIntersections_Work()
    {
        var ray = new Ray3(Vec3.Zero, new Vec3(2f, 0f, 0f));
        var sphere = new Sphere3(new Vec3(10f, 0f, 0f), 2f);
        var inside = new Ray3(new Vec3(10f, 0f, 0f), new Vec3(2f, 0f, 0f));
        var miss = new Sphere3(new Vec3(0f, 5f, 0f), 1f);

        Assert.IsTrue(ray.Intersects(sphere));
        Assert.IsTrue(ray.TryIntersect(sphere, out var distance));
        Assert.AreEqual(4f, distance);
        Assert.AreEqual(new Vec3(8f, 0f, 0f), ray.PointAt(distance));
        Assert.IsTrue(inside.TryIntersect(sphere, out distance));
        Assert.AreEqual(0f, distance);
        Assert.IsFalse(ray.Intersects(miss));
        Assert.IsFalse(ray.Intersects(Sphere3.Empty));
        Assert.IsFalse(new Ray3(Vec3.Zero, Vec3.Zero).Intersects(sphere));
    }

    /// <summary>Generated rays intersect frustums through clipped nonnegative distance intervals.</summary>
    [TestMethod]
    public void GeneratedRayFrustumIntersections_Work()
    {
        var frustum = Frustum3.CreateFromClipTransform(Mat4.Identity);
        var ray = new Ray3(new Vec3(0f, 0f, -2f), new Vec3(0f, 0f, 2f));
        var inside = new Ray3(Vec3.Zero, Vec3.UnitZ);
        var miss = new Ray3(new Vec3(2f, 0f, 0f), Vec3.UnitX);

        Assert.IsTrue(ray.Intersects(frustum));
        Assert.IsTrue(ray.TryIntersect(frustum, out var distances));
        Assert.AreEqual(new Intervalf(0.5f, 1.5f), distances);
        Assert.IsTrue(inside.TryIntersect(frustum, out distances));
        Assert.AreEqual(new Intervalf(0f, 1f), distances);
        Assert.IsFalse(miss.Intersects(frustum));
        Assert.IsFalse(new Ray3(Vec3.Zero, Vec3.Zero).Intersects(frustum));
    }

    /// <summary>Generated ray formatting and parsing mirror box-style nested component text.</summary>
    [TestMethod]
    public void GeneratedRayFormattingAndParsing_UsesVectorStyle()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;
        var value = new Ray3(new Vec3(1f, 2f, 3f), new Vec3(0f, 0f, 2f));
        Span<char> destination = stackalloc char[26];
        Span<byte> utf8Destination = stackalloc byte[26];

        Assert.IsTrue(value.TryFormat(destination, out var charsWritten, default, formatProvider));
        Assert.AreEqual("((1, 2, 3), (0, 0, 2))", destination[..charsWritten].ToString());
        Assert.IsTrue(value.TryFormat(utf8Destination, out var bytesWritten, default, formatProvider));
        Assert.AreEqual("((1, 2, 3), (0, 0, 2))", Encoding.UTF8.GetString(utf8Destination[..bytesWritten]));
        Assert.AreEqual("((1.0, 2.0, 3.0), (0.0, 0.0, 2.0))", value.ToString("0.0", formatProvider));
        Assert.AreEqual(value, Ray3.Parse("((1, 2, 3), (0, 0, 2))", formatProvider));
        Assert.AreEqual(value, ParseUtf8<Ray3>("((1, 2, 3), (0, 0, 2))"u8));
        Assert.IsFalse(Ray3.TryParse((string?)null, formatProvider, out _));
        Assert.IsFalse(Ray3.TryParse("not a ray", formatProvider, out _));
    }

    /// <summary>Generated rays convert across scalar families and support origin/direction tuple conversions.</summary>
    [TestMethod]
    public void GeneratedRayConversions_Work()
    {
        var value = new Ray3(new Vec3(1f, 2f, 3f), Vec3.UnitX);
        Ray3d doubleValue = value;
        var floatValue = (Ray3)doubleValue;
        (Vec3 origin, Vec3 direction) = value;
        Ray3 tupleValue = (origin, direction);

        Assert.AreEqual(new Ray3d(new Vec3d(1d, 2d, 3d), Vec3d.UnitX), doubleValue);
        Assert.AreEqual(value, floatValue);
        Assert.AreEqual(value, tupleValue);
    }

    /// <summary>Generated double-precision rays mirror the single-precision API.</summary>
    [TestMethod]
    public void GeneratedRay3dHelpers_Work()
    {
        var ray = new Ray3d(Vec3d.Zero, new Vec3d(0d, 2d, 0d));
        var plane = Plane3d.CreateFromPointNormal(new Vec3d(0d, 8d, 0d), Vec3d.UnitY);
        var box = new Box3d(new Vec3d(-1d, 4d, -1d), new Vec3d(1d, 6d, 1d));
        var frustum = Frustum3d.CreateFromClipTransform(Mat4d.Identity);

        Assert.IsTrue(ray.TryIntersect(plane, out var planeDistance));
        Assert.AreEqual(4d, planeDistance);
        Assert.IsTrue(ray.TryIntersect(box, out Intervald distances));
        Assert.AreEqual(new Intervald(2d, 3d), distances);
        Assert.IsTrue(ray.TryIntersect(frustum, out distances));
        Assert.AreEqual(new Intervald(0d, 0.5d), distances);
        Assert.AreEqual(ray, ParseUtf8<Ray3d>("((0, 0, 0), (0, 2, 0))"u8));
    }

    /// <summary>Generated ray interfaces expose the types through static abstract members.</summary>
    [TestMethod]
    public void GeneratedRayInterfaces_Work()
    {
        var ray = CreateGeneric<Ray3, float, Vec3, Vec4, Plane3, Box3, Sphere3, Frustum3, Intervalf>(
            Vec3.Zero,
            Vec3.UnitX);
        var plane = Plane3.CreateFromPointNormal(new Vec3(2f, 0f, 0f), Vec3.UnitX);
        var box = new Box3(new Vec3(2f, -1f, -1f), new Vec3(3f, 1f, 1f));
        var frustum = Frustum3.CreateFromClipTransform(Mat4.Identity);

        Assert.AreEqual(new Ray3(Vec3.Zero, Vec3.UnitX), ray);
        Assert.IsTrue(PlaneIntersectGeneric<Ray3, float, Vec3, Vec4, Plane3, Box3, Sphere3, Frustum3, Intervalf>(
            ray,
            plane,
            out var distance));
        Assert.AreEqual(2f, distance);
        Assert.IsTrue(BoxIntersectGeneric<Ray3, float, Vec3, Vec4, Plane3, Box3, Sphere3, Frustum3, Intervalf>(
            ray,
            box,
            out var distances));
        Assert.AreEqual(new Intervalf(2f, 3f), distances);
        Assert.IsTrue(FrustumIntersectGeneric<Ray3, float, Vec3, Vec4, Plane3, Box3, Sphere3, Frustum3, Intervalf>(
            ray,
            frustum,
            out distances));
        Assert.AreEqual(new Intervalf(0f, 1f), distances);
    }

    private static TRay CreateGeneric<TRay, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>(
        TVector3 origin,
        TVector3 direction)
        where TRay : struct, IRay3<TRay, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar>
        where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
        where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
        where TSphere3 : struct, ISphere3<TSphere3, TScalar, TVector3, TBox3>
        where TFrustum3 : struct, IFrustum3<TFrustum3, TScalar, TVector3, TVector4, TPlane3, TBox3>
        where TInterval : struct, IInterval<TInterval, TScalar> =>
        TRay.Create(origin, direction);

    private static bool PlaneIntersectGeneric<TRay, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>(
        TRay ray,
        TPlane3 plane,
        out TScalar distance)
        where TRay : struct, IRay3<TRay, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar>
        where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
        where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
        where TSphere3 : struct, ISphere3<TSphere3, TScalar, TVector3, TBox3>
        where TFrustum3 : struct, IFrustum3<TFrustum3, TScalar, TVector3, TVector4, TPlane3, TBox3>
        where TInterval : struct, IInterval<TInterval, TScalar> =>
        ray.TryIntersect(plane, out distance);

    private static bool BoxIntersectGeneric<TRay, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>(
        TRay ray,
        TBox3 box,
        out TInterval distances)
        where TRay : struct, IRay3<TRay, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar>
        where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
        where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
        where TSphere3 : struct, ISphere3<TSphere3, TScalar, TVector3, TBox3>
        where TFrustum3 : struct, IFrustum3<TFrustum3, TScalar, TVector3, TVector4, TPlane3, TBox3>
        where TInterval : struct, IInterval<TInterval, TScalar> =>
        ray.TryIntersect(box, out distances);

    private static bool FrustumIntersectGeneric<TRay, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>(
        TRay ray,
        TFrustum3 frustum,
        out TInterval distances)
        where TRay : struct, IRay3<TRay, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar>
        where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
        where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
        where TSphere3 : struct, ISphere3<TSphere3, TScalar, TVector3, TBox3>
        where TFrustum3 : struct, IFrustum3<TFrustum3, TScalar, TVector3, TVector4, TPlane3, TBox3>
        where TInterval : struct, IInterval<TInterval, TScalar> =>
        ray.TryIntersect(frustum, out distances);

    private static T ParseUtf8<T>(ReadOnlySpan<byte> text)
        where T : IUtf8SpanParsable<T> =>
        T.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}

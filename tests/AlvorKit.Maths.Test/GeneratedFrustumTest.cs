namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D frustum types.</summary>
[TestClass]
public sealed class GeneratedFrustumTest
{
    /// <summary>Generated frustum core members expose planes, component refs, and span copies.</summary>
    [TestMethod]
    public void GeneratedFrustumCoreMembers_Work()
    {
        var frustum = SampleFrustum();
        Span<float> copied = stackalloc float[Frustum3.ComponentCount];
        Span<Plane3> planes = stackalloc Plane3[Frustum3.PlaneCount];
        frustum.CopyTo(copied);
        frustum.CopyPlanesTo(planes);
        Frustum3.ComponentRef(ref frustum, 0) = 2f;
        frustum[23] = 12f;
        var fromSpan = Frustum3.Create(copied);

        Assert.AreEqual(6, Frustum3.PlaneCount);
        Assert.AreEqual(8, Frustum3.CornerCount);
        Assert.AreEqual(24, Frustum3.ComponentCount);
        Assert.AreEqual(96, Frustum3.SizeInBytes);
        CollectionAssert.AreEqual(
            new[] { 1f, 0f, 0f, 1f, -1f, 0f, 0f, 2f, 0f, 1f, 0f, 3f, 0f, -1f, 0f, 4f, 0f, 0f, 1f, 5f, 0f, 0f, -1f, 6f },
            copied.ToArray());
        Assert.AreEqual(new Plane3(new Vec3(2f, 0f, 0f), 1f), frustum.Left);
        Assert.AreEqual(new Plane3(new Vec3(0f, 0f, -1f), 12f), frustum.Far);
        Assert.AreEqual(SampleFrustum(), fromSpan);
        Assert.AreEqual(SampleFrustum().Near, planes[4]);
        Assert.ThrowsException<ArgumentException>(() => _ = Frustum3.Create(new float[23]));
        Assert.ThrowsException<ArgumentException>(() => frustum.CopyTo(new float[23]));
        Assert.ThrowsException<ArgumentException>(() => frustum.CopyPlanesTo(new Plane3[5]));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = frustum[24]);
    }

    /// <summary>Generated frustum clip extraction follows AlvorKit row-based column-vector matrix conventions.</summary>
    [TestMethod]
    public void GeneratedFrustumClipExtraction_UsesDepthRange()
    {
        var defaultFrustum = Frustum3.CreateFromClipTransform(Mat4.Identity);
        var zeroToOneFrustum = Frustum3.CreateFromClipTransform(Mat4.Identity, ProjectionDepthRange.ZeroToOne);
        var projection = Mat4.CreatePerspectiveFieldOfView(float.Pi / 2f, 1f, 1f, 10f);
        var perspectiveFrustum = Frustum3.CreateFromClipTransform(projection);

        Assert.IsTrue(defaultFrustum.Contains(new Vec3(0f, 0f, -1f)));
        Assert.IsFalse(defaultFrustum.Contains(new Vec3(0f, 0f, -1.1f)));
        Assert.IsTrue(zeroToOneFrustum.Contains(new Vec3(0f, 0f, 0f)));
        Assert.IsFalse(zeroToOneFrustum.Contains(new Vec3(0f, 0f, -0.1f)));
        Assert.IsTrue(perspectiveFrustum.Contains(new Vec3(0f, 0f, -5f)));
        Assert.IsFalse(perspectiveFrustum.Contains(new Vec3(0f, 0f, 0f)));
        Assert.IsFalse(perspectiveFrustum.Contains(new Vec3(0f, 0f, -11f)));
        Assert.IsFalse(Frustum3.TryCreateFromClipTransform(Mat4.Identity, (ProjectionDepthRange)99, out _));
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => Frustum3.CreateFromClipTransform(Mat4.Identity, (ProjectionDepthRange)99));
    }

    /// <summary>Generated frustum box queries distinguish disjoint, intersecting, and contained boxes.</summary>
    [TestMethod]
    public void GeneratedFrustumBoxQueries_Work()
    {
        var frustum = Frustum3.CreateFromClipTransform(Mat4.Identity);
        var contained = new Box3(new Vec3(-0.5f), new Vec3(0.5f));
        var intersecting = new Box3(new Vec3(0.5f, -0.5f, -0.5f), new Vec3(1.5f, 0.5f, 0.5f));
        var disjoint = new Box3(new Vec3(2f), new Vec3(3f));

        Assert.AreEqual(ContainmentKind.Contains, frustum.Classify(contained));
        Assert.AreEqual(ContainmentKind.Intersects, frustum.Classify(intersecting));
        Assert.AreEqual(ContainmentKind.Disjoint, frustum.Classify(disjoint));
        Assert.AreEqual(ContainmentKind.Disjoint, frustum.Classify(Box3.Empty));
        Assert.IsTrue(frustum.Contains(contained));
        Assert.IsFalse(frustum.Contains(intersecting));
        Assert.IsTrue(frustum.Intersects(intersecting));
        Assert.IsFalse(frustum.Intersects(disjoint));
    }

    /// <summary>Generated frustum corner helpers reconstruct finite corners and reject infinite frustums.</summary>
    [TestMethod]
    public void GeneratedFrustumCorners_Work()
    {
        var frustum = Frustum3.CreateFromClipTransform(Mat4.Identity);
        Span<Vec3> corners = stackalloc Vec3[Frustum3.CornerCount];
        frustum.CopyCornersTo(corners);
        var infinite = Frustum3.CreateFromClipTransform(Mat4.CreateInfinitePerspective(float.Pi / 2f, 1f, 1f));

        AssertVecClose(new Vec3(-1f, -1f, -1f), corners[0]);
        AssertVecClose(new Vec3(1f, -1f, -1f), corners[1]);
        AssertVecClose(new Vec3(-1f, 1f, -1f), corners[2]);
        AssertVecClose(new Vec3(1f, 1f, -1f), corners[3]);
        AssertVecClose(new Vec3(-1f, -1f, 1f), corners[4]);
        AssertVecClose(new Vec3(1f, -1f, 1f), corners[5]);
        AssertVecClose(new Vec3(-1f, 1f, 1f), corners[6]);
        AssertVecClose(new Vec3(1f, 1f, 1f), corners[7]);
        Assert.IsTrue(infinite.Contains(new Vec3(0f, 0f, -1000f)));
        Assert.IsFalse(infinite.TryCopyCornersTo(corners));
        Assert.ThrowsException<InvalidOperationException>(() => infinite.CopyCornersTo(new Vec3[8]));
        Assert.ThrowsException<ArgumentException>(() => frustum.CopyCornersTo(new Vec3[7]));
    }

    /// <summary>Generated frustum transforms use inverse-transpose matrix semantics for every plane.</summary>
    [TestMethod]
    public void GeneratedFrustumTransforms_Work()
    {
        var frustum = Frustum3.CreateFromClipTransform(Mat4.Identity);
        var translated = Frustum3.Transform(frustum, Mat4.CreateTranslation(new Vec3(2f, 0f, 0f)));

        Assert.IsTrue(translated.Contains(new Vec3(2f, 0f, 0f)));
        Assert.IsFalse(translated.Contains(Vec3.Zero));
        Assert.IsFalse(Frustum3.TryTransform(frustum, Mat4.CreateScale(new Vec3(1f, 0f, 1f)), out _));
        Assert.ThrowsException<InvalidOperationException>(() => Frustum3.Transform(frustum, Mat4.CreateScale(new Vec3(1f, 0f, 1f))));
    }

    /// <summary>Generated frustum formatting and parsing mirror nested plane text.</summary>
    [TestMethod]
    public void GeneratedFrustumFormattingAndParsing_UsesPlaneStyle()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;
        var value = Frustum3.CreateFromClipTransform(Mat4.Identity);
        var expectedText =
            "((1, 0, 0, 1), (-1, 0, 0, 1), (0, 1, 0, 1), " +
            "(0, -1, 0, 1), (0, 0, 1, 1), (0, 0, -1, 1))";
        Span<char> destination = stackalloc char[160];
        Span<byte> utf8Destination = stackalloc byte[160];

        Assert.IsTrue(value.TryFormat(destination, out var charsWritten, default, formatProvider));
        Assert.AreEqual(expectedText, destination[..charsWritten].ToString());
        Assert.IsTrue(value.TryFormat(utf8Destination, out var bytesWritten, default, formatProvider));
        Assert.AreEqual(expectedText, System.Text.Encoding.UTF8.GetString(utf8Destination[..bytesWritten]));
        Assert.AreEqual(value, Frustum3.Parse(expectedText, formatProvider));
        Assert.AreEqual(value, Frustum3.Parse(System.Text.Encoding.UTF8.GetBytes(expectedText), formatProvider));
        Assert.IsFalse(Frustum3.TryParse((string?)null, formatProvider, out _));
        Assert.IsFalse(Frustum3.TryParse("not a frustum", formatProvider, out _));
    }

    /// <summary>Generated frustum conversions work across scalar families and six-plane tuples.</summary>
    [TestMethod]
    public void GeneratedFrustumConversions_Work()
    {
        var value = Frustum3.CreateFromClipTransform(Mat4.Identity);
        Frustum3d doubleValue = value;
        var floatValue = (Frustum3)doubleValue;
        (Plane3 left, Plane3 right, Plane3 bottom, Plane3 top, Plane3 near, Plane3 far) = value;
        Frustum3 tupleValue = (left, right, bottom, top, near, far);

        Assert.AreEqual(value, floatValue);
        Assert.AreEqual(new Plane3d(new Vec3d(1d, 0d, 0d), 1d), doubleValue.Left);
        Assert.AreEqual(value, tupleValue);
    }

    /// <summary>Generated double-precision frustums mirror the single-precision API.</summary>
    [TestMethod]
    public void GeneratedFrustum3dHelpers_Work()
    {
        var frustum = Frustum3d.CreateFromClipTransform(Mat4d.Identity);
        Span<Vec3d> corners = stackalloc Vec3d[Frustum3d.CornerCount];

        Assert.IsTrue(frustum.Contains(Vec3d.Zero));
        Assert.IsTrue(frustum.TryCopyCornersTo(corners));
        AssertVecClose(new Vec3d(-1d, -1d, -1d), corners[0]);
        Assert.AreEqual(frustum, Frustum3d.Parse(frustum.ToString(System.Globalization.CultureInfo.InvariantCulture), null));
    }

    /// <summary>Generic frustum interfaces expose construction and point queries through static abstract members.</summary>
    [TestMethod]
    public void GeneratedFrustumInterfaces_Work()
    {
        Assert.IsTrue(ContainsGeneric<Frustum3, float, Vec3, Vec4, Mat4, Plane3, Box3>(Mat4.Identity, Vec3.Zero));
    }

    private static Frustum3 SampleFrustum() =>
        new(
            new Plane3(new Vec3(1f, 0f, 0f), 1f),
            new Plane3(new Vec3(-1f, 0f, 0f), 2f),
            new Plane3(new Vec3(0f, 1f, 0f), 3f),
            new Plane3(new Vec3(0f, -1f, 0f), 4f),
            new Plane3(new Vec3(0f, 0f, 1f), 5f),
            new Plane3(new Vec3(0f, 0f, -1f), 6f));

    private static bool ContainsGeneric<TFrustum, TScalar, TVector3, TVector4, TMatrix4, TPlane3, TBox3>(
        TMatrix4 clipFromSource,
        TVector3 point)
        where TFrustum : struct, IFrustum3Transform<TFrustum, TScalar, TVector3, TVector4, TMatrix4, TPlane3, TBox3>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar>
        where TMatrix4 : struct
        where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
        where TBox3 : struct, IBox3<TBox3, TScalar, TVector3> =>
        TFrustum.CreateFromClipTransform(clipFromSource).Contains(point);

    private static void AssertVecClose(Vec3 expected, Vec3 actual)
    {
        for (var index = 0; index < Vec3.ComponentCount; index++)
            AssertClose(expected[index], actual[index]);
    }

    private static void AssertVecClose(Vec3d expected, Vec3d actual)
    {
        for (var index = 0; index < Vec3d.ComponentCount; index++)
            AssertClose(expected[index], actual[index]);
    }

    private static void AssertClose(float expected, float actual) =>
        Assert.AreEqual(expected, actual, 0.0001f);

    private static void AssertClose(double expected, double actual) =>
        Assert.AreEqual(expected, actual, 0.0001d);
}

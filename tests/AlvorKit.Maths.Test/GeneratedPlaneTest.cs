namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D plane types.</summary>
[TestClass]
public sealed class GeneratedPlaneTest
{
    /// <summary>Generated plane core members expose coefficients, component refs, and span copies.</summary>
    [TestMethod]
    public void GeneratedPlaneCoreMembers_Work()
    {
        var plane = new Plane3(new Vec3(0f, 2f, 0f), -4f);
        Span<float> copied = stackalloc float[Plane3.ComponentCount];
        plane.CopyTo(copied);
        Plane3.ComponentRef(ref plane, 0) = 1f;
        plane[3] = -5f;
        var fromSpan = Plane3.Create(copied);

        Assert.AreEqual(4, Plane3.ComponentCount);
        Assert.AreEqual(16, Plane3.SizeInBytes);
        CollectionAssert.AreEqual(new[] { 0f, 2f, 0f, -4f }, copied.ToArray());
        Assert.AreEqual(new Plane3(new Vec3(1f, 2f, 0f), -5f), plane);
        Assert.AreEqual(new Plane3(new Vec4(0f, 2f, 0f, -4f)), fromSpan);
        Assert.AreEqual(new Vec4(1f, 2f, 0f, -5f), plane.Coefficients);
        Assert.ThrowsException<ArgumentException>(() => _ = Plane3.Create(new float[3]));
        Assert.ThrowsException<ArgumentException>(() => plane.CopyTo(new float[3]));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = plane[4]);
    }

    /// <summary>Generated plane spatial helpers evaluate, measure, project, and reflect points.</summary>
    [TestMethod]
    public void GeneratedPlaneSpatialHelpers_Work()
    {
        var plane = Plane3.CreateFromPointNormal(new Vec3(0f, 2f, 0f), Vec3.UnitY);
        var scaled = new Plane3(new Vec3(0f, 2f, 0f), -4f);
        var normalized = scaled.Normalized;
        var point = new Vec3(1f, 5f, 3f);
        var fromPoints = Plane3.CreateFromPoints(Vec3.Zero, Vec3.UnitZ, Vec3.UnitX);

        Assert.AreEqual(3f, plane.Evaluate(point));
        Assert.AreEqual(3f, plane.SignedDistanceTo(point));
        Assert.AreEqual(3f, plane.DistanceTo(new Vec3(0f, -1f, 0f)));
        AssertVecClose(new Vec3(1f, 2f, 3f), plane.ProjectPoint(point));
        AssertVecClose(new Vec3(1f, -1f, 3f), plane.ReflectPoint(point));
        AssertVecClose(new Vec3(0f, 1f, 0f), normalized.Normal);
        Assert.AreEqual(-2f, normalized.Offset);
        Assert.IsTrue(normalized.IsNormalized(0f));
        Assert.AreEqual(-3f, plane.Flipped.Evaluate(point));
        AssertVecClose(Vec3.UnitY, fromPoints.Normal);
        Assert.IsTrue(Plane3.TryCreateFromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, out var valid));
        Assert.IsTrue(valid.IsNormalized(0.0001f));
        Assert.IsFalse(Plane3.TryCreateFromPoints(Vec3.Zero, Vec3.UnitX, new Vec3(2f, 0f, 0f), out _));
        Assert.ThrowsException<InvalidOperationException>(() => Plane3.CreateFromPoints(Vec3.Zero, Vec3.UnitX, new Vec3(2f, 0f, 0f)));
        Assert.IsFalse(Plane3.Zero.TryNormalize(out _));
        Assert.AreEqual(plane, Plane3.Zero.NormalizedOr(plane));
    }

    /// <summary>Generated plane classification uses exact signs and treats touching shapes as intersecting.</summary>
    [TestMethod]
    public void GeneratedPlaneClassification_Work()
    {
        var plane = Plane3.CreateFromPointNormal(new Vec3(0f, 2f, 0f), Vec3.UnitY);
        var scaled = new Plane3(new Vec3(0f, 2f, 0f), -4f);

        Assert.AreEqual(PlaneIntersectionKind.Negative, plane.Classify(new Vec3(0f, 1f, 0f)));
        Assert.AreEqual(PlaneIntersectionKind.Intersecting, plane.Classify(new Vec3(0f, 2f, 0f)));
        Assert.AreEqual(PlaneIntersectionKind.Positive, Plane3.Classify(plane, new Vec3(0f, 3f, 0f)));
        Assert.AreEqual(PlaneIntersectionKind.Negative, plane.Classify(new Box3(new Vec3(-1f, 0f, -1f), new Vec3(1f, 1f, 1f))));
        Assert.AreEqual(PlaneIntersectionKind.Positive, plane.Classify(new Box3(new Vec3(-1f, 3f, -1f), new Vec3(1f, 4f, 1f))));
        Assert.AreEqual(PlaneIntersectionKind.Intersecting, plane.Classify(new Box3(new Vec3(-1f, 1f, -1f), new Vec3(1f, 3f, 1f))));
        Assert.AreEqual(PlaneIntersectionKind.Intersecting, plane.Classify(new Box3(new Vec3(-1f, 2f, -1f), new Vec3(1f, 3f, 1f))));
        Assert.AreEqual(PlaneIntersectionKind.Intersecting, plane.Classify(Box3.Empty));
        Assert.AreEqual(PlaneIntersectionKind.Negative, scaled.Classify(new Sphere3(new Vec3(0f, 0f, 0f), 0.5f)));
        Assert.AreEqual(PlaneIntersectionKind.Intersecting, scaled.Classify(new Sphere3(new Vec3(0f, 1f, 0f), 1f)));
        Assert.AreEqual(PlaneIntersectionKind.Positive, scaled.Classify(new Sphere3(new Vec3(0f, 5f, 0f), 1f)));
        Assert.AreEqual(PlaneIntersectionKind.Intersecting, scaled.Classify(Sphere3.Empty));
    }

    /// <summary>Generated plane formatting and parsing use the same coefficient text as four-component vectors.</summary>
    [TestMethod]
    public void GeneratedPlaneFormattingAndParsing_UsesVectorStyle()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;
        var value = new Plane3(new Vec3(1f, 2f, 3f), 4.5f);
        Span<char> destination = stackalloc char[19];
        Span<byte> utf8Destination = stackalloc byte[19];

        Assert.IsTrue(value.TryFormat(destination, out var charsWritten, default, formatProvider));
        Assert.AreEqual("(1, 2, 3, 4.5)", destination[..charsWritten].ToString());
        Assert.IsTrue(value.TryFormat(utf8Destination, out var bytesWritten, default, formatProvider));
        Assert.AreEqual("(1, 2, 3, 4.5)", System.Text.Encoding.UTF8.GetString(utf8Destination[..bytesWritten]));
        Assert.AreEqual("(1.0, 2.0, 3.0, 4.5)", value.ToString("0.0", formatProvider));
        Assert.AreEqual(value, Plane3.Parse("(1, 2, 3, 4.5)", formatProvider));
        Assert.AreEqual(value, ParseUtf8<Plane3>("(1, 2, 3, 4.5)"u8));
        Assert.IsFalse(Plane3.TryParse((string?)null, formatProvider, out _));
        Assert.IsFalse(Plane3.TryParse("not a plane", formatProvider, out _));
    }

    /// <summary>Generated plane transforms follow inverse-transpose matrix semantics and quaternion normal rotation.</summary>
    [TestMethod]
    public void GeneratedPlaneTransforms_Work()
    {
        var plane = Plane3.CreateFromPointNormal(new Vec3(0f, 2f, 0f), Vec3.UnitY);
        var translated = Plane3.Transform(plane, Mat4.CreateTranslation(new Vec3(0f, 3f, 0f)));
        var rotated = Plane3.Transform(
            new Plane3(Vec3.UnitX, 0f),
            Quat.CreateFromAxisAngle(Vec3.UnitZ, float.Pi / 2f));
        var reflection = Mat4.CreateReflection(plane);
        var shadow = Mat4.CreateShadow(new Vec3(0f, -1f, 0f), plane);
        var reflected = Mat4.TransformPoint(reflection, new Vec3(1f, 5f, 3f));
        var shadowed = Mat4.TransformPoint(shadow, new Vec3(1f, 5f, 3f));

        AssertClose(0f, translated.Evaluate(new Vec3(0f, 5f, 0f)));
        AssertVecClose(Vec3.UnitY, rotated.Normal);
        AssertVecClose(new Vec3(1f, -1f, 3f), reflected);
        AssertVecClose(new Vec3(1f, 2f, 3f), shadowed);
        Assert.IsFalse(Plane3.TryTransform(plane, Mat4.CreateScale(new Vec3(1f, 0f, 1f)), out _));
        AssertMatrixClose(reflection, CreateReflectionGeneric<Mat4, float, Vec3, Vec4, Plane3>(plane));
    }

    /// <summary>Generated plane conversions work across scalar families and System.Numerics.</summary>
    [TestMethod]
    public void GeneratedPlaneConversions_Work()
    {
        var value = new Plane3(new Vec3(1f, 2f, 3f), 4f);
        var system = (System.Numerics.Plane)value;
        Plane3d doubleValue = value;
        var floatValue = (Plane3)doubleValue;
        (Vec3 normal, float offset) = value;
        Plane3 tupleValue = (normal, offset);

        Assert.AreEqual(new System.Numerics.Vector3(1f, 2f, 3f), system.Normal);
        Assert.AreEqual(4f, system.D);
        Assert.AreEqual(value, (Plane3)system);
        Assert.AreEqual(new Plane3d(new Vec3d(1d, 2d, 3d), 4d), doubleValue);
        Assert.AreEqual(value, floatValue);
        Assert.AreEqual(value, tupleValue);
    }

    /// <summary>Generated double-precision planes mirror the single-precision API.</summary>
    [TestMethod]
    public void GeneratedPlane3dHelpers_Work()
    {
        var plane = Plane3d.CreateFromPointNormal(new Vec3d(0d, 2d, 0d), Vec3d.UnitY);
        var reflection = Mat4d.CreateReflection(plane);
        var reflected = Mat4d.TransformPoint(reflection, new Vec3d(1d, 5d, 3d));
        var translated = Plane3d.Transform(plane, Mat4d.CreateTranslation(new Vec3d(0d, 3d, 0d)));

        AssertVecClose(new Vec3d(1d, -1d, 3d), reflected);
        AssertClose(0d, translated.Evaluate(new Vec3d(0d, 5d, 0d)));
        Assert.AreEqual(PlaneIntersectionKind.Positive, plane.Classify(new Box3d(new Vec3d(-1d, 3d, -1d), new Vec3d(1d, 4d, 1d))));
        Assert.AreEqual(PlaneIntersectionKind.Intersecting, plane.Classify(new Sphere3d(new Vec3d(0d, 2d, 0d), 0d)));
        Assert.AreEqual(plane, ParseUtf8<Plane3d>("(0, 1, 0, -2)"u8));
    }

    /// <summary>Generic plane interfaces expose the types through static abstract members.</summary>
    [TestMethod]
    public void GeneratedPlaneInterfaces_Work()
    {
        var plane = CreateGeneric<Plane3, float, Vec3, Vec4>(Vec3.UnitY, -2f);
        var normalized = NormalizeGeneric<Plane3, float, Vec3, Vec4>(new Plane3(new Vec3(0f, 2f, 0f), -4f));
        var reflection = CreateReflectionGeneric<Mat4, float, Vec3, Vec4, Plane3>(plane);

        Assert.AreEqual(0f, plane.Evaluate(new Vec3(0f, 2f, 0f)));
        Assert.AreEqual(plane, normalized);
        Assert.AreEqual(PlaneIntersectionKind.Positive, ClassifyGeneric<Plane3, float, Vec3, Vec4>(plane, new Vec3(0f, 3f, 0f)));
        AssertVecClose(new Vec3(0f, -1f, 0f), Mat4.TransformPoint(reflection, new Vec3(0f, 5f, 0f)));
    }

    private static TPlane CreateGeneric<TPlane, TScalar, TVector3, TVector4>(TVector3 normal, TScalar offset)
        where TPlane : struct, IPlane3<TPlane, TScalar, TVector3, TVector4>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar> =>
        TPlane.Create(normal, offset);

    private static TPlane NormalizeGeneric<TPlane, TScalar, TVector3, TVector4>(TPlane value)
        where TPlane : struct, IPlane3<TPlane, TScalar, TVector3, TVector4>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar> =>
        TPlane.Normalize(value);

    private static PlaneIntersectionKind ClassifyGeneric<TPlane, TScalar, TVector3, TVector4>(TPlane plane, TVector3 point)
        where TPlane : struct, IPlane3<TPlane, TScalar, TVector3, TVector4>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar> =>
        TPlane.Classify(plane, point);

    private static TMatrix CreateReflectionGeneric<TMatrix, TScalar, TVector3, TVector4, TPlane>(TPlane plane)
        where TMatrix : struct, IMat4PlaneTransform<TMatrix, TScalar, TVector3, TVector4, TPlane>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar>
        where TPlane : struct, IPlane3<TPlane, TScalar, TVector3, TVector4> =>
        TMatrix.CreateReflection(plane);

    private static T ParseUtf8<T>(ReadOnlySpan<byte> text)
        where T : IUtf8SpanParsable<T> =>
        T.Parse(text, System.Globalization.CultureInfo.InvariantCulture);

    private static void AssertMatrixClose(Mat4 expected, Mat4 actual)
    {
        for (var column = 0; column < Mat4.ColumnCount; column++)
        {
            for (var row = 0; row < Mat4.RowCount; row++)
                AssertClose(expected[column, row], actual[column, row]);
        }
    }

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

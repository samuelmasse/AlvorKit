namespace AlvorKit.Maths.Test;

/// <summary>Tests generated quaternion rotation, formatting, and interop helpers.</summary>
[TestClass]
public sealed class GeneratedQuaternionTest
{
    /// <summary>Generated quaternion core members expose components, identity values, and span copies.</summary>
    [TestMethod]
    public void GeneratedQuaternionCoreMembers_Work()
    {
        var value = new Quat(1f, 2f, 3f, 4f);
        Span<float> copied = stackalloc float[Quat.ComponentCount];
        value.CopyTo(copied);
        Quat.ComponentRef(ref value, 2) = 9f;
        value.Vector = new Vec3(5f, 6f, 7f);
        value.Scalar = 8f;

        Assert.AreEqual(4, Quat.ComponentCount);
        Assert.AreEqual(16, Quat.SizeInBytes);
        CollectionAssert.AreEqual(new[] { 1f, 2f, 3f, 4f }, copied.ToArray());
        Assert.AreEqual(new Quat(5f, 6f, 7f, 8f), value);
        Assert.AreEqual(7f, value[2]);
        Assert.AreEqual(Quat.Zero, AdditiveIdentity<Quat>());
        Assert.AreEqual(Quat.Identity, MultiplicativeIdentity<Quat>());
    }

    /// <summary>Generated quaternion rotations match the existing column-major matrix convention.</summary>
    [TestMethod]
    public void GeneratedQuaternionRotationHelpers_MatchMatrixConvention()
    {
        var rotation = Quat.CreateFromAxisAngle(Vec3.UnitZ, float.Pi / 2f);
        var rotated = rotation * Vec3.UnitX;
        var matrixRotated = rotation.ToMat4() * new Vec4(Vec3.UnitX, 1f);
        var existingMatrixRotated = Mat4.CreateRotation(float.Pi / 2f, Vec3.UnitZ) * new Vec4(Vec3.UnitX, 1f);
        var roundTrip = Quat.CreateFromRotationMatrix(rotation.ToMat3());
        var fromTo = Quat.CreateRotationBetween(Vec3.UnitX, Vec3.UnitY);
        var lookRightHanded = Quat.LookRotation(-Vec3.UnitZ, Vec3.UnitY, ProjectionHandedness.Right);
        var lookLeftHanded = Quat.LookRotation(Vec3.UnitZ, Vec3.UnitY, ProjectionHandedness.Left);

        AssertVecClose(Vec3.UnitY, rotated);
        AssertVecClose(existingMatrixRotated, matrixRotated);
        AssertVecClose(Vec3.UnitY, roundTrip * Vec3.UnitX);
        AssertVecClose(Vec3.UnitY, fromTo * Vec3.UnitX);
        AssertQuatClose(Quat.Identity, lookRightHanded);
        AssertQuatClose(Quat.Identity, lookLeftHanded);
        AssertMatrixClose(Mat4.CreateRotation(float.Pi / 2f, Vec3.UnitZ), rotation.ToMat4());
        AssertMatrixClose(Mat3.CreateRotation(rotation), rotation.ToMat3());
        AssertMatrixClose(Mat4.CreateRotation(rotation), rotation.ToMat4());
    }

    /// <summary>Generated quaternion interpolation and exponential helpers preserve expected rotation paths.</summary>
    [TestMethod]
    public void GeneratedQuaternionInterpolationAndExponentialHelpers_Work()
    {
        var target = Quat.CreateFromAxisAngle(Vec3.UnitZ, float.Pi / 2f);
        var halfway = Quat.Slerp(Quat.Identity, target, 0.5f);
        var nlerp = Quat.Nlerp(Quat.Identity, target, 0.5f);
        var expLog = Quat.Exp(Quat.Log(target));
        var sqrt = Quat.Sqrt(target);

        AssertVecClose(new Vec3(MathF.Sqrt(0.5f), MathF.Sqrt(0.5f), 0f), halfway * Vec3.UnitX);
        Assert.IsTrue(nlerp.IsNormalized(0.0001f));
        AssertVecClose(Vec3.UnitY, expLog * Vec3.UnitX);
        AssertVecClose(new Vec3(MathF.Sqrt(0.5f), MathF.Sqrt(0.5f), 0f), sqrt * Vec3.UnitX);
    }

    /// <summary>Generated quaternion formatting and parsing use the same component text as four-component vectors.</summary>
    [TestMethod]
    public void GeneratedQuaternionFormattingAndParsing_UsesVectorStyle()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;
        var value = new Quat(1f, 2f, 3f, 4.5f);
        Span<char> destination = stackalloc char[19];
        Span<byte> utf8Destination = stackalloc byte[19];

        Assert.IsTrue(value.TryFormat(destination, out var charsWritten, default, formatProvider));
        Assert.AreEqual("(1, 2, 3, 4.5)", destination[..charsWritten].ToString());
        Assert.IsTrue(value.TryFormat(utf8Destination, out var bytesWritten, default, formatProvider));
        Assert.AreEqual("(1, 2, 3, 4.5)", System.Text.Encoding.UTF8.GetString(utf8Destination[..bytesWritten]));
        Assert.AreEqual("(1.0, 2.0, 3.0, 4.5)", value.ToString("0.0", formatProvider));
        Assert.AreEqual(value, Quat.Parse("(1, 2, 3, 4.5)", formatProvider));
        Assert.AreEqual(value, ParseUtf8<Quat>("(1, 2, 3, 4.5)"u8));
        Assert.IsFalse(Quat.TryParse((string?)null, formatProvider, out _));
    }

    /// <summary>Generated quaternion conversions and relations work for float and double quaternions.</summary>
    [TestMethod]
    public void GeneratedQuaternionConversionsAndRelations_Work()
    {
        var value = new Quat(1f, 2f, 3f, 4f);
        var system = (System.Numerics.Quaternion)value;
        Quatd doubleValue = value;
        var floatValue = (Quat)doubleValue;
        (float x, float y, float z, float w) = value;
        Quat tupleValue = (x, y, z, w);
        var almost = new Quat(1.001f, 2f, 3f, 4f);

        Assert.AreEqual(1f, system.X);
        Assert.AreEqual(value, (Quat)system);
        Assert.AreEqual(new Quatd(1d, 2d, 3d, 4d), doubleValue);
        Assert.AreEqual(value, floatValue);
        Assert.AreEqual(value, tupleValue);
        Assert.IsTrue(Quat.Equal(value, almost, 0.01f).All);
        Assert.IsTrue(Quat.NotEqual(value, almost, 0.0001f).Any);
        Assert.IsTrue(Quat.IsFinite(value).All);
        Assert.IsTrue(Quat.IsNaN(new Quat(float.NaN, 0f, 0f, 1f)).Any);
    }

    /// <summary>Generated quaternion interfaces expose the type through static abstract members.</summary>
    [TestMethod]
    public void GeneratedQuaternionInterfaces_Work()
    {
        var normalized = NormalizeGeneric<Quat, float, Vec3, Vec4, Vec4b, Mat3, Mat4>(new Quat(0f, 0f, 2f, 0f));
        var rotated = TransformGeneric<Quat, float, Vec3, Mat3, Mat4>(
            Quat.CreateFromAxisAngle(Vec3.UnitZ, float.Pi / 2f),
            Vec3.UnitX);
        var system = ToSystemNumerics<Quat>(Quat.Identity);
        var matrix = CreateRotationGeneric<Mat4, float, Vec3, Vec4, Quat, Mat3>(
            Quat.CreateFromAxisAngle(Vec3.UnitZ, float.Pi / 2f));

        AssertQuatClose(new Quat(0f, 0f, 1f, 0f), normalized);
        AssertVecClose(Vec3.UnitY, rotated);
        Assert.AreEqual(1f, system.W);
        AssertVecClose(Vec3.UnitY, Mat4.TransformVector(matrix, Vec3.UnitX));
    }

    private static TQuat NormalizeGeneric<TQuat, TScalar, TVector3, TVector4, TMask, TMatrix3, TMatrix4>(TQuat value)
        where TQuat : struct, IQuat<TQuat, TScalar, TVector3, TVector4, TMask, TMatrix3, TMatrix4>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar>
        where TMask : struct =>
        TQuat.Normalize(value);

    private static TVector3 TransformGeneric<TQuat, TScalar, TVector3, TMatrix3, TMatrix4>(TQuat rotation, TVector3 vector)
        where TQuat : struct, IQuatRotation<TQuat, TScalar, TVector3, TMatrix3, TMatrix4>
        where TVector3 : struct, IVec3<TVector3, TScalar> =>
        TQuat.TransformVector(rotation, vector);

    private static System.Numerics.Quaternion ToSystemNumerics<TQuat>(TQuat value)
        where TQuat : struct, IQuatSystemNumerics<TQuat> =>
        (System.Numerics.Quaternion)value;

    private static TMatrix CreateRotationGeneric<TMatrix, TScalar, TVector3, TVector4, TQuat, TMatrix3>(TQuat rotation)
        where TMatrix : struct, IMat4QuaternionRotation<TMatrix, TScalar, TVector3, TVector4, TQuat, TMatrix3>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar>
        where TQuat : struct, IQuatRotation<TQuat, TScalar, TVector3, TMatrix3, TMatrix> =>
        TMatrix.CreateRotation(rotation);

    private static TQuat ParseUtf8<TQuat>(ReadOnlySpan<byte> source)
        where TQuat : IUtf8SpanParsable<TQuat> =>
        TQuat.Parse(source, System.Globalization.CultureInfo.InvariantCulture);

    private static void AssertQuatClose(Quat expected, Quat actual)
    {
        for (var index = 0; index < Quat.ComponentCount; index++)
            AssertClose(expected[index], actual[index]);
    }

    private static void AssertVecClose(Vec3 expected, Vec3 actual)
    {
        for (var index = 0; index < Vec3.ComponentCount; index++)
            AssertClose(expected[index], actual[index]);
    }

    private static void AssertVecClose(Vec4 expected, Vec4 actual)
    {
        for (var index = 0; index < Vec4.ComponentCount; index++)
            AssertClose(expected[index], actual[index]);
    }

    private static void AssertMatrixClose(Mat3 expected, Mat3 actual)
    {
        for (var column = 0; column < Mat3.ColumnCount; column++)
        {
            for (var row = 0; row < Mat3.RowCount; row++)
                AssertClose(expected[column, row], actual[column, row]);
        }
    }

    private static void AssertMatrixClose(Mat4 expected, Mat4 actual)
    {
        for (var column = 0; column < Mat4.ColumnCount; column++)
        {
            for (var row = 0; row < Mat4.RowCount; row++)
                AssertClose(expected[column, row], actual[column, row]);
        }
    }

    private static TQuat MultiplicativeIdentity<TQuat>()
        where TQuat : IMultiplicativeIdentity<TQuat, TQuat> =>
        TQuat.MultiplicativeIdentity;

    private static TQuat AdditiveIdentity<TQuat>()
        where TQuat : IAdditiveIdentity<TQuat, TQuat> =>
        TQuat.AdditiveIdentity;

    private static void AssertClose(float expected, float actual) =>
        Assert.AreEqual(expected, actual, 0.0001f);
}

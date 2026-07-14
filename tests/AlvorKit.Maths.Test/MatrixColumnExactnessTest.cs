namespace AlvorKit.Maths.Test;

/// <summary>Locks exact component bits for matrix operations against their selected column or System backend.</summary>
[TestClass]
public sealed class MatrixColumnExactnessTest
{
    private static readonly float FloatPayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7fc1_2345));
    private static readonly double DoublePayloadNaN = BitConverter.Int64BitsToDouble(unchecked((long)0x7ff8_1234_5678_9abc));

    /// <summary>Float rectangular lane operations match their original component formulas for special values.</summary>
    [TestMethod]
    public void FloatRectangularLaneOperations_MatchScalarFormulaBits()
    {
        var left = new Mat3x2(
            new Vec2(-0f, FloatPayloadNaN),
            new Vec2(float.PositiveInfinity, float.Epsilon),
            new Vec2(-float.Epsilon, float.MaxValue));
        var right = new Mat3x2(
            new Vec2(0f, -2f),
            new Vec2(float.NegativeInfinity, -float.Epsilon),
            new Vec2(float.Epsilon, -float.MaxValue));
        var divisor = new Mat3x2(
            new Vec2(-1f, 2f),
            new Vec2(float.PositiveInfinity, -2f),
            new Vec2(float.Epsilon, -float.MaxValue));

        AssertBits(ScalarAdd(left, right), left + right);
        AssertBits(ScalarSubtract(left, right), left - right);
        AssertBits(ScalarNegate(left), -left);
        AssertBits(ScalarScale(left, -0f), left * -0f);
        AssertBits(ScalarDivide(left, -2f), left / -2f);
        AssertBits(ScalarMultiply(left, right), Mat3x2.ComponentMultiply(left, right));
        AssertBits(ScalarDivide(left, divisor), left / divisor);
        AssertBits(ScalarLerp(left, right, 0.5f), Mat3x2.Lerp(left, right, 0.5f));
    }

    /// <summary>Double two-row matrices retain exact bits for the fixed-width column operations selected by the generator.</summary>
    [TestMethod]
    public void DoubleRectangularLaneOperations_MatchScalarFormulaBits()
    {
        var left = new Mat3x2d(
            new Vec2d(-0d, DoublePayloadNaN),
            new Vec2d(double.PositiveInfinity, double.Epsilon),
            new Vec2d(-double.Epsilon, double.MaxValue));
        var right = new Mat3x2d(
            new Vec2d(0d, -2d),
            new Vec2d(double.NegativeInfinity, -double.Epsilon),
            new Vec2d(double.Epsilon, -double.MaxValue));
        var divisor = new Mat3x2d(
            new Vec2d(-1d, 2d),
            new Vec2d(double.PositiveInfinity, -2d),
            new Vec2d(double.Epsilon, -double.MaxValue));

        AssertBits(ScalarAdd(left, right), left + right);
        AssertBits(ScalarNegate(left), -left);
        AssertBits(ScalarDivide(left, -2d), left / -2d);
        AssertBits(ScalarDivide(left, divisor), left / divisor);
    }

    /// <summary>Double square division uses complete-register columns without changing IEEE component bits.</summary>
    [TestMethod]
    public void DoubleSquareDivision_MatchesScalarFormulaBits()
    {
        var value = new Mat4d(
            new Vec4d(-0d, DoublePayloadNaN, double.PositiveInfinity, double.Epsilon),
            new Vec4d(0d, -2d, double.NegativeInfinity, -double.Epsilon),
            new Vec4d(double.MaxValue, -double.MaxValue, 1d, -1d),
            new Vec4d(4d, -8d, 16d, -32d));
        var divisor = new Mat4d(
            new Vec4d(-1d, 2d, double.PositiveInfinity, -2d),
            new Vec4d(double.Epsilon, -double.MaxValue, 4d, -8d),
            new Vec4d(0.5d, -0.5d, 3d, -3d),
            new Vec4d(16d, -16d, 32d, -32d));

        AssertBits(ScalarDivide(value, -2d), value / -2d);
        AssertBits(ScalarDivide(value, divisor), value / divisor);
    }

    /// <summary>Selected Mat4d column and row paths preserve exact component bits and reduction order.</summary>
    [TestMethod]
    public void DoubleSquareSelectedOperations_MatchScalarFormulaBits()
    {
        var left = new Mat4d(
            new Vec4d(-0d, DoublePayloadNaN, double.PositiveInfinity, double.Epsilon),
            new Vec4d(1d, -2d, double.NegativeInfinity, -double.Epsilon),
            new Vec4d(double.MaxValue, -double.MaxValue, 16_777_216d, -16_777_216d),
            new Vec4d(4d, -8d, 1d, -1d));
        var right = new Mat4d(
            new Vec4d(0d, -2d, double.NegativeInfinity, -double.Epsilon),
            new Vec4d(-1d, 2d, double.PositiveInfinity, double.Epsilon),
            new Vec4d(-double.MaxValue, double.MaxValue, 1d, 1d),
            new Vec4d(-4d, 8d, -1d, 1d));
        var vector = new Vec4d(1d, 1d, 1d, 1d);

        AssertBits(ScalarAdd(left, right), left + right);
        AssertBits(ScalarSubtract(left, right), left - right);
        AssertBits(ScalarNegate(left), -left);
        AssertBits(ScalarScale(left, -2d), left * -2d);
        AssertBits(ScalarMultiply(left, right), Mat4d.ComponentMultiply(left, right));
        AssertBits(ScalarLerp(left, right, 0.5d), Mat4d.Lerp(left, right, 0.5d));
        AssertBits(new Mat4d(left.Row0, left.Row1, left.Row2, left.Row3), left.Transposed);
        AssertBits(ScalarMatrixVector(left, vector), left * vector);
        AssertBits(new Mat4d(
            ScalarMatrixVector(left, right.Column0),
            ScalarMatrixVector(left, right.Column1),
            ScalarMatrixVector(left, right.Column2),
            ScalarMatrixVector(left, right.Column3)), left * right);
    }

    /// <summary>Rectangular matrix-vector directions and matrix products preserve operand and left-associative addition order.</summary>
    [TestMethod]
    public void FloatRectangularProducts_MatchScalarFormulaBits()
    {
        var left = new Mat3x2(
            new Vec2(16_777_216f, FloatPayloadNaN),
            new Vec2(1f, -0f),
            new Vec2(-16_777_216f, 0f));
        var column = new Vec3(1f, 1f, 1f);
        var row = new Vec2(-0f, 2f);
        var right = new Mat2x3(
            new Vec3(1f, -1f, 0.5f),
            new Vec3(-2f, 4f, -0f));

        AssertBits(ScalarMatrixVector(left, column), left * column);
        AssertBits(ScalarVectorMatrix(row, left), row * left);
        AssertBits(
            new Mat2(ScalarMatrixVector(left, right.Column0), ScalarMatrixVector(left, right.Column1)),
            left * right);
    }

    /// <summary>Square float matrix and vector products follow the compatible System backend.</summary>
    [TestMethod]
    public void FloatSquareProducts_MatchSystemBackendBits()
    {
        var left = new Mat4(
            new Vec4(16_777_216f, FloatPayloadNaN, -0f, float.Epsilon),
            new Vec4(1f, 2f, 0f, -float.Epsilon),
            new Vec4(-16_777_216f, -2f, 1f, float.MaxValue),
            new Vec4(1f, 0.5f, -1f, -float.MaxValue));
        var right = new Mat4(
            new Vec4(1f, 1f, 1f, 1f),
            new Vec4(-0f, 2f, -1f, 0.5f),
            new Vec4(4f, -2f, 0f, 1f),
            new Vec4(-1f, 0.25f, 2f, -0f));
        var vector = new Vec4(1f, FloatPayloadNaN, -0f, float.PositiveInfinity);

        AssertBits(SystemMatrixProduct(left, right), left * right);
        AssertBits(SystemMatrixVector(left, vector), left * vector);
    }

    /// <summary>The runtime Mat4 transpose shuffle only reorders components, including special IEEE bit patterns.</summary>
    [TestMethod]
    public void FloatSquareTranspose_PreservesComponentBits()
    {
        var value = new Mat4(
            new Vec4(-0f, FloatPayloadNaN, float.PositiveInfinity, float.Epsilon),
            new Vec4(0f, -2f, float.NegativeInfinity, -float.Epsilon),
            new Vec4(float.MaxValue, -float.MaxValue, 16_777_216f, -16_777_216f),
            new Vec4(4f, -8f, 1f, -1f));
        var expected = new Mat4(value.Row0, value.Row1, value.Row2, value.Row3);

        AssertBits(expected, value.Transposed);
        AssertBits(expected, Mat4.Transpose(value));
    }

    /// <summary>Square float inversion follows System arithmetic while retaining AlvorKit's default failure result.</summary>
    [TestMethod]
    public void FloatSquareInverse_MatchesSystemBackendAndFailureContract()
    {
        var value = new Mat4(
            new Vec4(1.25f, -2.5f, 0.5f, 0f),
            new Vec4(3.75f, 4.5f, -1.25f, 0f),
            new Vec4(-1.875f, 2.25f, 3.5f, 0f),
            new Vec4(2f, -3f, 4f, 1f));
        var systemValue = System.Runtime.CompilerServices.Unsafe.BitCast<Mat4, System.Numerics.Matrix4x4>(value);

        Assert.IsTrue(System.Numerics.Matrix4x4.Invert(systemValue, out var systemExpected));
        Assert.IsTrue(Mat4.TryInvert(value, out var actual));
        AssertBits(System.Runtime.CompilerServices.Unsafe.BitCast<System.Numerics.Matrix4x4, Mat4>(systemExpected), actual);
        Assert.IsFalse(Mat4.TryInvert(Mat4.Zero, out var singular));
        Assert.AreEqual(default, singular);
    }

    /// <summary>Compact float point and direction transforms follow the compatible System backend.</summary>
    [TestMethod]
    public void FloatAffineTransforms_MatchSystemBackendBits()
    {
        var matrix = new Mat3x2(
            new Vec2(1.25f, FloatPayloadNaN),
            new Vec2(-2.5f, float.PositiveInfinity),
            new Vec2(-0f, float.Epsilon));
        var value = new Vec2(FloatPayloadNaN, -0f);
        var systemMatrix = System.Runtime.CompilerServices.Unsafe.BitCast<Mat3x2, System.Numerics.Matrix3x2>(matrix);
        var systemValue = System.Runtime.CompilerServices.Unsafe.BitCast<Vec2, System.Numerics.Vector2>(value);

        AssertBits(
            System.Runtime.CompilerServices.Unsafe.BitCast<System.Numerics.Vector2, Vec2>(
                System.Numerics.Vector2.Transform(systemValue, systemMatrix)),
            Mat3x2.TransformPoint(matrix, value));
        AssertBits(
            System.Runtime.CompilerServices.Unsafe.BitCast<System.Numerics.Vector2, Vec2>(
                System.Numerics.Vector2.TransformNormal(systemValue, systemMatrix)),
            Mat3x2.TransformVector(matrix, value));
    }

    /// <summary>The runtime Mat3x2 inverse path preserves the prior formula bits and default singular result.</summary>
    [TestMethod]
    public void FloatAffineInverse_PreservesFormulaBitsAndFailureResult()
    {
        var finite = new Mat3x2(
            new Vec2(1.25f, -2.5f),
            new Vec2(3.75f, 4.5f),
            new Vec2(-1.875f, 2.25f));
        var special = new Mat3x2(
            new Vec2(FloatPayloadNaN, -0f),
            new Vec2(2f, 3f),
            new Vec2(float.PositiveInfinity, -float.Epsilon));

        Assert.IsTrue(Mat3x2.TryInvert(finite, out var finiteActual));
        AssertBits(ScalarAffineInverse(finite), finiteActual);
        Assert.IsTrue(Mat3x2.TryInvert(special, out var specialActual));
        AssertBits(ScalarAffineInverse(special), specialActual);
        Assert.IsFalse(Mat3x2.TryInvert(Mat3x2.Zero, out var singularActual));
        Assert.AreEqual(default, singularActual);
    }

    private static Mat3x2 ScalarAdd(Mat3x2 a, Mat3x2 b) => new(
        Add(a.Column0, b.Column0), Add(a.Column1, b.Column1), Add(a.Column2, b.Column2));

    private static Mat3x2 ScalarSubtract(Mat3x2 a, Mat3x2 b) => new(
        Subtract(a.Column0, b.Column0), Subtract(a.Column1, b.Column1), Subtract(a.Column2, b.Column2));

    private static Mat3x2 ScalarNegate(Mat3x2 value) => new(
        Negate(value.Column0), Negate(value.Column1), Negate(value.Column2));

    private static Mat3x2 ScalarScale(Mat3x2 value, float scale) => new(
        Scale(value.Column0, scale), Scale(value.Column1, scale), Scale(value.Column2, scale));

    private static Mat3x2 ScalarDivide(Mat3x2 value, float divisor) => new(
        Divide(value.Column0, divisor), Divide(value.Column1, divisor), Divide(value.Column2, divisor));

    private static Mat3x2 ScalarMultiply(Mat3x2 a, Mat3x2 b) => new(
        Multiply(a.Column0, b.Column0), Multiply(a.Column1, b.Column1), Multiply(a.Column2, b.Column2));

    private static Mat3x2 ScalarDivide(Mat3x2 a, Mat3x2 b) => new(
        Divide(a.Column0, b.Column0), Divide(a.Column1, b.Column1), Divide(a.Column2, b.Column2));

    private static Mat3x2 ScalarLerp(Mat3x2 from, Mat3x2 to, float amount) => new(
        Lerp(from.Column0, to.Column0, amount),
        Lerp(from.Column1, to.Column1, amount),
        Lerp(from.Column2, to.Column2, amount));

    private static Mat3x2 ScalarAffineInverse(Mat3x2 value)
    {
        var invDet = 1f / ((value.Column0.X * value.Column1.Y) - (value.Column1.X * value.Column0.Y));
        return new(
            new Vec2(value.Column1.Y * invDet, -value.Column0.Y * invDet),
            new Vec2(-value.Column1.X * invDet, value.Column0.X * invDet),
            new Vec2(
                ((value.Column1.X * value.Column2.Y) - (value.Column1.Y * value.Column2.X)) * invDet,
                ((value.Column0.Y * value.Column2.X) - (value.Column0.X * value.Column2.Y)) * invDet));
    }

    private static Mat3x2d ScalarAdd(Mat3x2d a, Mat3x2d b) => new(
        Add(a.Column0, b.Column0), Add(a.Column1, b.Column1), Add(a.Column2, b.Column2));

    private static Mat3x2d ScalarNegate(Mat3x2d value) => new(
        Negate(value.Column0), Negate(value.Column1), Negate(value.Column2));

    private static Mat3x2d ScalarDivide(Mat3x2d value, double divisor) => new(
        Divide(value.Column0, divisor), Divide(value.Column1, divisor), Divide(value.Column2, divisor));

    private static Mat3x2d ScalarDivide(Mat3x2d a, Mat3x2d b) => new(
        Divide(a.Column0, b.Column0), Divide(a.Column1, b.Column1), Divide(a.Column2, b.Column2));

    private static Mat4d ScalarDivide(Mat4d value, double divisor) => new(
        Divide(value.Column0, divisor), Divide(value.Column1, divisor),
        Divide(value.Column2, divisor), Divide(value.Column3, divisor));

    private static Mat4d ScalarDivide(Mat4d a, Mat4d b) => new(
        Divide(a.Column0, b.Column0), Divide(a.Column1, b.Column1),
        Divide(a.Column2, b.Column2), Divide(a.Column3, b.Column3));

    private static Mat4d ScalarAdd(Mat4d a, Mat4d b) => new(
        Add(a.Column0, b.Column0), Add(a.Column1, b.Column1), Add(a.Column2, b.Column2), Add(a.Column3, b.Column3));

    private static Mat4d ScalarSubtract(Mat4d a, Mat4d b) => new(
        Subtract(a.Column0, b.Column0), Subtract(a.Column1, b.Column1),
        Subtract(a.Column2, b.Column2), Subtract(a.Column3, b.Column3));

    private static Mat4d ScalarNegate(Mat4d value) => new(
        Negate(value.Column0), Negate(value.Column1), Negate(value.Column2), Negate(value.Column3));

    private static Mat4d ScalarScale(Mat4d value, double scale) => new(
        Scale(value.Column0, scale), Scale(value.Column1, scale), Scale(value.Column2, scale), Scale(value.Column3, scale));

    private static Mat4d ScalarMultiply(Mat4d a, Mat4d b) => new(
        Multiply(a.Column0, b.Column0), Multiply(a.Column1, b.Column1),
        Multiply(a.Column2, b.Column2), Multiply(a.Column3, b.Column3));

    private static Mat4d ScalarLerp(Mat4d from, Mat4d to, double amount) => new(
        Lerp(from.Column0, to.Column0, amount), Lerp(from.Column1, to.Column1, amount),
        Lerp(from.Column2, to.Column2, amount), Lerp(from.Column3, to.Column3, amount));

    private static Vec4d ScalarMatrixVector(Mat4d matrix, Vec4d value) => new(
        Sum(matrix.Column0.X, value.X, matrix.Column1.X, value.Y, matrix.Column2.X, value.Z, matrix.Column3.X, value.W),
        Sum(matrix.Column0.Y, value.X, matrix.Column1.Y, value.Y, matrix.Column2.Y, value.Z, matrix.Column3.Y, value.W),
        Sum(matrix.Column0.Z, value.X, matrix.Column1.Z, value.Y, matrix.Column2.Z, value.Z, matrix.Column3.Z, value.W),
        Sum(matrix.Column0.W, value.X, matrix.Column1.W, value.Y, matrix.Column2.W, value.Z, matrix.Column3.W, value.W));

    private static Vec2 ScalarMatrixVector(Mat3x2 matrix, Vec3 value) => new(
        ((matrix.Column0.X * value.X) + (matrix.Column1.X * value.Y)) + (matrix.Column2.X * value.Z),
        ((matrix.Column0.Y * value.X) + (matrix.Column1.Y * value.Y)) + (matrix.Column2.Y * value.Z));

    private static Vec3 ScalarVectorMatrix(Vec2 value, Mat3x2 matrix) => new(
        (value.X * matrix.Column0.X) + (value.Y * matrix.Column0.Y),
        (value.X * matrix.Column1.X) + (value.Y * matrix.Column1.Y),
        (value.X * matrix.Column2.X) + (value.Y * matrix.Column2.Y));

    private static Vec4 ScalarMatrixVector(Mat4 matrix, Vec4 value) => new(
        Sum(matrix.Column0.X, value.X, matrix.Column1.X, value.Y, matrix.Column2.X, value.Z, matrix.Column3.X, value.W),
        Sum(matrix.Column0.Y, value.X, matrix.Column1.Y, value.Y, matrix.Column2.Y, value.Z, matrix.Column3.Y, value.W),
        Sum(matrix.Column0.Z, value.X, matrix.Column1.Z, value.Y, matrix.Column2.Z, value.Z, matrix.Column3.Z, value.W),
        Sum(matrix.Column0.W, value.X, matrix.Column1.W, value.Y, matrix.Column2.W, value.Z, matrix.Column3.W, value.W));

    private static Mat4 SystemMatrixProduct(Mat4 left, Mat4 right) =>
        System.Runtime.CompilerServices.Unsafe.BitCast<System.Numerics.Matrix4x4, Mat4>(
            System.Runtime.CompilerServices.Unsafe.BitCast<Mat4, System.Numerics.Matrix4x4>(right) *
            System.Runtime.CompilerServices.Unsafe.BitCast<Mat4, System.Numerics.Matrix4x4>(left));

    private static Vec4 SystemMatrixVector(Mat4 matrix, Vec4 value) =>
        System.Runtime.CompilerServices.Unsafe.BitCast<System.Numerics.Vector4, Vec4>(
            System.Numerics.Vector4.Transform(
                System.Runtime.CompilerServices.Unsafe.BitCast<Vec4, System.Numerics.Vector4>(value),
                System.Runtime.CompilerServices.Unsafe.BitCast<Mat4, System.Numerics.Matrix4x4>(matrix)));

    private static float Sum(float a0, float b0, float a1, float b1, float a2, float b2, float a3, float b3) =>
        (((a0 * b0) + (a1 * b1)) + (a2 * b2)) + (a3 * b3);

    private static Vec2 Add(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    private static Vec2 Subtract(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    private static Vec2 Negate(Vec2 value) => new(-value.X, -value.Y);
    private static Vec2 Scale(Vec2 value, float scale) => new(value.X * scale, value.Y * scale);
    private static Vec2 Divide(Vec2 value, float divisor) => new(value.X / divisor, value.Y / divisor);
    private static Vec2 Multiply(Vec2 a, Vec2 b) => new(a.X * b.X, a.Y * b.Y);
    private static Vec2 Divide(Vec2 a, Vec2 b) => new(a.X / b.X, a.Y / b.Y);
    private static Vec2 Lerp(Vec2 from, Vec2 to, float amount) => new(
        from.X + ((to.X - from.X) * amount), from.Y + ((to.Y - from.Y) * amount));

    private static Vec2d Add(Vec2d a, Vec2d b) => new(a.X + b.X, a.Y + b.Y);
    private static Vec2d Negate(Vec2d value) => new(-value.X, -value.Y);
    private static Vec2d Divide(Vec2d value, double divisor) => new(value.X / divisor, value.Y / divisor);
    private static Vec2d Divide(Vec2d a, Vec2d b) => new(a.X / b.X, a.Y / b.Y);

    private static Vec4d Divide(Vec4d value, double divisor) => new(
        value.X / divisor, value.Y / divisor, value.Z / divisor, value.W / divisor);

    private static Vec4d Divide(Vec4d a, Vec4d b) => new(
        a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);

    private static Vec4d Add(Vec4d a, Vec4d b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    private static Vec4d Subtract(Vec4d a, Vec4d b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
    private static Vec4d Negate(Vec4d value) => new(-value.X, -value.Y, -value.Z, -value.W);
    private static Vec4d Scale(Vec4d value, double scale) => new(value.X * scale, value.Y * scale, value.Z * scale, value.W * scale);
    private static Vec4d Multiply(Vec4d a, Vec4d b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
    private static Vec4d Lerp(Vec4d from, Vec4d to, double amount) => new(
        from.X + ((to.X - from.X) * amount), from.Y + ((to.Y - from.Y) * amount),
        from.Z + ((to.Z - from.Z) * amount), from.W + ((to.W - from.W) * amount));

    private static double Sum(double a0, double b0, double a1, double b1, double a2, double b2, double a3, double b3) =>
        (((a0 * b0) + (a1 * b1)) + (a2 * b2)) + (a3 * b3);

    private static void AssertBits(Mat3x2 expected, Mat3x2 actual)
    {
        for (var column = 0; column < Mat3x2.ColumnCount; column++)
        {
            for (var row = 0; row < Mat3x2.RowCount; row++)
            {
                Assert.AreEqual(
                    BitConverter.SingleToInt32Bits(expected[column, row]),
                    BitConverter.SingleToInt32Bits(actual[column, row]),
                    $"[{column}, {row}]");
            }
        }
    }

    private static void AssertBits(Mat2 expected, Mat2 actual)
    {
        for (var column = 0; column < Mat2.ColumnCount; column++)
        {
            for (var row = 0; row < Mat2.RowCount; row++)
            {
                Assert.AreEqual(
                    BitConverter.SingleToInt32Bits(expected[column, row]),
                    BitConverter.SingleToInt32Bits(actual[column, row]),
                    $"[{column}, {row}]");
            }
        }
    }

    private static void AssertBits(Mat4 expected, Mat4 actual)
    {
        for (var column = 0; column < Mat4.ColumnCount; column++)
        {
            for (var row = 0; row < Mat4.RowCount; row++)
            {
                Assert.AreEqual(
                    BitConverter.SingleToInt32Bits(expected[column, row]),
                    BitConverter.SingleToInt32Bits(actual[column, row]),
                    $"[{column}, {row}]");
            }
        }
    }

    private static void AssertBits(Mat3x2d expected, Mat3x2d actual)
    {
        for (var column = 0; column < Mat3x2d.ColumnCount; column++)
        {
            for (var row = 0; row < Mat3x2d.RowCount; row++)
            {
                Assert.AreEqual(
                    BitConverter.DoubleToInt64Bits(expected[column, row]),
                    BitConverter.DoubleToInt64Bits(actual[column, row]),
                    $"[{column}, {row}]");
            }
        }
    }

    private static void AssertBits(Mat4d expected, Mat4d actual)
    {
        for (var column = 0; column < Mat4d.ColumnCount; column++)
        {
            for (var row = 0; row < Mat4d.RowCount; row++)
            {
                Assert.AreEqual(
                    BitConverter.DoubleToInt64Bits(expected[column, row]),
                    BitConverter.DoubleToInt64Bits(actual[column, row]),
                    $"[{column}, {row}]");
            }
        }
    }

    private static void AssertBits(Vec2 expected, Vec2 actual)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X), "X");
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y), "Y");
    }

    private static void AssertBits(Vec3 expected, Vec3 actual)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X), "X");
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y), "Y");
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Z), BitConverter.SingleToInt32Bits(actual.Z), "Z");
    }

    private static void AssertBits(Vec4d expected, Vec4d actual)
    {
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.X), BitConverter.DoubleToInt64Bits(actual.X), "X");
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Y), BitConverter.DoubleToInt64Bits(actual.Y), "Y");
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Z), BitConverter.DoubleToInt64Bits(actual.Z), "Z");
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.W), BitConverter.DoubleToInt64Bits(actual.W), "W");
    }

    private static void AssertBits(Vec4 expected, Vec4 actual)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X), "X");
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y), "Y");
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Z), BitConverter.SingleToInt32Bits(actual.Z), "Z");
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.W), BitConverter.SingleToInt32Bits(actual.W), "W");
    }

}

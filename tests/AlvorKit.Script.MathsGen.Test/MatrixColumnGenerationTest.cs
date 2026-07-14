namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Verifies matrix generation selects the retained column-vector and System-backed implementation shapes.</summary>
[TestClass]
public sealed class MatrixColumnGenerationTest
{
    /// <summary>Float rectangular matrices compose products from columns while Mat4 uses the compatible System backend.</summary>
    [TestMethod]
    public void FloatMatrices_UseSelectedColumnAndSystemExpressions()
    {
        var rectangular = MatrixFileEmitter.Emit(new(2, 3, VectorCatalog.Float));
        var square = MatrixFileEmitter.Emit(new(4, 4, VectorCatalog.Float));

        StringAssert.Contains(rectangular, "new(-value.Column0, -value.Column1)");
        StringAssert.Contains(rectangular, "new(left.Column0 + right.Column0, left.Column1 + right.Column1)");
        StringAssert.Contains(rectangular, "new(left.Column0 - right.Column0, left.Column1 - right.Column1)");
        StringAssert.Contains(rectangular, "new(left.Column0 / right.Column0, left.Column1 / right.Column1)");
        StringAssert.Contains(rectangular, "new(left.Column0 * right, left.Column1 * right)");
        StringAssert.Contains(rectangular, "new(left.Column0 / right, left.Column1 / right)");
        StringAssert.Contains(rectangular, "new(left * right.Column0, left * right.Column1)");
        StringAssert.Contains(rectangular,
            "new(from.Column0 + ((to.Column0 - from.Column0) * amount), " +
            "from.Column1 + ((to.Column1 - from.Column1) * amount))");
        StringAssert.Contains(rectangular, "new(left.Column0 * right.Column0, left.Column1 * right.Column1)");
        StringAssert.Contains(rectangular, "left.Column0 * right.X + left.Column1 * right.Y");
        StringAssert.Contains(rectangular, "Vec3.Dot(left, right.Column0)");
        StringAssert.Contains(rectangular, "Vec3.Dot(left, right.Column1)");
        StringAssert.Contains(square,
            "new(right.packed * left.packed)");
        StringAssert.Contains(square,
            "new(left.packed + right.packed)");
        StringAssert.Contains(square, "System.Numerics.Vector4.Transform(");
    }

    /// <summary>Double matrices use columns only for operations with retained complete-register vector implementations.</summary>
    [TestMethod]
    public void DoubleMatrices_UseOnlyRetainedColumnExpressions()
    {
        var twoRows = MatrixFileEmitter.Emit(new(3, 2, VectorCatalog.Double));
        var fourRows = MatrixFileEmitter.Emit(new(3, 4, VectorCatalog.Double));
        var fourSquare = MatrixFileEmitter.Emit(new(4, 4, VectorCatalog.Double));
        var threeRows = MatrixFileEmitter.Emit(new(3, 3, VectorCatalog.Double));

        StringAssert.Contains(twoRows, "new(-value.Column0, -value.Column1, -value.Column2)");
        StringAssert.Contains(twoRows,
            "new(left.Column0 + right.Column0, left.Column1 + right.Column1, left.Column2 + right.Column2)");
        StringAssert.Contains(twoRows,
            "new(left.Column0 / right.Column0, left.Column1 / right.Column1, left.Column2 / right.Column2)");
        StringAssert.Contains(twoRows, "new(left.Column0 / right, left.Column1 / right, left.Column2 / right)");
        StringAssert.Contains(fourRows,
            "new(left.Column0 / right.Column0, left.Column1 / right.Column1, left.Column2 / right.Column2)");
        StringAssert.Contains(fourRows, "new(left.Column0 / right, left.Column1 / right, left.Column2 / right)");
        StringAssert.Contains(fourSquare, "new(-value.Column0, -value.Column1, -value.Column2, -value.Column3)");
        StringAssert.Contains(fourSquare,
            "new(left.Column0 + right.Column0, left.Column1 + right.Column1, left.Column2 + right.Column2, left.Column3 + right.Column3)");
        StringAssert.Contains(fourSquare,
            "new(left.Column0 - right.Column0, left.Column1 - right.Column1, left.Column2 - right.Column2, left.Column3 - right.Column3)");
        StringAssert.Contains(fourSquare,
            "new(left.Column0 * right.Column0, left.Column1 * right.Column1, left.Column2 * right.Column2, left.Column3 * right.Column3)");
        StringAssert.Contains(fourSquare, "new(left.Column0 * right, left.Column1 * right, left.Column2 * right, left.Column3 * right)");
        StringAssert.Contains(fourSquare,
            "new(from.Column0 + ((to.Column0 - from.Column0) * amount), from.Column1 + ((to.Column1 - from.Column1) * amount), " +
            "from.Column2 + ((to.Column2 - from.Column2) * amount), from.Column3 + ((to.Column3 - from.Column3) * amount))");
        StringAssert.Contains(fourSquare,
            "left.Column0 * right.X + left.Column1 * right.Y + left.Column2 * right.Z + left.Column3 * right.W");
        StringAssert.Contains(twoRows, "ScalarMath.Lerp(from[0, 0], to[0, 0], amount)");
        StringAssert.Contains(twoRows, "Vec2d.Dot(left, right.Column0)");
        StringAssert.Contains(twoRows, "Vec2d.Dot(left, right.Column1)");

        StringAssert.Contains(twoRows,
            $"operator -(Mat3x2d left, Mat3x2d right) =>{Environment.NewLine}        new({Environment.NewLine}");
        StringAssert.Contains(twoRows,
            $"ComponentMultiply(Mat3x2d left, Mat3x2d right) =>{Environment.NewLine}        new({Environment.NewLine}");
        StringAssert.Contains(fourRows,
            $"operator -(Mat3x4d value) =>{Environment.NewLine}        new({Environment.NewLine}");
        StringAssert.Contains(fourRows,
            $"operator +(Mat3x4d left, Mat3x4d right) =>{Environment.NewLine}        new({Environment.NewLine}");
        StringAssert.Contains(threeRows,
            $"operator /(Mat3d left, Mat3d right) =>{Environment.NewLine}        new({Environment.NewLine}");
    }

    /// <summary>Mat4 uses the runtime transpose intrinsic while double matrices retain exact ordered expressions.</summary>
    [TestMethod]
    public void Mat4TransposeAndDoubleProducts_UseSelectedShapes()
    {
        var mat4 = MatrixFileEmitter.Emit(new(4, 4, VectorCatalog.Float));
        var mat4d = MatrixFileEmitter.Emit(new(4, 4, VectorCatalog.Double));

        StringAssert.Contains(mat4,
            "new(System.Numerics.Matrix4x4.Transpose(this.packed))");
        StringAssert.Contains(mat4, "[StructLayout(LayoutKind.Explicit)]");
        StringAssert.Contains(mat4, "private System.Numerics.Matrix4x4 packed;");
        StringAssert.Contains(mat4, "System.Numerics.Matrix4x4.Invert(value.packed, out result.packed)");
        StringAssert.Contains(mat4d, "new(this.Row0, this.Row1, this.Row2, this.Row3)");
        StringAssert.Contains(mat4d,
            "left.Column0.X * right.Column0.X + left.Column1.X * right.Column0.Y + " +
            "left.Column2.X * right.Column0.Z + left.Column3.X * right.Column0.W");
        StringAssert.Contains(mat4d,
            $"/// <summary>Multiplies two column-major matrices.</summary>{Environment.NewLine}" +
            $"    [MethodImpl(MethodImplOptions.AggressiveInlining)]{Environment.NewLine}" +
            "    public static Mat4d operator *(Mat4d left, Mat4d right)");
        StringAssert.Contains(mat4, "Matrix4x4.Transpose");
        Assert.IsFalse(mat4d.Contains("System.Runtime.Intrinsics", StringComparison.Ordinal));
    }

    /// <summary>Float affine inversion uses the compatible runtime layout while double inversion retains its scalar formula.</summary>
    [TestMethod]
    public void Mat3x2Inverse_UsesCompatibleRuntimePathForFloatOnly()
    {
        var mat3x2 = MatrixFileEmitter.Emit(new(3, 2, VectorCatalog.Float));
        var mat3x2d = MatrixFileEmitter.Emit(new(3, 2, VectorCatalog.Double));

        StringAssert.Contains(mat3x2, "[StructLayout(LayoutKind.Explicit)]");
        StringAssert.Contains(mat3x2, "private System.Numerics.Matrix3x2 packed;");
        StringAssert.Contains(mat3x2, "System.Numerics.Matrix3x2.Invert(value.packed, out result.packed)");
        StringAssert.Contains(mat3x2, "System.Numerics.Vector2.Transform(");
        StringAssert.Contains(mat3x2, "System.Numerics.Vector2.TransformNormal(");
        StringAssert.Contains(mat3x2,
            "explicit operator System.Numerics.Matrix3x2(Mat3x2 value) =>" + Environment.NewLine +
            "        value.packed;");
        StringAssert.Contains(mat3x2d, "var invDet = 1d / det;");
        Assert.IsFalse(mat3x2d.Contains("System.Numerics.Matrix3x2", StringComparison.Ordinal));
    }
}

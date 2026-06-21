namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated math source planning and filesystem output.</summary>
[TestClass]
public sealed class MathsGeneratorTest
{
    /// <summary>Generation recreates the output directory and emits every configured primitive family.</summary>
    [TestMethod]
    public void GenerateTo_RecreatesOutputAndEmitsConfiguredVectors()
    {
        using var workspace = TempWorkspace.Create();
        var outputRoot = workspace.CreateDirectory("generated");
        var project = Path.Combine(outputRoot, MathsGenerator.PrimitivesProjectName);
        Directory.CreateDirectory(project);
        var staleFile = Path.Combine(project, "stale.txt");
        File.WriteAllText(staleFile, "old");

        MathsGenerator.GenerateTo(outputRoot, "9.8.7");

        var vecDirectory = Path.Combine(project, MathsGenerator.VecDirectoryName);
        var matDirectory = Path.Combine(project, MathsGenerator.MatDirectoryName);
        var quatDirectory = Path.Combine(project, MathsGenerator.QuatDirectoryName);
        var boxDirectory = Path.Combine(project, MathsGenerator.BoxDirectoryName);
        string VecFile(string fileName) => Path.Combine(vecDirectory, fileName);
        string MatFile(string fileName) => Path.Combine(matDirectory, fileName);
        string QuatFile(string fileName) => Path.Combine(quatDirectory, fileName);
        string BoxFile(string fileName) => Path.Combine(boxDirectory, fileName);
        Assert.IsFalse(File.Exists(staleFile));
        Assert.IsFalse(Directory.Exists(Path.Combine(project, "Generated")));
        Assert.AreEqual(96, Directory.GetFiles(vecDirectory, "*.cs").Length);
        Assert.AreEqual(18, Directory.GetFiles(matDirectory, "*.cs").Length);
        Assert.AreEqual(2, Directory.GetFiles(quatDirectory, "*.cs").Length);
        Assert.AreEqual(6, Directory.GetFiles(boxDirectory, "*.cs").Length);
        Assert.IsTrue(File.Exists(Path.Combine(project, "AlvorKit.Maths.Primitives.csproj")));
        Assert.IsFalse(File.Exists(Path.Combine(project, "ScalarMath.g.cs")));
        Assert.IsTrue(File.Exists(CoreFile("ScalarMath.cs")));
        Assert.IsTrue(File.Exists(CoreFile("Vec", "IVec.cs")));
        Assert.IsTrue(File.Exists(CoreFile("Vec", "IVec3FloatingToInteger.cs")));
        Assert.IsTrue(File.Exists(CoreFile("Mat", "IMat2.cs")));
        Assert.IsTrue(File.Exists(CoreFile("Mat", "IMat4Transform.cs")));
        Assert.IsTrue(File.Exists(CoreFile("Quat", "IQuat.cs")));
        Assert.IsTrue(File.Exists(CoreFile("Box", "IBox.cs")));
        Assert.IsFalse(File.Exists(VecFile("IVec.g.cs")));
        Assert.IsFalse(File.Exists(VecFile("IVecMask.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2Mask.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3Mask.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4Mask.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2Floating.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3Floating.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4Floating.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2SignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3SignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4SignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2UnsignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3UnsignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4UnsignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("Vec2.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("Vec2.Swizzles.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("Vec4u128.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("Vec4u128.Swizzles.g.cs")));
        Assert.IsFalse(File.Exists(MatFile("IMat.g.cs")));
        Assert.IsFalse(File.Exists(MatFile("ProjectionHandedness.g.cs")));
        Assert.IsFalse(File.Exists(MatFile("ProjectionDepthRange.g.cs")));
        Assert.IsFalse(File.Exists(MatFile("IMat2.g.cs")));
        Assert.IsFalse(File.Exists(MatFile("IMat2x3.g.cs")));
        Assert.IsTrue(File.Exists(MatFile("Mat2.g.cs")));
        Assert.IsTrue(File.Exists(MatFile("Mat2d.g.cs")));
        Assert.IsTrue(File.Exists(MatFile("Mat3.g.cs")));
        Assert.IsTrue(File.Exists(MatFile("Mat4.g.cs")));
        Assert.IsTrue(File.Exists(MatFile("Mat4d.g.cs")));
        Assert.IsTrue(File.Exists(MatFile("Mat2x3.g.cs")));
        Assert.IsTrue(File.Exists(MatFile("Mat4x3d.g.cs")));
        Assert.IsFalse(File.Exists(QuatFile("IQuat.g.cs")));
        Assert.IsTrue(File.Exists(QuatFile("Quat.g.cs")));
        Assert.IsTrue(File.Exists(QuatFile("Quatd.g.cs")));
        Assert.IsFalse(File.Exists(BoxFile("IBox.g.cs")));
        Assert.IsTrue(File.Exists(BoxFile("Box2.g.cs")));
        Assert.IsTrue(File.Exists(BoxFile("Box2d.g.cs")));
        Assert.IsTrue(File.Exists(BoxFile("Box2i.g.cs")));
        Assert.IsTrue(File.Exists(BoxFile("Box3.g.cs")));
        Assert.IsTrue(File.Exists(BoxFile("Box3d.g.cs")));
        Assert.IsTrue(File.Exists(BoxFile("Box3i.g.cs")));
        Assert.IsFalse(File.Exists(MatFile("Mat2x2.g.cs")));
        Assert.IsFalse(File.Exists(MatFile("Mat3x3.g.cs")));
        Assert.IsFalse(File.Exists(MatFile("Mat4x4.g.cs")));
        Assert.IsFalse(File.Exists(QuatFile("Quath.g.cs")));
        Assert.IsFalse(File.Exists(BoxFile("Box4.g.cs")));
        Assert.IsFalse(File.Exists(BoxFile("Box2u.g.cs")));
        Assert.IsFalse(File.Exists(MatFile("Mat2i.g.cs")));
        Assert.IsFalse(File.Exists(MatFile("Mat4x3u128.g.cs")));
        var projectFile = File.ReadAllText(Path.Combine(project, "AlvorKit.Maths.Primitives.csproj"));
        StringAssert.Contains(projectFile, "<Version>9.8.7</Version>");
        StringAssert.Contains(projectFile, @"<ProjectReference Include=""..\..\..\src\AlvorKit.Maths.Core\AlvorKit.Maths.Core.csproj"" />");
        StringAssert.Contains(projectFile,
            "<Using Include=\"System.Diagnostics\" />");
    }

    /// <summary>Generated source includes 2D and 3D bounding boxes with formatting, parsing, and spatial helpers.</summary>
    [TestMethod]
    public void GenerateTo_EmitsExpectedBoxFeatures()
    {
        using var workspace = TempWorkspace.Create();
        var outputRoot = workspace.CreateDirectory("generated");

        MathsGenerator.GenerateTo(outputRoot, "9.8.7");

        var boxDirectory = Path.Combine(outputRoot, MathsGenerator.PrimitivesProjectName, MathsGenerator.BoxDirectoryName);
        var boxInterface = File.ReadAllText(CoreFile("Box", "IBox.cs"));
        var box2Interface = File.ReadAllText(CoreFile("Box", "IBox2.cs"));
        var box3Interface = File.ReadAllText(CoreFile("Box", "IBox3.cs"));
        var box2 = File.ReadAllText(Path.Combine(boxDirectory, "Box2.g.cs"));
        var box2d = File.ReadAllText(Path.Combine(boxDirectory, "Box2d.g.cs"));
        var box2i = File.ReadAllText(Path.Combine(boxDirectory, "Box2i.g.cs"));
        var box3i = File.ReadAllText(Path.Combine(boxDirectory, "Box3i.g.cs"));

        StringAssert.Contains(boxInterface, "public interface IBox<TSelf, TScalar, TVector>");
        StringAssert.Contains(boxInterface, "ISpanFormattable");
        StringAssert.Contains(boxInterface, "IUtf8SpanParsable<TSelf>");
        StringAssert.Contains(boxInterface, "static abstract ref TScalar ComponentRef(ref TSelf value, int index);");
        StringAssert.Contains(boxInterface, "static abstract TSelf CreateFromCenterSize(TVector center, TVector size);");
        StringAssert.Contains(boxInterface, "static abstract TSelf Union(TSelf left, TSelf right);");
        StringAssert.Contains(box2Interface, "public interface IBox2<TSelf, TScalar, TVector>");
        StringAssert.Contains(box2Interface, "TScalar Area { get; }");
        StringAssert.Contains(box3Interface, "public interface IBox3<TSelf, TScalar, TVector>");
        StringAssert.Contains(box3Interface, "TScalar Volume { get; }");
        StringAssert.Contains(box2, "public partial struct Box2(Vec2 min, Vec2 max)");
        StringAssert.Contains(box2, "IBox2<Box2, float, Vec2>");
        StringAssert.Contains(box2, "public static Box2 CreateFromCorners(Vec2 first, Vec2 second)");
        StringAssert.Contains(box2, "public readonly bool ContainsInclusive(Vec2 point)");
        StringAssert.Contains(box2, "public readonly bool ContainsHalfOpen(Vec2 point)");
        StringAssert.Contains(box2, "public readonly Box2 Inflated(Vec2 amount)");
        StringAssert.Contains(box2, "public readonly bool TryFormat(");
        StringAssert.Contains(box2, "public static Box2 Parse(ReadOnlySpan<char> s, IFormatProvider? formatProvider)");
        StringAssert.Contains(box2, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Box2 result)");
        StringAssert.Contains(box2, "public static implicit operator Box2d(Box2 value)");
        StringAssert.Contains(box2, "public static explicit operator Box2i(Box2 value)");
        StringAssert.Contains(box2d, "public static explicit operator Box2(Box2d value)");
        StringAssert.Contains(box2i, "public static implicit operator Box2(Box2i value)");
        StringAssert.Contains(box3i, "public readonly int Volume => Width * Height * Depth;");
        StringAssert.Contains(box3i, "public readonly float DistanceTo(Vec3i point)");
        Assert.IsFalse(box2.Contains("public Box2(float minX, float minY, float maxX, float maxY)", StringComparison.Ordinal));
        Assert.IsFalse(box2.Contains("public readonly bool Contains(float x, float y)", StringComparison.Ordinal));
        Assert.IsFalse(box2.Contains("public readonly Box2 Inflated(float amount)", StringComparison.Ordinal));
        Assert.IsFalse(box3i.Contains("public Box3i(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)", StringComparison.Ordinal));
        Assert.IsFalse(box2.Contains("Box2u", StringComparison.Ordinal));
        Assert.IsFalse(box3i.Contains("Box3d operator Box3i", StringComparison.Ordinal));
    }

    /// <summary>Generated source includes quaternions, rotation helpers, interpolation, and System.Numerics interop.</summary>
    [TestMethod]
    public void GenerateTo_EmitsExpectedQuaternionFeatures()
    {
        using var workspace = TempWorkspace.Create();
        var outputRoot = workspace.CreateDirectory("generated");

        MathsGenerator.GenerateTo(outputRoot, "9.8.7");

        var projectDirectory = Path.Combine(outputRoot, MathsGenerator.PrimitivesProjectName);
        var quatDirectory = Path.Combine(projectDirectory, MathsGenerator.QuatDirectoryName);
        var quatInterface = File.ReadAllText(CoreFile("Quat", "IQuat.cs"));
        var rotationInterface = File.ReadAllText(CoreFile("Quat", "IQuatRotation.cs"));
        var interpolationInterface = File.ReadAllText(CoreFile("Quat", "IQuatInterpolation.cs"));
        var systemInterface = File.ReadAllText(CoreFile("Quat", "IQuatSystemNumerics.cs"));
        var quat = File.ReadAllText(Path.Combine(quatDirectory, "Quat.g.cs"));
        var quatd = File.ReadAllText(Path.Combine(quatDirectory, "Quatd.g.cs"));
        var matDirectory = Path.Combine(projectDirectory, MathsGenerator.MatDirectoryName);
        var matrix3QuaternionInterface = File.ReadAllText(CoreFile("Mat", "IMat3QuaternionRotation.cs"));
        var matrix4QuaternionInterface = File.ReadAllText(CoreFile("Mat", "IMat4QuaternionRotation.cs"));
        var mat4 = File.ReadAllText(Path.Combine(matDirectory, "Mat4.g.cs"));
        var mat3 = File.ReadAllText(Path.Combine(matDirectory, "Mat3.g.cs"));
        var mat4d = File.ReadAllText(Path.Combine(matDirectory, "Mat4d.g.cs"));
        var mat3d = File.ReadAllText(Path.Combine(projectDirectory, MathsGenerator.MatDirectoryName, "Mat3d.g.cs"));

        StringAssert.Contains(quatInterface, "public interface IQuat<TSelf, TScalar, TVector3, TVector4, TMask, TMatrix3, TMatrix4>");
        StringAssert.Contains(quatInterface, "IUtf8SpanParsable<TSelf>");
        StringAssert.Contains(quatInterface, "static abstract ref TScalar ComponentRef(ref TSelf value, int index);");
        StringAssert.Contains(rotationInterface, "public interface IQuatRotation<TSelf, TScalar, TVector3, TMatrix3, TMatrix4>");
        StringAssert.Contains(rotationInterface, "static abstract TSelf LookRotation(TVector3 direction, TVector3 up);");
        StringAssert.Contains(interpolationInterface, "static abstract TSelf Slerp(TSelf from, TSelf to, TScalar amount);");
        StringAssert.Contains(interpolationInterface, "static abstract TSelf Exp(TSelf value);");
        StringAssert.Contains(systemInterface, "System.Numerics.Quaternion");
        StringAssert.Contains(matrix3QuaternionInterface,
            "public interface IMat3QuaternionRotation<TSelf, TScalar, TVector3, TQuaternion, TMatrix4>");
        StringAssert.Contains(matrix4QuaternionInterface,
            "public interface IMat4QuaternionRotation<TSelf, TScalar, TVector3, TVector4, TQuaternion, TMatrix3>");
        StringAssert.Contains(quat, "public struct Quat(float x, float y, float z, float w)");
        StringAssert.Contains(quat, "IQuat<Quat, float, Vec3, Vec4, Vec4b, Mat3, Mat4>");
        StringAssert.Contains(quat, "IQuatSystemNumerics<Quat>");
        StringAssert.Contains(quat, "public static Quat Identity => new(0f, 0f, 0f, 1f);");
        StringAssert.Contains(quat, "public static Quat CreateFromAxisAngle(Vec3 axis, float radians)");
        StringAssert.Contains(quat, "public static Quat CreateFromEulerAngles(Vec3 angles)");
        StringAssert.Contains(quat, "public static Quat CreateFromRotationMatrix(Mat4 matrix)");
        StringAssert.Contains(quat, "public static Vec3 TransformVector(Quat rotation, Vec3 vector)");
        StringAssert.Contains(quat, "public static Quat Slerp(Quat from, Quat to, float amount)");
        StringAssert.Contains(quat, "public static explicit operator System.Numerics.Quaternion(Quat value)");
        StringAssert.Contains(quat, "public static implicit operator Quatd(Quat value)");
        StringAssert.Contains(quatd, "IQuat<Quatd, double, Vec3d, Vec4d, Vec4b, Mat3d, Mat4d>");
        StringAssert.Contains(quatd, "public static explicit operator Quat(Quatd value)");
        Assert.IsFalse(quatd.Contains("IQuatSystemNumerics<Quatd>", StringComparison.Ordinal));
        StringAssert.Contains(mat3, "IMat3QuaternionRotation<Mat3, float, Vec3, Quat, Mat4>");
        StringAssert.Contains(mat4, "public static Mat4 CreateRotation(Quat rotation)");
        StringAssert.Contains(mat4d, "IMat4QuaternionRotation<Mat4d, double, Vec3d, Vec4d, Quatd, Mat3d>");
        StringAssert.Contains(mat3d, "public static Mat3d CreateRotation(Quatd rotation)");
        Assert.IsFalse(quat.Contains("public static Quat CreateFromEulerAngles(float pitch, float yaw, float roll)", StringComparison.Ordinal));
    }

    /// <summary>Generated source includes column-major matrix layout, algebra, and interop helpers.</summary>
    [TestMethod]
    public void GenerateTo_EmitsExpectedMatrixFeatures()
    {
        using var workspace = TempWorkspace.Create();
        var outputRoot = workspace.CreateDirectory("generated");

        MathsGenerator.GenerateTo(outputRoot, "9.8.7");

        var matDirectory = Path.Combine(outputRoot, MathsGenerator.PrimitivesProjectName, MathsGenerator.MatDirectoryName);
        var mat2 = File.ReadAllText(Path.Combine(matDirectory, "Mat2.g.cs"));
        var mat2x3 = File.ReadAllText(Path.Combine(matDirectory, "Mat2x3.g.cs"));
        var mat3x2 = File.ReadAllText(Path.Combine(matDirectory, "Mat3x2.g.cs"));
        var mat4 = File.ReadAllText(Path.Combine(matDirectory, "Mat4.g.cs"));
        var mat4d = File.ReadAllText(Path.Combine(matDirectory, "Mat4d.g.cs"));
        var projectionHandedness = File.ReadAllText(CoreFile("Mat", "ProjectionHandedness.cs"));
        var projectionDepthRange = File.ReadAllText(CoreFile("Mat", "ProjectionDepthRange.cs"));
        var matrixInterface = File.ReadAllText(CoreFile("Mat", "IMat.cs"));
        var matrixSquareInterface = File.ReadAllText(CoreFile("Mat", "IMatSquare.cs"));
        var matrixRelationalInterface = File.ReadAllText(CoreFile("Mat", "IMatRelationalOperators.cs"));
        var matrixQueryInterface = File.ReadAllText(CoreFile("Mat", "IMatQuery.cs"));
        var matrixTransform2DInterface = File.ReadAllText(CoreFile("Mat", "IMat3Transform2D.cs"));
        var matrix3x2Transform2DInterface = File.ReadAllText(CoreFile("Mat", "IMat3x2Transform2D.cs"));
        var matrix3x2SystemInterface = File.ReadAllText(CoreFile("Mat", "IMat3x2SystemNumerics.cs"));
        var matrixTransformInterface = File.ReadAllText(CoreFile("Mat", "IMat4Transform.cs"));
        StringAssert.Contains(projectionHandedness, "public enum ProjectionHandedness");
        StringAssert.Contains(projectionHandedness, "Right");
        StringAssert.Contains(projectionDepthRange, "public enum ProjectionDepthRange");
        StringAssert.Contains(projectionDepthRange, "NegativeOneToOne");
        StringAssert.Contains(matrixInterface, "public interface IMat<TSelf, TScalar, TColumn, TRow, TTranspose>");
        StringAssert.Contains(matrixInterface, "ISpanFormattable");
        StringAssert.Contains(matrixInterface, "IUtf8SpanParsable<TSelf>");
        StringAssert.Contains(matrixInterface, "static abstract ref TScalar ComponentRef(ref TSelf value, int column, int row);");
        StringAssert.Contains(matrixInterface, "static abstract TSelf CreateOuterProduct(TColumn columnVector, TRow rowVector);");
        StringAssert.Contains(matrixInterface, "static abstract TSelf Lerp(TSelf from, TSelf to, TScalar amount);");
        StringAssert.Contains(matrixSquareInterface, "public interface IMatSquare<TSelf, TScalar, TColumn>");
        StringAssert.Contains(matrixSquareInterface, "IMultiplicativeIdentity<TSelf, TSelf>");
        StringAssert.Contains(matrixSquareInterface, "static abstract TSelf Adjugate(TSelf value);");
        StringAssert.Contains(matrixSquareInterface, "static abstract TSelf InverseTranspose(TSelf value);");
        StringAssert.Contains(matrixSquareInterface, "TScalar Trace { get; }");
        StringAssert.Contains(matrixRelationalInterface, "public interface IMatRelationalOperators<TSelf, TScalar, TMask>");
        StringAssert.Contains(matrixQueryInterface, "public interface IMatQuery<TSelf, TScalar>");
        StringAssert.Contains(matrixTransform2DInterface, "public interface IMat3Transform2D<TSelf, TScalar, TVector2, TVector3>");
        StringAssert.Contains(matrix3x2Transform2DInterface,
            "public interface IMat3x2Transform2D<TSelf, TScalar, TVector2, TVector3, TTranspose>");
        StringAssert.Contains(matrix3x2SystemInterface, "public interface IMat3x2SystemNumerics<TSelf>");
        StringAssert.Contains(matrixTransformInterface, "public interface IMat4Transform<TSelf, TScalar, TVector2, TVector3, TVector4>");
        StringAssert.Contains(mat2, "public struct Mat2(Vec2 column0, Vec2 column1)");
        StringAssert.Contains(mat2, "IMat2<Mat2, float, Vec2, Vec2, Mat2>");
        StringAssert.Contains(mat2, "IMatScalarArithmeticOperators<Mat2, float>");
        StringAssert.Contains(mat2, "IMatRelationalOperators<Mat2, float, Vec2b>");
        StringAssert.Contains(mat2, "public Mat2(float m00, float m01, float m10, float m11)");
        StringAssert.Contains(mat2, "public static Mat2 Identity => new(1f);");
        StringAssert.Contains(mat2, "public static Mat2 CreateDiagonal(Vec2 diagonal)");
        StringAssert.Contains(mat2, "public Vec2 Diagonal");
        StringAssert.Contains(mat2, "public static Mat2 CreateOuterProduct(Vec2 columnVector, Vec2 rowVector)");
        StringAssert.Contains(mat2, "public static Mat2 Lerp(Mat2 from, Mat2 to, float amount)");
        StringAssert.Contains(mat2, "public readonly float Trace");
        StringAssert.Contains(mat2, "public readonly float Determinant");
        StringAssert.Contains(mat2, "public static bool TryInvert(Mat2 value, out Mat2 result)");
        StringAssert.Contains(mat2, "public static Mat2 Adjugate(Mat2 value)");
        StringAssert.Contains(mat2, "public static Mat2 InverseTranspose(Mat2 value)");
        StringAssert.Contains(mat2, "public static Vec2b Equal(Mat2 left, Mat2 right, float epsilon)");
        StringAssert.Contains(mat2, "public static bool IsIdentity(Mat2 value, float epsilon)");
        StringAssert.Contains(mat2, "public readonly bool TryFormat(");
        StringAssert.Contains(mat2, "public static Mat2 Parse(ReadOnlySpan<char> s, IFormatProvider? formatProvider)");
        StringAssert.Contains(mat2, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Mat2 result)");
        StringAssert.Contains(mat2, "public static Vec2 operator *(Mat2 left, Vec2 right)");
        StringAssert.Contains(mat2, "public static Mat3x2 operator *(Mat2 left, Mat3x2 right)");
        StringAssert.Contains(mat2, "public static Mat2 ComponentMultiply(Mat2 left, Mat2 right)");
        StringAssert.Contains(mat2x3, "public struct Mat2x3(Vec3 column0, Vec3 column1)");
        StringAssert.Contains(mat2x3, "public Vec2 Row0");
        StringAssert.Contains(mat2x3, "public static Mat2x3 CreateDiagonal(Vec2 diagonal)");
        StringAssert.Contains(mat2x3, "public readonly Mat3x2 Transposed");
        StringAssert.Contains(mat3x2, "IMat3x2SystemNumerics<Mat3x2>");
        StringAssert.Contains(mat3x2, "IMat3x2Transform2D<Mat3x2, float, Vec2, Vec3, Mat2x3>");
        StringAssert.Contains(mat3x2, "public static Mat3x2 AffineIdentity");
        StringAssert.Contains(mat3x2, "public static Mat3x2 CreateSkew(Vec2 radians)");
        StringAssert.Contains(mat3x2, "public static bool TryInvert(Mat3x2 value, out Mat3x2 result)");
        StringAssert.Contains(mat3x2, "public static explicit operator System.Numerics.Matrix3x2(Mat3x2 value)");
        Assert.IsFalse(mat2x3.Contains("public static Mat2x3 Identity", StringComparison.Ordinal));
        StringAssert.Contains(mat4, "public static explicit operator System.Numerics.Matrix4x4(Mat4 value)");
        StringAssert.Contains(mat4, "public static Mat4 CreateTranslation(Vec3 translation)");
        StringAssert.Contains(mat4, "public Vec3 Translation");
        StringAssert.Contains(mat4, "public static Mat4 CreateScale(Vec3 scale)");
        StringAssert.Contains(mat4, "public static Mat4 CreateRotationZ(float radians, Vec3 center)");
        StringAssert.Contains(mat4, "public static Mat4 CreateRotation(float radians, Vec3 axis)");
        StringAssert.Contains(mat4, "public static Mat4 CreateShear(Vec2 x, Vec2 y, Vec2 z)");
        StringAssert.Contains(mat4, "public static Mat4 CreateScaleBias(float scale, float bias)");
        StringAssert.Contains(mat4, "public static Mat4 CreatePerspective(");
        StringAssert.Contains(mat4, "public static Mat4 CreatePerspectiveOffCenter(");
        StringAssert.Contains(mat4, "public static Mat4 CreatePerspectiveFieldOfView(");
        StringAssert.Contains(mat4, "ProjectionHandedness.Right");
        StringAssert.Contains(mat4, "ProjectionDepthRange.NegativeOneToOne");
        StringAssert.Contains(mat4, "public static Mat4 CreateOrthographicOffCenter(");
        StringAssert.Contains(mat4, "public static Mat4 CreateOrthographic(");
        StringAssert.Contains(mat4, "public static Mat4 CreateFrustum(");
        StringAssert.Contains(mat4, "public static Mat4 CreateInfinitePerspective(");
        StringAssert.Contains(mat4, "public static Mat4 CreateTweakedInfinitePerspective(");
        StringAssert.Contains(mat4, "public static Vec3 Project(");
        StringAssert.Contains(mat4, "public static Vec3 UnProject(");
        StringAssert.Contains(mat4, "public static Mat4 PickMatrix");
        StringAssert.Contains(mat4, "public static Mat4 CreateViewport(");
        StringAssert.Contains(mat4, "Vec4 viewport");
        StringAssert.Contains(mat4, "public static Mat4 LookAt(Vec3 eye, Vec3 target, Vec3 up)");
        StringAssert.Contains(mat4, "public static Mat4 LookTo(Vec3 eye, Vec3 direction, Vec3 up)");
        StringAssert.Contains(mat4, "public static Mat4 CreateWorld(Vec3 position, Vec3 forward, Vec3 up)");
        StringAssert.Contains(mat4, "public readonly Vec3 ExtractScale()");
        StringAssert.Contains(mat4d, "IMat4Transform<Mat4d, double, Vec2d, Vec3d, Vec4d>");
        StringAssert.Contains(mat4d, "public static Mat4d CreateTranslation(Vec3d translation)");
        Assert.IsFalse(mat3x2.Contains("public static Mat3x2 CreateTranslation(float x, float y)", StringComparison.Ordinal));
        Assert.IsFalse(mat3x2.Contains("public static Mat3x2 CreateSkew(float radiansX, float radiansY)", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Mat4 CreateTranslation(float x, float y, float z)", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Mat4 CreateScale(float x, float y, float z)", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Mat4 CreateShearX(float y, float z)", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Mat4 CreateViewport(float x, float y, float width, float height)", StringComparison.Ordinal));
        Assert.IsFalse(matrixTransformInterface.Contains("RH_NO", StringComparison.Ordinal));
        Assert.IsFalse(matrixTransformInterface.Contains("RH_ZO", StringComparison.Ordinal));
        Assert.IsFalse(matrixTransformInterface.Contains("LH_NO", StringComparison.Ordinal));
        Assert.IsFalse(matrixTransformInterface.Contains("LH_ZO", StringComparison.Ordinal));
        Assert.IsFalse(matrixTransformInterface.Contains("LookAtRH", StringComparison.Ordinal));
        Assert.IsFalse(matrixTransformInterface.Contains("LookToRH", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Mat4 CreateFrustumRH_NO", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Mat4 CreatePerspectiveFieldOfViewRH_NO", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Mat4 CreateInfinitePerspectiveRH_NO", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Mat4 CreateTweakedInfinitePerspectiveRH_NO", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Vec3 ProjectNO", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Vec3 UnProjectNO", StringComparison.Ordinal));
        Assert.IsFalse(mat4.Contains("public static Mat4 LookAtRH", StringComparison.Ordinal));
        Assert.IsFalse(mat4d.Contains("public static Mat4d CreatePerspectiveFieldOfViewRH_NO", StringComparison.Ordinal));
    }

    /// <summary>Generated source includes tuple conversions, cross-scalar conversions, and read/write swizzles.</summary>
    [TestMethod]
    public void GenerateTo_EmitsExpectedVectorFeatures()
    {
        using var workspace = TempWorkspace.Create();
        var outputRoot = workspace.CreateDirectory("generated");

        MathsGenerator.GenerateTo(outputRoot, "9.8.7");

        var projectDirectory = Path.Combine(outputRoot, MathsGenerator.PrimitivesProjectName);
        var vecDirectory = Path.Combine(projectDirectory, MathsGenerator.VecDirectoryName);
        var scalarMath = File.ReadAllText(CoreFile("ScalarMath.cs"));
        var vectorInterface = File.ReadAllText(CoreFile("Vec", "IVec.cs"));
        var vec3Interface = File.ReadAllText(CoreFile("Vec", "IVec3.cs"));
        var numericInterface = File.ReadAllText(CoreFile("Vec", "IVecNumeric.cs"));
        var maskInterface = File.ReadAllText(CoreFile("Vec", "IVecMask.cs"));
        var vec3MaskInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3Mask.g.cs"));
        var integerInterface = File.ReadAllText(CoreFile("Vec", "IVecInteger.cs"));
        var integerCountShiftInterface = File.ReadAllText(CoreFile("Vec", "IVecIntegerCountShiftOperators.cs"));
        var floatingInterface = File.ReadAllText(CoreFile("Vec", "IVecFloating.cs"));
        var floatingGeometryInterface = File.ReadAllText(CoreFile("Vec", "IVecFloatingGeometry.cs"));
        var floatingScalarInterface = File.ReadAllText(CoreFile("Vec", "IVecFloatingScalarFunctions.cs"));
        var floatingVectorInterpolationInterface = File.ReadAllText(CoreFile("Vec", "IVecFloatingVectorInterpolation.cs"));
        var crossInterface = File.ReadAllText(CoreFile("Vec", "IVec3Cross.cs"));
        var scalarArithmeticInterface = File.ReadAllText(CoreFile("Vec", "IVecScalarArithmeticOperators.cs"));
        var scalarIntegerInterface = File.ReadAllText(CoreFile("Vec", "IVecScalarIntegerOperators.cs"));
        var vec2AxesInterface = File.ReadAllText(CoreFile("Vec", "IVec2Axes.cs"));
        var vec2PlanarInterface = File.ReadAllText(CoreFile("Vec", "IVec2Planar.cs"));
        var vec3FloatingInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3Floating.g.cs"));
        var vec3SignedIntegerInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3SignedInteger.g.cs"));
        var vec3UnsignedIntegerInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3UnsignedInteger.g.cs"));
        var vec3FloatingToIntegerInterface = File.ReadAllText(CoreFile("Vec", "IVec3FloatingToInteger.cs"));
        var vec3SystemNumericsInterface = File.ReadAllText(CoreFile("Vec", "IVec3SystemNumerics.cs"));
        var relationalInterface = File.ReadAllText(CoreFile("Vec", "IVecRelationalOperators.cs"));
        var vec2 = File.ReadAllText(Path.Combine(vecDirectory, "Vec2.g.cs"));
        var vec3 = File.ReadAllText(Path.Combine(vecDirectory, "Vec3.g.cs"));
        var vec3h = File.ReadAllText(Path.Combine(vecDirectory, "Vec3h.g.cs"));
        var vec3i8 = File.ReadAllText(Path.Combine(vecDirectory, "Vec3i8.g.cs"));
        var vec3u8 = File.ReadAllText(Path.Combine(vecDirectory, "Vec3u8.g.cs"));
        var vec3u16 = File.ReadAllText(Path.Combine(vecDirectory, "Vec3u16.g.cs"));
        var vec3i = File.ReadAllText(Path.Combine(vecDirectory, "Vec3i.g.cs"));
        var vec3u = File.ReadAllText(Path.Combine(vecDirectory, "Vec3u.g.cs"));
        var vec3u64 = File.ReadAllText(Path.Combine(vecDirectory, "Vec3u64.g.cs"));
        var vec3b = File.ReadAllText(Path.Combine(vecDirectory, "Vec3b.g.cs"));
        var vec4 = File.ReadAllText(Path.Combine(vecDirectory, "Vec4.g.cs"));
        var vec3Swizzles = File.ReadAllText(Path.Combine(vecDirectory, "Vec3.Swizzles.g.cs"));
        StringAssert.Contains(scalarMath, "public static class ScalarMath");
        StringAssert.Contains(scalarMath, "public static T Saturate<T>(T value)");
        StringAssert.Contains(scalarMath, "where T : IFloatingPointIeee754<T>");
        StringAssert.Contains(scalarMath, "public static int FindMostSignificantBit<T>(T value)");
        StringAssert.Contains(scalarMath, "Unsafe.SizeOf<T>()");
        StringAssert.Contains(vectorInterface, "public interface IVec<TSelf, TScalar>");
        StringAssert.Contains(vectorInterface, "IComparable<TSelf>");
        StringAssert.Contains(vectorInterface, "IEqualityOperators<TSelf, TSelf, bool>");
        StringAssert.Contains(vectorInterface, "ISpanFormattable");
        StringAssert.Contains(vectorInterface, "IUtf8SpanFormattable");
        StringAssert.Contains(vectorInterface, "ISpanParsable<TSelf>");
        StringAssert.Contains(vectorInterface, "IUtf8SpanParsable<TSelf>");
        StringAssert.Contains(vectorInterface, "Applies to all vector types");
        StringAssert.Contains(vectorInterface, "<c>Vec2</c>, <c>Vec3i</c>, <c>Vec4d</c>");
        StringAssert.Contains(vec3Interface, "static abstract implicit operator TSelf((TScalar X, TScalar Y, TScalar Z) value);");
        StringAssert.Contains(vec3Interface, "void Deconstruct(out TScalar x, out TScalar y, out TScalar z);");
        StringAssert.Contains(numericInterface, "public interface IVecNumeric<TSelf, TScalar, TMask, TLength, TArithmetic>");
        StringAssert.Contains(numericInterface, "Applies to all non-Boolean numeric vector types");
        StringAssert.Contains(maskInterface, "public interface IVecMask<TSelf>");
        StringAssert.Contains(maskInterface, "IBitwiseOperators<TSelf, TSelf, TSelf>");
        StringAssert.Contains(maskInterface, "Applies to all Boolean mask vector types");
        StringAssert.Contains(maskInterface, "static abstract TSelf False { get; }");
        StringAssert.Contains(maskInterface, "static abstract TSelf True { get; }");
        StringAssert.Contains(maskInterface, "static abstract TSelf operator ~(TSelf value);");
        StringAssert.Contains(vec3MaskInterface, "Vec3u128 Select(Vec3u128 whenTrue, Vec3u128 whenFalse);");
        StringAssert.Contains(integerInterface, "public interface IVecInteger<TSelf, TScalar, TMask, TCount, TLength, TArithmetic>");
        StringAssert.Contains(integerInterface, "IShiftOperators<TSelf, int, TArithmetic>");
        StringAssert.Contains(integerInterface, "IVecIntegerCountShiftOperators<TSelf, TCount, TArithmetic>");
        StringAssert.Contains(integerInterface, "static abstract TArithmetic operator >>>(TSelf left, int right);");
        StringAssert.Contains(integerCountShiftInterface, "public interface IVecIntegerCountShiftOperators<TSelf, TCount, TResult>");
        StringAssert.Contains(integerCountShiftInterface, "static abstract TResult operator >>>(TSelf left, TCount right);");
        StringAssert.Contains(floatingInterface, "public interface IVecFloating<TSelf, TScalar, TMask>");
        StringAssert.Contains(floatingGeometryInterface, "public interface IVecFloatingGeometry<TSelf, TScalar>");
        StringAssert.Contains(floatingScalarInterface, "public interface IVecFloatingScalarFunctions<TSelf, TScalar>");
        StringAssert.Contains(floatingVectorInterpolationInterface, "public interface IVecFloatingVectorInterpolation<TSelf>");
        StringAssert.Contains(crossInterface, "public interface IVec3Cross<TSelf, TScalar>");
        StringAssert.Contains(scalarArithmeticInterface, "public interface IVecScalarArithmeticOperators<TSelf, TScalar, TResult>");
        StringAssert.Contains(scalarArithmeticInterface, "IModulusOperators<TSelf, TScalar, TResult>");
        StringAssert.Contains(scalarArithmeticInterface, "static abstract TSelf Clamp(TSelf value, TScalar min, TScalar max);");
        StringAssert.Contains(scalarIntegerInterface, "public interface IVecScalarIntegerOperators<TSelf, TScalar, TResult>");
        StringAssert.Contains(vec2AxesInterface, "public interface IVec2Axes<TSelf>");
        StringAssert.Contains(vec2PlanarInterface, "public interface IVec2Planar<TSelf, TScalar>");
        StringAssert.Contains(vec3FloatingInterface, "public interface IVec3Floating<TSelf, TScalar, TMask>");
        StringAssert.Contains(vec3FloatingInterface, "IVecFloatingScalarFunctions<TSelf, TScalar>");
        StringAssert.Contains(vec3FloatingInterface, "IVec3FloatingToInteger<Vec3i>");
        StringAssert.Contains(vec3SignedIntegerInterface, "public interface IVec3SignedInteger<TSelf, TScalar, TMask, TCount, TLength, TArithmetic>");
        StringAssert.Contains(vec3UnsignedIntegerInterface,
            "public interface IVec3UnsignedInteger<TSelf, TScalar, TMask, TCount, TLength, TArithmetic>");
        StringAssert.Contains(vec3FloatingToIntegerInterface, "public interface IVec3FloatingToInteger<TInteger>");
        StringAssert.Contains(vec3SystemNumericsInterface, "public interface IVec3SystemNumerics<TSelf>");
        StringAssert.Contains(relationalInterface, "public interface IVecRelationalOperators<TSelf, TMask>");
        StringAssert.Contains(relationalInterface, "Applies to all numeric vector types with comparison operators");
        StringAssert.Contains(vec2, "IVec2Floating<Vec2, float, Vec2b>");
        StringAssert.Contains(vec2, "IVecScalarArithmeticOperators<Vec2, float, Vec2>");
        StringAssert.Contains(vec2, "IVec2SystemNumerics<Vec2>");
        StringAssert.Contains(vec3, "IVec3Floating<Vec3, float, Vec3b>");
        StringAssert.Contains(vec3, "[DebuggerDisplay(\"{ToString(),nq}\")]");
        StringAssert.Contains(vec3, "IVecScalarArithmeticOperators<Vec3, float, Vec3>");
        StringAssert.Contains(vec3i, "IVec3SignedInteger<Vec3i, int, Vec3b, Vec3i, float, Vec3i>");
        StringAssert.Contains(vec3i, "IVecScalarIntegerOperators<Vec3i, int, Vec3i>");
        StringAssert.Contains(vec3u, "IVec3UnsignedInteger<Vec3u, uint, Vec3b, Vec3i, float, Vec3u>");
        StringAssert.Contains(vec3u8, "IVec3UnsignedInteger<Vec3u8, byte, Vec3b, Vec3i, float, Vec3i>");
        StringAssert.Contains(vec3u8, "IVecScalarIntegerOperators<Vec3u8, byte, Vec3i>");
        StringAssert.Contains(vec3u, "public static Vec3 operator +(Vec3u left, float right)");
        StringAssert.Contains(vec3u, "public static Vec3 operator -(float left, Vec3u right)");
        StringAssert.Contains(vec3u, "public static Vec3 operator *(Vec3u left, float right)");
        StringAssert.Contains(vec3u, "public static Vec3 operator *(float left, Vec3u right)");
        StringAssert.Contains(vec3u, "public static Vec3d operator /(Vec3u left, double right)");
        StringAssert.Contains(vec3u, "public static Vec3d operator /(double left, Vec3u right)");
        StringAssert.Contains(vec3u, "public static Vec3i64 operator +(Vec3u left, Vec3i right)");
        StringAssert.Contains(vec3u, "public static Vec3i64 operator -(Vec3u value)");
        StringAssert.Contains(vec3i8, "public static Vec3i operator +(Vec3i8 left, Vec3i8 right)");
        StringAssert.Contains(vec3u8, "public static Vec3i operator +(Vec3u8 left, Vec3u8 right)");
        StringAssert.Contains(vec3u16, "public static Vec3i operator *(Vec3u16 left, ushort right)");
        StringAssert.Contains(vec3u8, "public static Vec3i operator ~(Vec3u8 value)");
        StringAssert.Contains(vec3u8, "public static Vec3i operator %(Vec3u8 left, byte right)");
        StringAssert.Contains(vec3u8, "public static Vec3i operator &(Vec3u8 left, byte right)");
        StringAssert.Contains(vec3u8, "public static Vec3i operator <<(Vec3u8 left, int right)");
        StringAssert.Contains(vec3u8, "public static Vec3i operator <<(Vec3u8 left, Vec3i right)");
        Assert.IsFalse(vec3u64.Contains("public static Vec3u64 operator +(Vec3u64 left, int right)", StringComparison.Ordinal));
        StringAssert.Contains(vec3b, "IVec3Mask<Vec3b>");
        StringAssert.Contains(vec3, "public static Vec3 Create(float value)");
        StringAssert.Contains(vec3, "public static Vec3 Create(float x, float y, float z)");
        StringAssert.Contains(vec3, "public readonly void CopyTo(Span<float> destination)");
        StringAssert.Contains(vec3, "public readonly bool TryFormat(");
        StringAssert.Contains(vec3, "public readonly int CompareTo(Vec3 other)");
        StringAssert.Contains(vec3, "public static Vec3 Parse(ReadOnlySpan<char> s, IFormatProvider? formatProvider)");
        StringAssert.Contains(vec3, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Vec3 result)");
        StringAssert.Contains(vec3, "var xComparison = X.CompareTo(other.X);");
        StringAssert.Contains(vec3, "var zComparison = Z.CompareTo(other.Z);");
        StringAssert.Contains(vec3, "if (!TryAppendUtf8(\"(\"u8, ref remainder, ref bytesWritten))");
        StringAssert.Contains(vec3, "return float.TryParse(source, formatProvider, out value);");
        StringAssert.Contains(vec3, "TryFormat(destination, out var charsWritten, format.AsSpan(), formatProvider)");
        StringAssert.Contains(vec3, "if (!TryAppend(\"(\".AsSpan(), ref remainder, ref charsWritten))");
        StringAssert.Contains(vec3, "if (!TryAppendComponent(X, ref remainder, ref charsWritten, format, formatProvider))");
        StringAssert.Contains(vec3, "public static implicit operator Vec3((float X, float Y, float Z) value)");
        StringAssert.Contains(vec3, "public static implicit operator Vec3(System.Numerics.Vector3 value)");
        StringAssert.Contains(vec3, "public static implicit operator System.Numerics.Vector3(Vec3 value)");
        StringAssert.Contains(vec3, "public static implicit operator Vec3(Vec3i value)");
        StringAssert.Contains(vec3, "public static Vec3d operator -(Vec3 left, double right)");
        StringAssert.Contains(vec3, "public static Vec3d operator *(Vec3 left, double right)");
        StringAssert.Contains(vec3, "public static Vec3d operator /(double left, Vec3 right)");
        StringAssert.Contains(vec3, "public static Vec3b operator <(Vec3 left, float right)");
        StringAssert.Contains(vec3, "public static Vec3b LessThan(Vec3 left, float right)");
        StringAssert.Contains(vec3, "public Vec3(Vec2 xy, float z)");
        StringAssert.Contains(vec4, "public Vec4(Vec2 xy, Vec2 zw)");
        StringAssert.Contains(vec4, "public static explicit operator Vec4(Vec4d value)");
        StringAssert.Contains(vec3, "public static Vec3b operator <(Vec3 left, Vec3 right)");
        StringAssert.Contains(vec3h, "public static Vec3b operator <=(Vec3h left, Half right)");
        Assert.IsFalse(vec3h.Contains("public static Vec3b operator <=(Vec3h left, short right)", StringComparison.Ordinal));
        StringAssert.Contains(vec3u8, "public static Vec3b LessThan(Vec3u8 left, Vec3i right)");
        StringAssert.Contains(vec3u, "public static Vec3b operator <(Vec3u left, int right)");
        StringAssert.Contains(vec3u, "public static Vec3b operator >(float left, Vec3u right)");
        StringAssert.Contains(vec3u, "public static Vec3b LessThan(Vec3u left, int right)");
        StringAssert.Contains(vec3u, "public static Vec3b GreaterThanOrEqual(float left, Vec3u right)");
        StringAssert.Contains(vec3, "public static Vec3 Sqrt(Vec3 value)");
        StringAssert.Contains(vec3, "ScalarMath.Sqrt(value.X)");
        StringAssert.Contains(vec3, "ScalarMath.SmoothStep(edge0.X, edge1.X, value.X)");
        StringAssert.Contains(vec3, "ScalarMath.Modulo(left.X, right.X)");
        StringAssert.Contains(vec3, "ScalarMath.IsFinite(value.X)");
        StringAssert.Contains(vec3i, "public static Vec3i operator >>>(Vec3i left, int right)");
        StringAssert.Contains(vec3i, "public static Vec3i operator >>>(Vec3i left, Vec3i right)");
        StringAssert.Contains(vec3i, "ScalarMath.BitCount(value.X)");
        Assert.IsFalse(vec3i.Contains("BitCountScalar", StringComparison.Ordinal));
        StringAssert.Contains(vec3u, "public static Vec3u operator >>>(Vec3u left, Vec3i right)");
        Assert.IsFalse(vec3u.Contains("public static Vec3u operator >>>(Vec3u left, Vec3u right)", StringComparison.Ordinal));
        StringAssert.Contains(vec3b, "IVec3Mask<Vec3b>");
        StringAssert.Contains(vec3b, "public static Vec3b operator ~(Vec3b value)");
        StringAssert.Contains(vec3b, "var text = value ? \"True\" : \"False\";");
        StringAssert.Contains(vec3b, "var text = value ? \"True\"u8 : \"False\"u8;");
        StringAssert.Contains(vec3b, "if (source.SequenceEqual(\"True\"u8))");
        StringAssert.Contains(vec3b, "public static bool operator true(Vec3b value)");
        StringAssert.Contains(vec3b, "public readonly Vec3u128 Select(Vec3u128 whenTrue, Vec3u128 whenFalse)");
        StringAssert.Contains(vec3b, "ScalarMath.Select(X, whenTrue.X, whenFalse.X)");
        Assert.IsFalse(vec3.Contains("public Vec3 YXZ", StringComparison.Ordinal));
        StringAssert.Contains(vec3Swizzles, "public partial struct Vec3");
        StringAssert.Contains(vec3Swizzles, "public Vec3 YXZ");
        StringAssert.Contains(vec3Swizzles, "public readonly Vec3 XXX");
        StringAssert.Contains(vec3Swizzles, "/// <summary>Gets or sets the YXZ swizzle.</summary>");
        StringAssert.Contains(vec3Swizzles, "[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
        Assert.IsFalse(vec3Swizzles.Contains("EditorBrowsable", StringComparison.Ordinal));
    }

    /// <summary>Configured type names follow the AlvorKit vector suffix convention.</summary>
    [TestMethod]
    public void VectorCatalog_UsesExpectedNames()
    {
        var names = VectorCatalog.Vectors.Select(vector => vector.TypeName).ToArray();

        CollectionAssert.Contains(names, "Vec2");
        CollectionAssert.Contains(names, "Vec3h");
        CollectionAssert.Contains(names, "Vec3i8");
        CollectionAssert.Contains(names, "Vec3u8");
        CollectionAssert.Contains(names, "Vec3i16");
        CollectionAssert.Contains(names, "Vec3u16");
        CollectionAssert.Contains(names, "Vec3i");
        CollectionAssert.Contains(names, "Vec3u");
        CollectionAssert.Contains(names, "Vec4d");
        CollectionAssert.Contains(names, "Vec4i64");
        CollectionAssert.Contains(names, "Vec4u64");
        CollectionAssert.Contains(names, "Vec4i128");
        CollectionAssert.Contains(names, "Vec4u128");
        CollectionAssert.Contains(names, "Vec2b");
    }

    /// <summary>Returns a source file path in the compile-checked maths core project.</summary>
    private static string CoreFile(params string[] segments)
    {
        var parts = new List<string> { RepositoryRoot(), "src", "AlvorKit.Maths.Core" };
        parts.AddRange(segments);
        return Path.Combine([.. parts]);
    }

    /// <summary>Finds the repository root from the current test process output directory.</summary>
    private static string RepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(current, "AlvorKit.slnx")))
            current = Directory.GetParent(current)?.FullName ?? throw new InvalidOperationException("Repository root was not found.");

        return current;
    }
}

namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated math source planning and focused source emission.</summary>
[TestClass]
public sealed class MathsGeneratorTest
{
    /// <summary>Configured type names follow the AlvorKit vector, matrix, quaternion, plane, and box suffix conventions.</summary>
    [TestMethod]
    public void Catalogs_UseExpectedNames()
    {
        var vectorNames = VectorCatalog.Vectors.Select(vector => vector.TypeName).ToArray();
        var matrixNames = MatrixCatalog.Matrices.Select(matrix => matrix.TypeName).ToArray();
        var quaternionNames = QuaternionCatalog.Quaternions.Select(quaternion => quaternion.TypeName).ToArray();
        var planeNames = PlaneCatalog.Planes.Select(plane => plane.TypeName).ToArray();
        var boxNames = BoxCatalog.Boxes.Select(box => box.TypeName).ToArray();

        Assert.AreEqual(42, vectorNames.Length);
        Assert.AreEqual(18, matrixNames.Length);
        Assert.AreEqual(2, quaternionNames.Length);
        Assert.AreEqual(2, planeNames.Length);
        Assert.AreEqual(6, boxNames.Length);
        CollectionAssert.Contains(vectorNames, "Vec3h");
        CollectionAssert.Contains(vectorNames, "Vec4u128");
        CollectionAssert.Contains(vectorNames, "Vec2b");
        CollectionAssert.Contains(matrixNames, "Mat4x3d");
        CollectionAssert.Contains(quaternionNames, "Quatd");
        CollectionAssert.Contains(planeNames, "Plane3d");
        CollectionAssert.Contains(boxNames, "Box3i");
    }

    /// <summary>Scalar specs expose naming and numeric metadata used across generated maths families.</summary>
    [TestMethod]
    public void ScalarSpecs_ExposeExpectedMetadata()
    {
        Assert.IsTrue(VectorCatalog.Bool.IsBool);
        Assert.IsTrue(VectorCatalog.Float.IsFloating);
        Assert.IsTrue(VectorCatalog.Int.IsInteger);
        Assert.IsTrue(VectorCatalog.Int.IsSigned);
        Assert.IsFalse(VectorCatalog.UInt.IsSigned);
        Assert.IsTrue(VectorCatalog.Half.RequiresArithmeticCast);
        Assert.IsFalse(VectorCatalog.Double.RequiresArithmeticCast);
        Assert.AreEqual(8, VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.Int8).BitWidth);
        Assert.AreEqual(8, VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.UInt8).BitWidth);
        Assert.AreEqual(16, VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.Int16).BitWidth);
        Assert.AreEqual(16, VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.UInt16).BitWidth);
        Assert.AreEqual(32, VectorCatalog.Int.BitWidth);
        Assert.AreEqual(32, VectorCatalog.UInt.BitWidth);
        Assert.AreEqual(64, VectorCatalog.Int64.BitWidth);
        Assert.AreEqual(64, VectorCatalog.UInt64.BitWidth);
        Assert.AreEqual(128, VectorCatalog.Int128.BitWidth);
        Assert.AreEqual(128, VectorCatalog.UInt128.BitWidth);
        Assert.AreEqual(0, VectorCatalog.Float.BitWidth);
        Assert.AreEqual("(Half)0", VectorCatalog.Half.ZeroLiteral);
        Assert.AreEqual("(byte)0", VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.UInt8).ZeroLiteral);
        Assert.AreEqual("0", VectorCatalog.Bool.ZeroLiteral);
        Assert.AreEqual("Vec3", VectorCatalog.Float.VectorName(3));
        Assert.AreEqual("Mat2x3", VectorCatalog.Float.MatrixName(2, 3));
        Assert.AreEqual("Quatd", VectorCatalog.Double.QuaternionName());
        Assert.AreEqual("Plane3", VectorCatalog.Float.PlaneName());
        Assert.AreEqual("Plane3d", VectorCatalog.Double.PlaneName());
        Assert.AreEqual("Box3i", VectorCatalog.Int.BoxName(3));
        Assert.AreEqual("(sbyte)(x + y)", VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.Int8).CastArithmetic("x + y"));
        Assert.AreEqual("x + y", VectorCatalog.Int.CastArithmetic("x + y"));
    }

    /// <summary>Floating vector source includes tuple conversions, System.Numerics interop, and parsing helpers.</summary>
    [TestMethod]
    public void VectorEmitter_EmitsExpectedFloatingVectorFeatures()
    {
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));

        StringAssert.Contains(vec3, "public partial struct Vec3(float x, float y, float z)");
        StringAssert.Contains(vec3, "IVec3Floating<Vec3, float, Vec3b>");
        StringAssert.Contains(vec3, "public static implicit operator Vec3((float X, float Y, float Z) value)");
        StringAssert.Contains(vec3, "public static implicit operator System.Numerics.Vector3(Vec3 value)");
        StringAssert.Contains(vec3, "public static Vec3d operator /(double left, Vec3 right)");
        StringAssert.Contains(vec3, "MathsParseHelper.TryParseComponent(body.Trim(), formatProvider, out float z)");
    }

    /// <summary>Unsigned integer vector source includes cross-scalar arithmetic helpers.</summary>
    [TestMethod]
    public void VectorEmitter_EmitsExpectedUnsignedIntegerVectorFeatures()
    {
        var vec3u = VectorFileEmitter.Emit(new(3, VectorCatalog.UInt));

        StringAssert.Contains(vec3u, "public static Vec3d operator /(Vec3u left, double right)");
        StringAssert.Contains(vec3u, "public static Vec3i64 operator +(Vec3u left, Vec3i right)");
    }

    /// <summary>Signed integer vector source includes count-vector shift helpers.</summary>
    [TestMethod]
    public void VectorEmitter_EmitsExpectedSignedIntegerVectorFeatures()
    {
        var vec3i = VectorFileEmitter.Emit(new(3, VectorCatalog.Int));

        StringAssert.Contains(vec3i, "public static Vec3i operator >>>(Vec3i left, Vec3i right)");
    }

    /// <summary>Boolean vector source includes mask selection helpers.</summary>
    [TestMethod]
    public void VectorEmitter_EmitsExpectedMaskVectorFeatures()
    {
        var vec3b = VectorFileEmitter.Emit(new(3, VectorCatalog.Bool));

        StringAssert.Contains(vec3b, "public readonly Vec3u128 Select(Vec3u128 whenTrue, Vec3u128 whenFalse)");
    }

    /// <summary>Swizzle source is emitted outside the primary vector file.</summary>
    [TestMethod]
    public void SwizzleEmitter_EmitsExpectedSwizzleFeatures()
    {
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec3Swizzles = SwizzleFileEmitter.Emit(new(3, VectorCatalog.Float));

        StringAssert.Contains(vec3Swizzles, "public Vec3 YXZ");
        Assert.IsFalse(vec3.Contains("public Vec3 YXZ", StringComparison.Ordinal));
    }

    /// <summary>Vector interface source includes shape-specific floating interfaces.</summary>
    [TestMethod]
    public void VectorInterfaceEmitter_EmitsExpectedInterfaceFeatures()
    {
        var vectorInterfaces = VectorInterfaceFileEmitter.EmitAll().ToDictionary(file => file.FileName, file => file.Source);

        StringAssert.Contains(vectorInterfaces["IVec3Floating.g.cs"], "IVec3FloatingToInteger<Vec3i>");
    }

    /// <summary>Matrix source includes column-major layout, algebra, transform, relation, and quaternion helpers.</summary>
    [TestMethod]
    public void MatrixEmitter_EmitsExpectedMatrixFeatures()
    {
        var mat2 = MatrixFileEmitter.Emit(new(2, 2, VectorCatalog.Float));
        var mat3 = MatrixFileEmitter.Emit(new(3, 3, VectorCatalog.Float));
        var mat3x2 = MatrixFileEmitter.Emit(new(3, 2, VectorCatalog.Float));
        var mat3x2d = MatrixFileEmitter.Emit(new(3, 2, VectorCatalog.Double));
        var mat4 = MatrixFileEmitter.Emit(new(4, 4, VectorCatalog.Float));
        var mat4d = MatrixFileEmitter.Emit(new(4, 4, VectorCatalog.Double));

        StringAssert.Contains(mat2, "public struct Mat2(Vec2 column0, Vec2 column1)");
        StringAssert.Contains(mat2, "IMat2<Mat2, float, Vec2, Vec2, Mat2>");
        StringAssert.Contains(mat2, "public static Mat2 CreateOuterProduct(Vec2 columnVector, Vec2 rowVector)");
        StringAssert.Contains(mat3, "IMat3QuaternionRotation<Mat3, float, Vec3, Quat, Mat4>");
        StringAssert.Contains(mat3x2, "IMat3x2Transform2D<Mat3x2, float, Vec2, Vec3, Mat2x3>");
        StringAssert.Contains(mat3x2, "IMat3x2SystemNumerics<Mat3x2>");
        StringAssert.Contains(mat3x2d, "IMat3x2Transform2D<Mat3x2d, double, Vec2d, Vec3d, Mat2x3d>");
        Assert.IsFalse(mat3x2d.Contains("IMat3x2SystemNumerics<Mat3x2d>", StringComparison.Ordinal));
        StringAssert.Contains(mat4, "IMat4Transform<Mat4, float, Vec2, Vec3, Vec4>");
        StringAssert.Contains(mat4, "public static Mat4 CreateRotation(Quat rotation)");
        StringAssert.Contains(mat4, "IMat4PlaneTransform<Mat4, float, Vec3, Vec4, Plane3>");
        StringAssert.Contains(mat4, "public static Mat4 CreateReflection(Plane3 plane)");
        StringAssert.Contains(mat4, "public static Mat4 CreatePerspectiveFieldOfView");
        StringAssert.Contains(mat4d, "IMat4QuaternionRotation<Mat4d, double, Vec3d, Vec4d, Quatd, Mat3d>");
        Assert.IsFalse(mat4d.Contains("IMat4SystemNumerics<Mat4d>", StringComparison.Ordinal));

        var nonFloatingMembers = new MemberBlock();
        MatrixTransformEmitter.Emit(new MatrixSpec(4, 4, VectorCatalog.Int), nonFloatingMembers);
        Assert.AreEqual("", nonFloatingMembers.ToString());
    }

    /// <summary>Quaternion source includes rotation helpers, interpolation, System.Numerics interop, and scalar conversions.</summary>
    [TestMethod]
    public void QuaternionEmitter_EmitsExpectedQuaternionFeatures()
    {
        var quat = QuaternionFileEmitter.Emit(new(VectorCatalog.Float));
        var quatd = QuaternionFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(quat, "/// <summary>Single-precision floating-point quaternion");
        StringAssert.Contains(quat, "public struct Quat(float x, float y, float z, float w)");
        StringAssert.Contains(quat, "IQuat<Quat, float, Vec3, Vec4, Vec4b, Mat3, Mat4>");
        StringAssert.Contains(quat, "IQuatSystemNumerics<Quat>");
        StringAssert.Contains(quat, "public static Quat CreateFromAxisAngle(Vec3 axis, float radians)");
        StringAssert.Contains(quat, "public static Quat CreateFromRotationMatrix(Mat4 matrix)");
        StringAssert.Contains(quat, "public static Quat Slerp(Quat from, Quat to, float amount)");
        StringAssert.Contains(quat, "public static explicit operator System.Numerics.Quaternion(Quat value)");
        StringAssert.Contains(quat, "public static implicit operator Quatd(Quat value)");
        StringAssert.Contains(quatd, "/// <summary>Double-precision floating-point quaternion");
        StringAssert.Contains(quatd, "IQuat<Quatd, double, Vec3d, Vec4d, Vec4b, Mat3d, Mat4d>");
        Assert.IsFalse(quatd.Contains("IQuatSystemNumerics<Quatd>", StringComparison.Ordinal));
    }

    /// <summary>Plane source includes point queries, transforms, System.Numerics interop, and scalar conversions.</summary>
    [TestMethod]
    public void PlaneEmitter_EmitsExpectedPlaneFeatures()
    {
        var plane = PlaneFileEmitter.Emit(new(VectorCatalog.Float));
        var planed = PlaneFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(plane, "/// <summary>Single-precision floating-point 3D plane");
        StringAssert.Contains(plane, "public struct Plane3(Vec3 normal, float offset)");
        StringAssert.Contains(plane, "IPlane3Transform<Plane3, float, Vec3, Vec4, Mat4, Quat>");
        StringAssert.Contains(plane, "IPlane3SystemNumerics<Plane3>");
        StringAssert.Contains(plane, "public static Plane3 Create(Vec3 normal, float offset)");
        StringAssert.Contains(plane, "public static Plane3 CreateFromPointNormal(Vec3 point, Vec3 normal)");
        StringAssert.Contains(plane, "public readonly Vec3 ProjectPoint(Vec3 point)");
        StringAssert.Contains(plane, "public static bool TryTransform(Plane3 plane, Mat4 matrix, out Plane3 result)");
        StringAssert.Contains(plane, "public static explicit operator System.Numerics.Plane(Plane3 value)");
        StringAssert.Contains(plane, "public static implicit operator Plane3d(Plane3 value)");
        StringAssert.Contains(planed, "/// <summary>Double-precision floating-point 3D plane");
        StringAssert.Contains(planed, "IPlane3Transform<Plane3d, double, Vec3d, Vec4d, Mat4d, Quatd>");
        Assert.IsFalse(planed.Contains("IPlane3SystemNumerics<Plane3d>", StringComparison.Ordinal));
        Assert.IsFalse(plane.Contains("public Plane3(float", StringComparison.Ordinal));
        Assert.IsFalse(plane.Contains("public static Plane3 Create(float", StringComparison.Ordinal));
    }

    /// <summary>Box source includes 2D and 3D spatial helpers, formatting, parsing, and scalar conversions.</summary>
    [TestMethod]
    public void BoxEmitter_EmitsExpectedBoxFeatures()
    {
        var box2 = BoxFileEmitter.Emit(new(2, VectorCatalog.Float));
        var box2d = BoxFileEmitter.Emit(new(2, VectorCatalog.Double));
        var box2i = BoxFileEmitter.Emit(new(2, VectorCatalog.Int));
        var box3i = BoxFileEmitter.Emit(new(3, VectorCatalog.Int));

        StringAssert.Contains(box2, "public partial struct Box2(Vec2 min, Vec2 max)");
        StringAssert.Contains(box2, "IBox2<Box2, float, Vec2>");
        StringAssert.Contains(box2, "public static Box2 CreateFromCorners(Vec2 first, Vec2 second)");
        StringAssert.Contains(box2, "public readonly bool ContainsInclusive(Vec2 point)");
        StringAssert.Contains(box2, "public readonly bool TryFormat(");
        StringAssert.Contains(box2, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Box2 result)");
        StringAssert.Contains(box2, "public static implicit operator Box2d(Box2 value)");
        StringAssert.Contains(box2d, "public static explicit operator Box2(Box2d value)");
        StringAssert.Contains(box2i, "public static implicit operator Box2(Box2i value)");
        StringAssert.Contains(box3i, "public readonly int Volume => Width * Height * Depth;");
        StringAssert.Contains(box3i, "public readonly float DistanceTo(Vec3i point)");
        Assert.IsFalse(box2.Contains("public Box2(float minX, float minY, float maxX, float maxY)", StringComparison.Ordinal));
        Assert.IsFalse(box3i.Contains("Box3d operator Box3i", StringComparison.Ordinal));
    }

    /// <summary>Generated XML documentation summaries use sentence-case descriptions without broad marketing phrases.</summary>
    [TestMethod]
    public void GeneratedDocumentation_UsesClearDescriptionWording()
    {
        foreach (var (fileName, source) in DocumentationSampleSources())
        {
            foreach (var line in source.Split(Environment.NewLine))
            {
                var description = XmlDescription(line, "<summary>")
                    ?? XmlDescription(line, "<remarks>")
                    ?? XmlDescription(line, "<returns>");
                if (description is null || description.Length == 0)
                    continue;

                Assert.IsFalse(char.IsAsciiLetterLower(description[0]), $"{fileName}: {line.Trim()}");
                AssertDescriptionDoesNotContain(description, "game engine", fileName, line);
                AssertDescriptionDoesNotContain(description, "game math", fileName, line);
                AssertDescriptionDoesNotContain(description, "graphics APIs", fileName, line);
            }
        }
    }

    private static IEnumerable<(string FileName, string Source)> DocumentationSampleSources()
    {
        foreach (var (fileName, source) in VectorInterfaceFileEmitter.EmitAll())
            yield return (fileName, source);

        yield return ("Vec2.g.cs", VectorFileEmitter.Emit(new(2, VectorCatalog.Float)));
        yield return ("Vec2h.g.cs", VectorFileEmitter.Emit(new(2, VectorCatalog.Half)));
        yield return ("Vec2i64.g.cs", VectorFileEmitter.Emit(new(2, VectorCatalog.Int64)));
        yield return ("Vec4.g.cs", VectorFileEmitter.Emit(new(4, VectorCatalog.Float)));
        yield return ("Vec4.Swizzles.g.cs", SwizzleFileEmitter.Emit(new(4, VectorCatalog.Float)));
        yield return ("Mat4.g.cs", MatrixFileEmitter.Emit(new(4, 4, VectorCatalog.Float)));
        yield return ("Quat.g.cs", QuaternionFileEmitter.Emit(new(VectorCatalog.Float)));
        yield return ("Quatd.g.cs", QuaternionFileEmitter.Emit(new(VectorCatalog.Double)));
        yield return ("Plane3.g.cs", PlaneFileEmitter.Emit(new(VectorCatalog.Float)));
        yield return ("Plane3d.g.cs", PlaneFileEmitter.Emit(new(VectorCatalog.Double)));
        yield return ("Box3i.g.cs", BoxFileEmitter.Emit(new(3, VectorCatalog.Int)));
    }

    private static string? XmlDescription(string line, string tag)
    {
        var prefix = $"/// {tag}";
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
            return null;

        return trimmed[prefix.Length..].TrimStart();
    }

    private static void AssertDescriptionDoesNotContain(string description, string phrase, string fileName, string line) =>
        Assert.IsFalse(description.Contains(phrase, StringComparison.OrdinalIgnoreCase), $"{fileName}: {line.Trim()}");
}

namespace AlvorKit.Maths.Test;

/// <summary>Tests generated matrix types.</summary>
[TestClass]
public sealed class GeneratedMatrixTest
{
    /// <summary>Generated matrices use column-major storage and expose row-major helpers explicitly.</summary>
    [TestMethod]
    public void GeneratedMatrices_UseColumnMajorLayout()
    {
        var matrix = new Mat3(
            1f, 2f, 3f,
            4f, 5f, 6f,
            7f, 8f, 9f);
        Span<float> columnMajor = stackalloc float[9];
        Span<float> rowMajor = stackalloc float[9];

        matrix.CopyToColumnMajor(columnMajor);
        matrix.CopyToRowMajor(rowMajor);

        Assert.AreEqual(new Vec3(1f, 2f, 3f), matrix.Column0);
        Assert.AreEqual(new Vec3(4f, 5f, 6f), matrix.Column1);
        Assert.AreEqual(new Vec3(7f, 8f, 9f), matrix.Column2);
        Assert.AreEqual(new Vec3(1f, 4f, 7f), matrix.Row0);
        CollectionAssert.AreEqual(new[] { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f }, columnMajor.ToArray());
        CollectionAssert.AreEqual(new[] { 1f, 4f, 7f, 2f, 5f, 8f, 3f, 6f, 9f }, rowMajor.ToArray());
        Assert.AreEqual(matrix, Mat3.FromRowMajor(rowMajor));
    }

    /// <summary>Generated matrix multiplication follows column-vector algebra for vectors and rectangular matrices.</summary>
    [TestMethod]
    public void GeneratedMatrixMultiplication_FollowsColumnVectorAlgebra()
    {
        var left = Mat2.CreateRows(new Vec2(1f, 2f), new Vec2(3f, 4f));
        var right = Mat2.CreateRows(new Vec2(7f, 8f), new Vec2(9f, 10f));
        var rectangularLeft = Mat2x3.CreateRows(new Vec2(1f, 2f), new Vec2(3f, 4f), new Vec2(5f, 6f));
        var rectangularRight = Mat4x2.CreateRows(new Vec4(1f, 2f, 3f, 4f), new Vec4(5f, 6f, 7f, 8f));

        Assert.AreEqual(new Vec2(17f, 39f), left * new Vec2(5f, 6f));
        Assert.AreEqual(new Vec2(23f, 34f), new Vec2(5f, 6f) * left);
        Assert.AreEqual(Mat2.CreateRows(new Vec2(25f, 28f), new Vec2(57f, 64f)), left * right);
        Assert.AreEqual(new Vec3(23f, 53f, 83f), rectangularLeft * new Vec2(7f, 8f));
        Assert.AreEqual(
            Mat4x3.CreateRows(new Vec4(11f, 14f, 17f, 20f), new Vec4(23f, 30f, 37f, 44f), new Vec4(35f, 46f, 57f, 68f)),
            rectangularLeft * rectangularRight);
    }

    /// <summary>Generated square matrices expose transpose, determinant, and inverse helpers.</summary>
    [TestMethod]
    public void GeneratedSquareMatrixAlgebra_Works()
    {
        var matrix = Mat3.CreateRows(new Vec3(1f, 0f, 5f), new Vec3(2f, 1f, 6f), new Vec3(3f, 4f, 0f));
        var invertible = new Mat2(4f, 7f, 2f, 6f);
        var basis = Mat3.CreateColumns(new Vec3(1f, 0f, 0f), new Vec3(1f, 1f, 0f), new Vec3(1f, 1f, 1f));

        Assert.AreEqual(10f, invertible.Trace);
        Assert.AreEqual(1f, matrix.Determinant);
        Assert.AreEqual(new Mat3(matrix.Row0, matrix.Row1, matrix.Row2), matrix.Transposed);
        Assert.IsTrue(Mat2.TryInvert(invertible, out var inverse));
        AssertMatrixClose(new Mat2(0.6f, -0.7f, -0.2f, 0.4f), inverse);
        AssertMatrixClose(Mat2.Identity, invertible * inverse);
        Assert.IsFalse(Mat2.TryInvert(new Mat2(1f, 2f, 2f, 4f), out _));
        Assert.AreEqual(new Mat2(6f, -7f, -2f, 4f), Mat2.Adjugate(invertible));
        AssertMatrixClose(Mat2.Invert(invertible).Transposed, Mat2.InverseTranspose(invertible));
        AssertMatrixClose(Mat3.Identity, Mat3.Orthonormalize(basis));
    }

    /// <summary>Generated matrix helpers expose diagonal access, outer products, and interpolation.</summary>
    [TestMethod]
    public void GeneratedMatrixCommonHelpers_Work()
    {
        var diagonal = Mat3x2.CreateDiagonal(new Vec2(2f, 3f));
        var editable = new Mat3x2
        {
            Diagonal = new Vec2(5f, 6f),
        };
        var outer = Mat3x2.CreateOuterProduct(new Vec2(2f, 3f), new Vec3(4f, 5f, 6f));
        var lerped = Mat2.Lerp(Mat2.Zero, new Mat2(2f, 4f, 6f, 8f), 0.25f);

        Assert.AreEqual(new Vec2(2f, 3f), diagonal.Diagonal);
        Assert.AreEqual(Mat3x2.CreateDiagonal(new Vec2(5f, 6f)), editable);
        Assert.AreEqual(
            new Mat3x2(new Vec2(8f, 12f), new Vec2(10f, 15f), new Vec2(12f, 18f)),
            outer);
        Assert.AreEqual(new Mat2(0.5f, 1f, 1.5f, 2f), lerped);
    }

    /// <summary>Generated Mat4 graphics helpers follow column-major transform conventions.</summary>
    [TestMethod]
    public void GeneratedMat4TransformHelpers_Work()
    {
        var translated = Mat4.CreateTranslation(new Vec3(2f, 3f, 4f)) * new Vec4(1f, 2f, 3f, 1f);
        var rotated = Mat4.CreateRotationZ(float.Pi / 2f) * new Vec4(1f, 0f, 0f, 1f);
        var centered = Mat4.CreateRotationZ(float.Pi, new Vec3(1f, 1f, 0f)) * new Vec4(2f, 1f, 0f, 1f);
        var projection = Mat4.CreatePerspectiveFieldOfView(float.Pi / 2f, 1f, 1f, 11f);
        var lookAt = Mat4.LookAt(new Vec3(0f, 0f, 1f), Vec3.Zero, Vec3.UnitY);
        var lookTo = Mat4.LookTo(new Vec3(0f, 0f, 1f), new Vec3(0f, 0f, -1f), Vec3.UnitY);
        var mutable = Mat4.Identity;
        mutable.Translation = new Vec3(2f, 3f, 4f);

        AssertVecClose(new Vec4(3f, 5f, 7f, 1f), translated);
        AssertVecClose(new Vec4(0f, 1f, 0f, 1f), rotated);
        AssertVecClose(new Vec4(0f, 1f, 0f, 1f), centered);
        AssertMatrixClose(Mat4.LookAt(new Vec3(0f, 0f, 1f), Vec3.Zero, Vec3.UnitY, ProjectionHandedness.Right), lookAt);
        AssertMatrixClose(lookAt, lookTo);
        Assert.AreEqual(new Vec3(2f, 3f, 4f), mutable.Translation);
        Assert.AreEqual(-1f, projection[2, 3]);
        AssertClose(-1.2f, projection[2, 2]);
        AssertClose(-2.2f, projection[3, 2]);
    }

    /// <summary>Generated double-precision Mat4 transform helpers mirror the single-precision transform API.</summary>
    [TestMethod]
    public void GeneratedMat4dTransformHelpers_Work()
    {
        var translated = Mat4d.CreateTranslation(new Vec3d(2d, 3d, 4d)) * new Vec4d(1d, 2d, 3d, 1d);
        var rotated = Mat4d.CreateRotationZ(double.Pi / 2d) * new Vec4d(1d, 0d, 0d, 1d);
        var projection = Mat4d.CreatePerspectiveFieldOfView(double.Pi / 2d, 1d, 1d, 11d);
        var lookAt = Mat4d.LookAt(new Vec3d(0d, 0d, 1d), Vec3d.Zero, Vec3d.UnitY);

        AssertVecClose(new Vec4d(3d, 5d, 7d, 1d), translated);
        AssertVecClose(new Vec4d(0d, 1d, 0d, 1d), rotated);
        AssertMatrixClose(Mat4d.LookAt(new Vec3d(0d, 0d, 1d), Vec3d.Zero, Vec3d.UnitY, ProjectionHandedness.Right), lookAt);
        Assert.AreEqual(-1d, projection[2, 3]);
        AssertClose(-1.2d, projection[2, 2]);
        AssertClose(-2.2d, projection[3, 2]);
    }

    /// <summary>Generated Mat4 GLM-style transform helpers work with axis rotations, shears, and cross-product matrices.</summary>
    [TestMethod]
    public void GeneratedMat4AdditionalTransformHelpers_Work()
    {
        var axisRotated = Mat4.CreateRotation(float.Pi / 2f, Vec3.UnitZ) * new Vec4(1f, 0f, 0f, 1f);
        var translated = Mat4.Translate(Mat4.Identity, new Vec3(2f, 3f, 4f)) * new Vec4(1f, 2f, 3f, 1f);
        var scaled = Mat4.Scale(Mat4.Identity, new Vec3(2f, 3f, 4f)) * new Vec4(1f, 2f, 3f, 1f);
        var sheared = Mat4.CreateShear(new Vec2(1f, 0f), new Vec2(0f, 0f), new Vec2(0f, 0f)) * new Vec4(2f, 3f, 4f, 1f);
        var shearX = Mat4.CreateShear(Vec2.Zero, new Vec2(1f, 0f), new Vec2(2f, 0f)) * new Vec4(2f, 3f, 4f, 1f);
        var scaleBias = Mat4.CreateScaleBias(0.5f, 0.5f) * new Vec4(2f, 4f, 6f, 1f);
        var world = Mat4.CreateWorld(new Vec3(2f, 3f, 4f), Vec3.UnitZ, Vec3.UnitY);
        var extractedScale = Mat4.CreateScale(new Vec3(2f, 3f, 4f)).ExtractScale();
        var withoutTranslation = Mat4.CreateTranslation(new Vec3(2f, 3f, 4f)).WithoutTranslation();
        var transformedPoint = Mat4.TransformPoint(Mat4.CreateTranslation(new Vec3(2f, 3f, 4f)), new Vec3(1f, 2f, 3f));
        var transformedVector = Mat4.TransformVector(Mat4.CreateTranslation(new Vec3(2f, 3f, 4f)), new Vec3(1f, 2f, 3f));
        var crossed = Mat4.MatrixCross3(new Vec3(1f, 2f, 3f)) * new Vec3(4f, 5f, 6f);

        AssertVecClose(new Vec4(0f, 1f, 0f, 1f), axisRotated);
        AssertVecClose(new Vec4(3f, 5f, 7f, 1f), translated);
        AssertVecClose(new Vec4(2f, 6f, 12f, 1f), scaled);
        AssertVecClose(new Vec4(5f, 3f, 4f, 1f), sheared);
        AssertVecClose(new Vec4(2f, 5f, 8f, 1f), shearX);
        AssertVecClose(new Vec4(1.5f, 2.5f, 3.5f, 1f), scaleBias);
        AssertVecClose(new Vec4(2f, 3f, 4f, 1f), world * new Vec4(0f, 0f, 0f, 1f));
        AssertVecClose(new Vec3(2f, 3f, 4f), extractedScale);
        AssertMatrixClose(Mat4.Identity, withoutTranslation);
        AssertVecClose(new Vec3(3f, 5f, 7f), transformedPoint);
        AssertVecClose(new Vec3(1f, 2f, 3f), transformedVector);
        AssertVecClose(Vec3.Cross(new Vec3(1f, 2f, 3f), new Vec3(4f, 5f, 6f)), crossed);
    }

    /// <summary>Generated Mat4 projection helpers cover frustums, infinite projections, project, unproject, and picking.</summary>
    [TestMethod]
    public void GeneratedMat4ProjectionUtilities_Work()
    {
        var perspective = Mat4.CreatePerspectiveFieldOfView(float.Pi / 2f, 1f, 1f, 11f);
        var defaultPerspective = Mat4.CreatePerspectiveFieldOfView(float.Pi / 2f, 1f, 1f, 11f);
        var explicitPerspective = Mat4.CreatePerspectiveFieldOfView(
            float.Pi / 2f,
            1f,
            1f,
            11f,
            ProjectionHandedness.Left,
            ProjectionDepthRange.ZeroToOne);
        var frustum = Mat4.CreateFrustum(-1f, 1f, -1f, 1f, 1f, 11f);
        var defaultFrustum = Mat4.CreateFrustum(-1f, 1f, -1f, 1f, 1f, 11f);
        var perspectiveFromSize = Mat4.CreatePerspective(2f, 2f, 1f, 11f);
        var perspectiveOffCenter = Mat4.CreatePerspectiveOffCenter(-1f, 1f, -1f, 1f, 1f, 11f);
        var fovBySize = Mat4.CreatePerspectiveFieldOfView(float.Pi / 2f, 2f, 2f, 1f, 11f);
        var defaultFovBySize = Mat4.CreatePerspectiveFieldOfView(float.Pi / 2f, 2f, 2f, 1f, 11f);
        var orthographic = Mat4.CreateOrthographic(2f, 2f, 1f, 11f);
        var defaultOrthographic = Mat4.CreateOrthographicOffCenter(-1f, 1f, -1f, 1f, 1f, 11f);
        var infinite = Mat4.CreateInfinitePerspective(float.Pi / 2f, 1f, 1f);
        var defaultInfinite = Mat4.CreateInfinitePerspective(float.Pi / 2f, 1f, 1f);
        var tweaked = Mat4.CreateTweakedInfinitePerspective(float.Pi / 2f, 1f, 1f, 0.001f);
        var defaultTweaked = Mat4.CreateTweakedInfinitePerspective(float.Pi / 2f, 1f, 1f, 0.001f);
        var viewport = new Vec4(0f, 0f, 100f, 100f);
        var projectedNO = Mat4.Project(new Vec3(0.25f, -0.25f, 0.5f), Mat4.Identity, Mat4.Identity, viewport);
        var projectedDefault = Mat4.Project(new Vec3(0.25f, -0.25f, 0.5f), Mat4.Identity, Mat4.Identity, viewport);
        var projectedZO = Mat4.Project(
            new Vec3(0.25f, -0.25f, 0.5f),
            Mat4.Identity,
            Mat4.Identity,
            viewport,
            ProjectionDepthRange.ZeroToOne);
        var unprojectedNO = Mat4.UnProject(projectedNO, Mat4.Identity, Mat4.Identity, viewport);
        var unprojectedDefault = Mat4.UnProject(projectedNO, Mat4.Identity, Mat4.Identity, viewport);
        var unprojectedZO = Mat4.UnProject(projectedZO, Mat4.Identity, Mat4.Identity, viewport, ProjectionDepthRange.ZeroToOne);
        var viewportTransform = Mat4.CreateViewport(viewport) * new Vec4(0.25f, -0.25f, 0.5f, 1f);
        var pick = Mat4.PickMatrix(new Vec2(50f, 50f), new Vec2(50f, 50f), viewport);

        AssertMatrixClose(perspective, defaultPerspective);
        Assert.AreEqual(1f, explicitPerspective[2, 3]);
        AssertClose(1.1f, explicitPerspective[2, 2]);
        AssertClose(-1.1f, explicitPerspective[3, 2]);
        AssertMatrixClose(perspective, frustum);
        AssertMatrixClose(frustum, defaultFrustum);
        AssertMatrixClose(frustum, perspectiveFromSize);
        AssertMatrixClose(frustum, perspectiveOffCenter);
        AssertMatrixClose(perspective, fovBySize);
        AssertMatrixClose(fovBySize, defaultFovBySize);
        AssertMatrixClose(defaultOrthographic, orthographic);
        AssertMatrixClose(
            Mat4.CreateOrthographicOffCenter(
                -1f,
                1f,
                -1f,
                1f,
                1f,
                11f,
                ProjectionHandedness.Right,
                ProjectionDepthRange.NegativeOneToOne),
            defaultOrthographic);
        AssertMatrixClose(infinite, defaultInfinite);
        AssertMatrixClose(tweaked, defaultTweaked);
        Assert.AreEqual(-1f, infinite[2, 2]);
        Assert.AreEqual(-2f, infinite[3, 2]);
        AssertClose(-0.999f, tweaked[2, 2]);
        AssertClose(-1.999f, tweaked[3, 2]);
        AssertVecClose(new Vec3(62.5f, 37.5f, 0.75f), projectedNO);
        AssertVecClose(projectedNO, projectedDefault);
        AssertVecClose(new Vec3(62.5f, 37.5f, 0.5f), projectedZO);
        AssertVecClose(new Vec3(0.25f, -0.25f, 0.5f), unprojectedNO);
        AssertVecClose(unprojectedNO, unprojectedDefault);
        AssertVecClose(new Vec3(0.25f, -0.25f, 0.5f), unprojectedZO);
        AssertVecClose(new Vec4(62.5f, 37.5f, 0.75f, 1f), viewportTransform);
        AssertVecClose(new Vec4(2f, 2f, 0f, 1f), pick * new Vec4(1f, 1f, 0f, 1f));
        Assert.AreEqual(Mat4.Identity, Mat4.PickMatrix(new Vec2(0f), new Vec2(0f), viewport));
    }

    /// <summary>Generated Mat3 2D transform helpers use column-vector transform composition.</summary>
    [TestMethod]
    public void GeneratedMat3Transform2DHelpers_Work()
    {
        var transformed = Mat3.CreateTranslation2D(new Vec2(4f, 5f)) * Mat3.CreateScale2D(new Vec2(2f, 3f)) * new Vec3(1f, 1f, 1f);
        var rotated = Mat3.CreateRotation2D(float.Pi / 2f) * new Vec3(1f, 0f, 1f);
        var translated = Mat3.Translate2D(Mat3.Identity, new Vec2(2f, 3f)) * new Vec3(1f, 1f, 1f);
        var centeredRotation = Mat3.CreateRotation2D(float.Pi, new Vec2(1f, 1f)) * new Vec3(2f, 1f, 1f);
        var skew = Mat3.CreateSkew2D(new Vec2(MathF.Atan(2f), 0f)) * new Vec3(3f, 4f, 1f);
        var shearX = Mat3.CreateShearX2D(2f) * new Vec3(3f, 4f, 1f);
        var shearY = Mat3.CreateShearY2D(2f) * new Vec3(3f, 4f, 1f);
        var mutable = Mat3.Identity;
        mutable.Translation2D = new Vec2(2f, 3f);

        AssertVecClose(new Vec3(6f, 8f, 1f), transformed);
        AssertVecClose(new Vec3(0f, 1f, 1f), rotated);
        AssertVecClose(new Vec3(3f, 4f, 1f), translated);
        AssertVecClose(new Vec3(0f, 1f, 1f), centeredRotation);
        AssertVecClose(new Vec3(11f, 4f, 1f), skew);
        AssertVecClose(new Vec3(3f, 10f, 1f), shearX);
        AssertVecClose(new Vec3(11f, 4f, 1f), shearY);
        Assert.AreEqual(new Vec2(2f, 3f), mutable.Translation2D);
        AssertVecClose(new Vec2(3f, 4f), Mat3.TransformPoint2D(mutable, new Vec2(1f, 1f)));
        AssertVecClose(new Vec2(1f, 1f), Mat3.TransformVector2D(mutable, new Vec2(1f, 1f)));
    }

    /// <summary>Generated Mat3x2 compact 2D affine helpers mirror System.Numerics-style transforms.</summary>
    [TestMethod]
    public void GeneratedMat3x2Transform2DHelpers_Work()
    {
        var transform = Mat3x2.CreateTranslation(new Vec2(4f, 5f)) * Mat3x2.CreateScale(new Vec2(2f, 3f));
        var transformed = Mat3x2.TransformPoint(transform, new Vec2(1f, 1f));
        var vector = Mat3x2.TransformVector(transform, new Vec2(1f, 1f));
        var centeredRotation = Mat3x2.CreateRotation(float.Pi, new Vec2(1f, 1f));
        var skew = Mat3x2.CreateSkew(new Vec2(MathF.Atan(2f), 0f));
        var mutable = Mat3x2.AffineIdentity;
        mutable.Translation = new Vec2(2f, 3f);

        AssertVecClose(new Vec2(6f, 8f), transformed);
        AssertVecClose(new Vec2(2f, 3f), vector);
        AssertVecClose(new Vec2(0f, 1f), Mat3x2.TransformPoint(centeredRotation, new Vec2(2f, 1f)));
        AssertVecClose(new Vec2(11f, 4f), Mat3x2.TransformPoint(skew, new Vec2(3f, 4f)));
        Assert.AreEqual(new Vec2(2f, 3f), mutable.Translation);
        AssertMatrixClose(Mat3x2.AffineIdentity, transform * transform.Inverted);
        Assert.IsTrue(Mat3x2.TryInvert(transform, out var inverse));
        AssertMatrixClose(Mat3x2.AffineIdentity, inverse * transform);
        Assert.IsFalse(Mat3x2.TryInvert(new Mat3x2(new Vec2(1f, 2f), new Vec2(2f, 4f), Vec2.Zero), out _));
    }

    /// <summary>Generated matrix relation and query helpers provide masks and tolerance-based checks.</summary>
    [TestMethod]
    public void GeneratedMatrixRelationAndQueryHelpers_Work()
    {
        var almostIdentity = new Mat2(1.0001f, 0f, 0f, 1f);
        var rotation = new Mat2(0f, 1f, -1f, 0f);

        Assert.IsTrue(Mat2.Equal(Mat2.Identity, almostIdentity, 0.001f).All);
        Assert.IsTrue(Mat2.NotEqual(Mat2.Identity, almostIdentity, 0.00001f).Any);
        Assert.IsTrue(Mat2.Zero.IsNull(0f));
        Assert.IsTrue(Mat2.Identity.IsIdentity(0f));
        Assert.IsFalse(Mat2.Zero.IsIdentity(0.001f));
        Assert.IsTrue(rotation.IsNormalized(0f));
        Assert.IsTrue(rotation.IsOrthogonal(0f));
    }

    /// <summary>Generated affine inverse helpers invert common 2D and 3D transform matrices.</summary>
    [TestMethod]
    public void GeneratedAffineInverseHelpers_Work()
    {
        var transform2D = Mat3.CreateTranslation2D(new Vec2(2f, 3f)) * Mat3.CreateScale2D(new Vec2(4f, 5f));
        var transform3D = Mat4.CreateTranslation(new Vec3(2f, 3f, 4f)) * Mat4.CreateScale(new Vec3(5f, 6f, 7f));

        AssertMatrixClose(Mat3.Identity, transform2D * Mat3.AffineInverse(transform2D));
        AssertMatrixClose(Mat4.Identity, transform3D * Mat4.AffineInverse(transform3D));
    }

    /// <summary>Generated matrix formatting uses nested column-vector text and supports span and UTF-8 destinations.</summary>
    [TestMethod]
    public void GeneratedMatrixFormatting_UsesColumnVectorStyle()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;
        var value = new Mat2(1f, 2f, 3f, 4f);
        Span<char> destination = stackalloc char[16];
        Span<char> tooSmall = stackalloc char[15];
        Span<byte> utf8Destination = stackalloc byte[16];
        Span<byte> utf8TooSmall = stackalloc byte[15];

        Assert.IsTrue(value.TryFormat(destination, out var charsWritten, default, formatProvider));
        Assert.AreEqual(16, charsWritten);
        Assert.AreEqual("((1, 2), (3, 4))", destination[..charsWritten].ToString());
        Assert.IsTrue(value.TryFormat(utf8Destination, out var bytesWritten, default, formatProvider));
        Assert.AreEqual(16, bytesWritten);
        Assert.AreEqual("((1, 2), (3, 4))", System.Text.Encoding.UTF8.GetString(utf8Destination[..bytesWritten]));
        Assert.AreEqual("((1.0, 2.0), (3.0, 4.0))", value.ToString("0.0", formatProvider));
        Assert.IsFalse(value.TryFormat(tooSmall, out var shortCharsWritten, default, formatProvider));
        Assert.AreEqual(0, shortCharsWritten);
        Assert.IsFalse(value.TryFormat(utf8TooSmall, out var shortBytesWritten, default, formatProvider));
        Assert.AreEqual(0, shortBytesWritten);
    }

    /// <summary>Generated matrix parsing accepts nested column-vector text from strings, spans, and UTF-8 byte spans.</summary>
    [TestMethod]
    public void GeneratedMatrixParsing_AcceptsColumnVectorStyle()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;

        Assert.AreEqual(new Mat2(1f, 2f, 3f, 4.5f), Mat2.Parse("((1.0, 2.0), (3.0, 4.5))", formatProvider));
        Assert.AreEqual(new Mat2d(1d, 2d, 3d, 4.5d), ParseUtf8<Mat2d>("((1.0, 2.0), (3.0, 4.5))"u8));
        Assert.IsTrue(Mat2.TryParse("((5, 6), (7, 8))", formatProvider, out var parsed));
        Assert.AreEqual(new Mat2(5f, 6f, 7f, 8f), parsed);
        Assert.IsFalse(Mat2.TryParse((string?)null, formatProvider, out _));
        Assert.IsFalse(Mat2.TryParse("((1,2), (3, 4))", formatProvider, out _));
        Assert.IsFalse(Mat2.TryParse("((1, 2) (3, 4))", formatProvider, out _));
        Assert.IsFalse(Mat2.TryParse("((1, 2) (3, 4))"u8, formatProvider, out _));
        Assert.IsFalse(Mat2.TryParse("((bad, 2), (3, 4))"u8, formatProvider, out _));
        Assert.ThrowsException<FormatException>(() => Mat2.Parse("((1,2), (3, 4))", formatProvider));
    }

    /// <summary>Generated matrix interfaces expose shape, copying, identities, operators, algebra, and interop to generic code.</summary>
    [TestMethod]
    public void GeneratedMatrixInterfaces_Work()
    {
        var matrix = CreateMat2<Mat2, float, Vec2, Vec2, Mat2>(new Vec2(1f, 2f), new Vec2(3f, 4f));
        Span<float> copied = stackalloc float[4];
        SetColumn<Mat2, float, Vec2, Vec2, Mat2>(ref matrix, 1, new Vec2(5f, 6f));
        SetComponent<Mat2, float, Vec2, Vec2, Mat2>(ref matrix, 0, 1, 9f);
        CopyGeneric<Mat2, float, Vec2, Vec2, Mat2>(matrix, copied);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = Mat2.ColumnRef(ref matrix, 2));

        Assert.AreEqual(2, ColumnCount<Mat2, float, Vec2, Vec2, Mat2>());
        Assert.AreEqual(2, RowCount<Mat2, float, Vec2, Vec2, Mat2>());
        Assert.AreEqual(4, ComponentCount<Mat2, float, Vec2, Vec2, Mat2>());
        Assert.AreEqual(16, SizeInBytes<Mat2, float, Vec2, Vec2, Mat2>());
        Assert.AreEqual(new Mat2(1f, 9f, 5f, 6f), matrix);
        CollectionAssert.AreEqual(new[] { 1f, 9f, 5f, 6f }, copied.ToArray());
        Assert.AreEqual(Mat2.Zero, AdditiveIdentity<Mat2>());
        Assert.AreEqual(Mat2.Identity, MultiplicativeIdentity<Mat2>());
        Assert.AreEqual(new Mat2(2f, 10f, 6f, 7f), AddScalarRight<Mat2, float>(matrix, 1f));
        Assert.AreEqual(new Mat2(9f, 1f, 5f, 4f), SubtractScalarLeft<Mat2, float>(10f, matrix));
        Assert.AreEqual(new Mat2(2f, 18f, 10f, 12f), AddMatrix<Mat2>(matrix, matrix));
        Assert.AreEqual(new Mat2(1f, 81f, 25f, 36f), ComponentMultiply<Mat2, float, Vec2, Vec2, Mat2>(matrix, matrix));
        Assert.AreEqual(new Mat2(1f, 5f, 9f, 6f), Transpose<Mat2, float, Vec2, Vec2, Mat2>(matrix));
        Assert.AreEqual(new Vec2(11f, 21f), TransformColumn<Mat2, float, Vec2, Vec2, Mat2>(matrix, new Vec2(1f, 2f)));
        Assert.AreEqual(new Vec2(19f, 17f), TransformRow<Mat2, float, Vec2, Vec2, Mat2>(new Vec2(1f, 2f), matrix));
        Assert.IsTrue(InvertSquare<Mat2, float, Vec2>(new Mat2(4f, 7f, 2f, 6f), out var inverse));
        AssertMatrixClose(new Mat2(0.6f, -0.7f, -0.2f, 0.4f), inverse);

        var translated = Translation<Mat4, float, Vec2, Vec3, Vec4>(new Vec3(1f, 2f, 3f)) * new Vec4(1f, 1f, 1f, 1f);
        var system = ToSystemNumerics<Mat4>(Mat4.Identity);
        Assert.AreEqual(new Vec4(2f, 3f, 4f, 1f), translated);
        Assert.AreEqual(1f, system.M11);
        Assert.AreEqual(Mat4.Identity, FromSystemNumerics<Mat4>(system));
    }

    /// <summary>Generated Mat4 converts explicitly to and from row-vector System.Numerics matrices.</summary>
    [TestMethod]
    public void GeneratedMat4SystemNumericsConversion_PreservesRows()
    {
        var matrix = Mat4.CreateRows(
            new Vec4(1f, 2f, 3f, 4f),
            new Vec4(5f, 6f, 7f, 8f),
            new Vec4(9f, 10f, 11f, 12f),
            new Vec4(13f, 14f, 15f, 16f));

        var system = (Matrix4x4)matrix;
        var roundTrip = (Mat4)system;

        Assert.AreEqual(1f, system.M11);
        Assert.AreEqual(2f, system.M12);
        Assert.AreEqual(5f, system.M21);
        Assert.AreEqual(16f, system.M44);
        Assert.AreEqual(matrix, roundTrip);
    }

    /// <summary>Generated Mat3x2 converts explicitly to and from row-vector System.Numerics matrices.</summary>
    [TestMethod]
    public void GeneratedMat3x2SystemNumericsConversion_PreservesRows()
    {
        var matrix = Mat3x2.CreateRows(
            new Vec3(1f, 2f, 3f),
            new Vec3(4f, 5f, 6f));

        var system = (Matrix3x2)matrix;
        var roundTrip = (Mat3x2)system;
        var genericSystem = ToSystemNumerics3x2<Mat3x2>(matrix);
        Vec2 systemTransformed = Vector2.Transform(new Vector2(7f, 8f), system);

        Assert.AreEqual(1f, system.M11);
        Assert.AreEqual(4f, system.M12);
        Assert.AreEqual(2f, system.M21);
        Assert.AreEqual(5f, system.M22);
        Assert.AreEqual(3f, system.M31);
        Assert.AreEqual(6f, system.M32);
        Assert.AreEqual(matrix, roundTrip);
        Assert.AreEqual(matrix, FromSystemNumerics3x2<Mat3x2>(genericSystem));
        Assert.AreEqual(matrix * new Vec3(7f, 8f, 1f), systemTransformed);
    }

    private static void AssertMatrixClose(Mat2 expected, Mat2 actual)
    {
        for (var column = 0; column < Mat2.ColumnCount; column++)
        {
            for (var row = 0; row < Mat2.RowCount; row++)
                AssertClose(expected[column, row], actual[column, row]);
        }
    }

    private static void AssertMatrixClose(Mat3 expected, Mat3 actual)
    {
        for (var column = 0; column < Mat3.ColumnCount; column++)
        {
            for (var row = 0; row < Mat3.RowCount; row++)
                AssertClose(expected[column, row], actual[column, row]);
        }
    }

    private static void AssertMatrixClose(Mat3x2 expected, Mat3x2 actual)
    {
        for (var column = 0; column < Mat3x2.ColumnCount; column++)
        {
            for (var row = 0; row < Mat3x2.RowCount; row++)
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

    private static void AssertMatrixClose(Mat4d expected, Mat4d actual)
    {
        for (var column = 0; column < Mat4d.ColumnCount; column++)
        {
            for (var row = 0; row < Mat4d.RowCount; row++)
                AssertClose(expected[column, row], actual[column, row]);
        }
    }

    private static void AssertVecClose(Vec2 expected, Vec2 actual)
    {
        for (var index = 0; index < Vec2.ComponentCount; index++)
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

    private static void AssertVecClose(Vec4d expected, Vec4d actual)
    {
        for (var index = 0; index < Vec4d.ComponentCount; index++)
            AssertClose(expected[index], actual[index]);
    }

    private static void AssertClose(float expected, float actual) =>
        Assert.AreEqual(expected, actual, 0.0001f);

    private static void AssertClose(double expected, double actual) =>
        Assert.AreEqual(expected, actual, 0.0001d);

    private static TMatrix ParseUtf8<TMatrix>(ReadOnlySpan<byte> source)
        where TMatrix : IUtf8SpanParsable<TMatrix> =>
        TMatrix.Parse(source, System.Globalization.CultureInfo.InvariantCulture);

    private static int ColumnCount<TMatrix, TScalar, TColumn, TRow, TTranspose>()
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        TMatrix.ColumnCount;

    private static int RowCount<TMatrix, TScalar, TColumn, TRow, TTranspose>()
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        TMatrix.RowCount;

    private static int ComponentCount<TMatrix, TScalar, TColumn, TRow, TTranspose>()
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        TMatrix.ComponentCount;

    private static int SizeInBytes<TMatrix, TScalar, TColumn, TRow, TTranspose>()
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        TMatrix.SizeInBytes;

    private static TMatrix CreateMat2<TMatrix, TScalar, TColumn, TRow, TTranspose>(TColumn column0, TColumn column1)
        where TMatrix : struct, IMat2<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        TMatrix.CreateColumns(column0, column1);

    private static void SetColumn<TMatrix, TScalar, TColumn, TRow, TTranspose>(ref TMatrix value, int column, TColumn columnValue)
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        TMatrix.ColumnRef(ref value, column) = columnValue;

    private static void SetComponent<TMatrix, TScalar, TColumn, TRow, TTranspose>(
        ref TMatrix value,
        int column,
        int row,
        TScalar component)
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        TMatrix.ComponentRef(ref value, column, row) = component;

    private static void CopyGeneric<TMatrix, TScalar, TColumn, TRow, TTranspose>(TMatrix value, Span<TScalar> destination)
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        value.CopyTo(destination);

    private static TMatrix AdditiveIdentity<TMatrix>()
        where TMatrix : IAdditiveIdentity<TMatrix, TMatrix> =>
        TMatrix.AdditiveIdentity;

    private static TMatrix MultiplicativeIdentity<TMatrix>()
        where TMatrix : IMultiplicativeIdentity<TMatrix, TMatrix> =>
        TMatrix.MultiplicativeIdentity;

    private static TMatrix AddScalarRight<TMatrix, TScalar>(TMatrix left, TScalar right)
        where TMatrix : IAdditionOperators<TMatrix, TScalar, TMatrix> =>
        left + right;

    private static TMatrix SubtractScalarLeft<TMatrix, TScalar>(TScalar left, TMatrix right)
        where TMatrix : struct, IMatScalarArithmeticOperators<TMatrix, TScalar> =>
        left - right;

    private static TMatrix AddMatrix<TMatrix>(TMatrix left, TMatrix right)
        where TMatrix : IAdditionOperators<TMatrix, TMatrix, TMatrix> =>
        left + right;

    private static TMatrix ComponentMultiply<TMatrix, TScalar, TColumn, TRow, TTranspose>(TMatrix left, TMatrix right)
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        TMatrix.ComponentMultiply(left, right);

    private static TTranspose Transpose<TMatrix, TScalar, TColumn, TRow, TTranspose>(TMatrix value)
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        TMatrix.Transpose(value);

    private static TColumn TransformColumn<TMatrix, TScalar, TColumn, TRow, TTranspose>(TMatrix left, TRow right)
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        left * right;

    private static TRow TransformRow<TMatrix, TScalar, TColumn, TRow, TTranspose>(TColumn left, TMatrix right)
        where TMatrix : struct, IMat<TMatrix, TScalar, TColumn, TRow, TTranspose>
        where TColumn : struct, IVec<TColumn, TScalar>
        where TRow : struct, IVec<TRow, TScalar>
        where TTranspose : struct =>
        left * right;

    private static bool InvertSquare<TMatrix, TScalar, TColumn>(TMatrix value, out TMatrix result)
        where TMatrix : struct, IMatSquare<TMatrix, TScalar, TColumn>
        where TColumn : struct, IVec<TColumn, TScalar> =>
        TMatrix.TryInvert(value, out result);

    private static TMatrix Translation<TMatrix, TScalar, TVector2, TVector3, TVector4>(TVector3 translation)
        where TMatrix : struct, IMat4Transform<TMatrix, TScalar, TVector2, TVector3, TVector4>
        where TVector2 : struct, IVec2<TVector2, TScalar>
        where TVector3 : struct, IVec3<TVector3, TScalar>
        where TVector4 : struct, IVec4<TVector4, TScalar> =>
        TMatrix.CreateTranslation(translation);

    private static System.Numerics.Matrix4x4 ToSystemNumerics<TMatrix>(TMatrix value)
        where TMatrix : struct, IMat4SystemNumerics<TMatrix> =>
        (System.Numerics.Matrix4x4)value;

    private static TMatrix FromSystemNumerics<TMatrix>(System.Numerics.Matrix4x4 value)
        where TMatrix : struct, IMat4SystemNumerics<TMatrix> =>
        (TMatrix)value;

    private static System.Numerics.Matrix3x2 ToSystemNumerics3x2<TMatrix>(TMatrix value)
        where TMatrix : struct, IMat3x2SystemNumerics<TMatrix> =>
        (System.Numerics.Matrix3x2)value;

    private static TMatrix FromSystemNumerics3x2<TMatrix>(System.Numerics.Matrix3x2 value)
        where TMatrix : struct, IMat3x2SystemNumerics<TMatrix> =>
        (TMatrix)value;
}

namespace AlvorKit.OpenGL.Maths.Test;

[TestClass]
public sealed class PublicApiAndLayoutTest
{
    /// <summary>The bridge exposes the locked 226-method surface without misleading region overloads.</summary>
    [TestMethod]
    public void PublicApiMatchesLockedCatalog()
    {
        var extensionMethods = typeof(GlStateMathsExtensions).Assembly
            .GetExportedTypes()
            .SelectMany(static type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(static method => method.IsDefined(typeof(ExtensionAttribute), false))
            .ToArray();

        var expected = ExpectedPublicApi.Create();
        var actual = extensionMethods.Select(ExpectedPublicApi.Format).ToHashSet(StringComparer.Ordinal);

        Assert.HasCount(226, extensionMethods);
        Assert.HasCount(226, expected);
        Assert.HasCount(226, actual);
        Assert.IsTrue(
            expected.SetEquals(actual),
            $"Missing:{Environment.NewLine}{string.Join(Environment.NewLine, expected.Except(actual))}"
            + $"{Environment.NewLine}Unexpected:{Environment.NewLine}{string.Join(Environment.NewLine, actual.Except(expected))}");
    }

    /// <summary>Every matrix type used by span reinterpretation has a dense scalar layout.</summary>
    [TestMethod]
    public void MatrixLayoutsAreDenseAndContiguous()
    {
        AssertFloatLayout<Mat2>(Mat2.ComponentCount);
        AssertFloatLayout<Mat2x3>(Mat2x3.ComponentCount);
        AssertFloatLayout<Mat2x4>(Mat2x4.ComponentCount);
        AssertFloatLayout<Mat3x2>(Mat3x2.ComponentCount);
        AssertFloatLayout<Mat3>(Mat3.ComponentCount);
        AssertFloatLayout<Mat3x4>(Mat3x4.ComponentCount);
        AssertFloatLayout<Mat4x2>(Mat4x2.ComponentCount);
        AssertFloatLayout<Mat4x3>(Mat4x3.ComponentCount);
        AssertFloatLayout<Mat4>(Mat4.ComponentCount);

        AssertDoubleLayout<Mat2d>(Mat2d.ComponentCount);
        AssertDoubleLayout<Mat2x3d>(Mat2x3d.ComponentCount);
        AssertDoubleLayout<Mat2x4d>(Mat2x4d.ComponentCount);
        AssertDoubleLayout<Mat3x2d>(Mat3x2d.ComponentCount);
        AssertDoubleLayout<Mat3d>(Mat3d.ComponentCount);
        AssertDoubleLayout<Mat3x4d>(Mat3x4d.ComponentCount);
        AssertDoubleLayout<Mat4x2d>(Mat4x2d.ComponentCount);
        AssertDoubleLayout<Mat4x3d>(Mat4x3d.ComponentCount);
        AssertDoubleLayout<Mat4d>(Mat4d.ComponentCount);
    }

    /// <summary>Every vector, quaternion, and interval reinterpreted by the bridge has dense scalar storage.</summary>
    [TestMethod]
    public void ValueLayoutsAreDenseAndContiguous()
    {
        AssertFloatLayout<Vec2>(2);
        AssertFloatLayout<Vec3>(3);
        AssertFloatLayout<Vec4>(4);
        AssertDoubleLayout<Vec2d>(2);
        AssertDoubleLayout<Vec3d>(3);
        AssertDoubleLayout<Vec4d>(4);
        AssertIntLayout<Vec2i>(2);
        AssertIntLayout<Vec3i>(3);
        AssertIntLayout<Vec4i>(4);
        AssertUIntLayout<Vec2u>(2);
        AssertUIntLayout<Vec3u>(3);
        AssertUIntLayout<Vec4u>(4);
        AssertFloatLayout<Quat>(4);
        AssertDoubleLayout<Quatd>(4);
        AssertDoubleLayout<Intervald>(2);
    }

    /// <summary>Representative bridge calls allocate no managed memory after warm-up.</summary>
    [TestMethod]
    public void RepresentativeSuccessPathsAllocateNothing()
    {
        var gl = new GlNoop();
        var matrix = Mat4.Identity;
        Vec3 value = (1f, 2f, 3f);

        InvokeRepresentativeCalls(gl, in matrix, value);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 128; i++)
            InvokeRepresentativeCalls(gl, in matrix, value);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(0L, allocated);
    }

    private static void AssertFloatLayout<T>(int components) where T : struct
    {
        Assert.AreEqual(components * sizeof(float), Unsafe.SizeOf<T>());
        Assert.AreEqual(components * 2, MemoryMarshal.Cast<T, float>(new T[2]).Length);
    }

    private static void AssertDoubleLayout<T>(int components) where T : struct
    {
        Assert.AreEqual(components * sizeof(double), Unsafe.SizeOf<T>());
        Assert.AreEqual(components * 2, MemoryMarshal.Cast<T, double>(new T[2]).Length);
    }

    private static void AssertIntLayout<T>(int components) where T : struct
    {
        Assert.AreEqual(components * sizeof(int), Unsafe.SizeOf<T>());
        Assert.AreEqual(components * 2, MemoryMarshal.Cast<T, int>(new T[2]).Length);
    }

    private static void AssertUIntLayout<T>(int components) where T : struct
    {
        Assert.AreEqual(components * sizeof(uint), Unsafe.SizeOf<T>());
        Assert.AreEqual(components * 2, MemoryMarshal.Cast<T, uint>(new T[2]).Length);
    }

    private static void InvokeRepresentativeCalls(Gl gl, in Mat4 matrix, Vec3 value)
    {
        gl.ClearColor((1f, 0.5f, 0.25f, 1f));
        gl.Viewport((1920u, 1080u));
        gl.Uniform3f(2, value);
        gl.UniformMatrix4fv(3, in matrix);
        gl.VertexAttribPointer<Vec3>(0, false, 12, 0);
    }
}

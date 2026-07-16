namespace AlvorKit.OpenGL.Maths.Test;

[TestClass]
public sealed class MathsForwardingTest
{
    /// <summary>State overloads forward exact values while preventing lossy unsigned-to-signed conversion.</summary>
    [TestMethod]
    public void StateOverloadsForwardAndProtectConversions()
    {
        var gl = new RecordingGl();

        gl.ClearColor((1f, 2f, 3f, 4f));
        Assert.AreEqual(new Vec4(1f, 2f, 3f, 4f), gl.LastColor);

        gl.ColorMask((true, false, true, false));
        Assert.AreEqual(new Vec4b(true, false, true, false), gl.LastMask);

        gl.Viewport((12u, 34u));
        Assert.AreEqual(new Vec4i(0, 0, 12, 34), gl.LastRegion);

        Vec4i[] scissors = [(4, 5, 6, 7)];
        gl.ScissorArrayv(2, scissors);
        Assert.AreEqual(scissors[0], gl.LastRegion);

        Vec4i[] negativeScissors = [(0, 0, -1, 1)];
        gl.ScissorArrayv(0, negativeScissors);
        Assert.AreEqual(negativeScissors[0], gl.LastRegion);

        var calls = gl.CallCount;
        Assert.ThrowsExactly<OverflowException>(() => gl.Viewport((uint.MaxValue, 1u)));
        Assert.AreEqual(calls, gl.CallCount);
    }

    /// <summary>Vector and matrix overloads preserve component order, count, and column-major matrix storage.</summary>
    [TestMethod]
    public void UniformOverloadsForwardMathsLayouts()
    {
        var gl = new RecordingGl();

        gl.Uniform3f(7, (1f, 2f, 3f));
        CollectionAssert.AreEqual(new[] { 1f, 2f, 3f }, gl.LastFloats);

        Vec3[] vectors = [(1f, 2f, 3f), (4f, 5f, 6f)];
        gl.Uniform3fv(9, vectors);
        CollectionAssert.AreEqual(new[] { 1f, 2f, 3f, 4f, 5f, 6f }, gl.LastFloats);

        var matrix = new Mat4(
            1f, 2f, 3f, 4f,
            5f, 6f, 7f, 8f,
            9f, 10f, 11f, 12f,
            13f, 14f, 15f, 16f);
        gl.UniformMatrix4fv(11, in matrix);

        Assert.IsFalse(gl.LastTranspose);
        CollectionAssert.AreEqual(Enumerable.Range(1, 16).Select(static value => (float)value).ToArray(), gl.LastFloats);
    }

    /// <summary>Spatial overloads preserve offsets, extents, and representable calculated endpoints.</summary>
    [TestMethod]
    public void SpatialOverloadsForwardOriginsAndSizes()
    {
        var gl = new RecordingGl();

        gl.TexSubImage3D(
            GlTextureTarget.Texture2DArray,
            0,
            (-2, 3, 4),
            (5u, 6u, 7u),
            GlPixelFormat.Rgba,
            GlPixelType.UnsignedByte,
            0);
        Assert.AreEqual(new Vec3i(-2, 3, 4), gl.LastOffset);
        Assert.AreEqual(new Vec3i(5, 6, 7), gl.LastSize);

        gl.ReadPixels((-3, 8), (20u, 30u), GlPixelFormat.Rgba, GlPixelType.UnsignedByte, 0);
        Assert.AreEqual(new Vec4i(-3, 8, 20, 30), gl.LastRegion);

        gl.RenderbufferStorage(GlRenderbufferTarget.Renderbuffer, GlInternalFormat.Rgba8, (40u, 50u));
        Assert.AreEqual(new Vec4i(0, 0, 40, 50), gl.LastRegion);

        gl.BlitFramebuffer(
            (int.MinValue, 0),
            ((uint)int.MaxValue + 1u, 1u),
            (0, 0),
            (1u, 1u),
            GlClearBufferMask.ColorBufferBit,
            GlBlitFramebufferFilter.Nearest);
        Assert.AreEqual(new Vec4i(int.MinValue, 0, 0, 1), gl.LastSourceRegion);
        Assert.AreEqual(new Vec4i(0, 0, 1, 1), gl.LastDestinationRegion);

        var calls = gl.CallCount;
        Assert.ThrowsExactly<OverflowException>(() => gl.BlitFramebuffer(
            (1, 0),
            ((uint)int.MaxValue, 1u),
            (0, 0),
            (1u, 1u),
            GlClearBufferMask.ColorBufferBit,
            GlBlitFramebufferFilter.Nearest));
        Assert.AreEqual(calls, gl.CallCount);
    }

    /// <summary>Typed vertex declarations select exact component counts and storage enums.</summary>
    [TestMethod]
    public void VertexFormatsMapSupportedMathsTypes()
    {
        var gl = new RecordingGl();

        AssertNormal<Vec2>(gl, 2, GlVertexAttribPointerType.Float, GlVertexAttribType.Float);
        AssertNormal<Vec3>(gl, 3, GlVertexAttribPointerType.Float, GlVertexAttribType.Float);
        AssertNormal<Vec4>(gl, 4, GlVertexAttribPointerType.Float, GlVertexAttribType.Float);
        AssertNormal<Vec2d>(gl, 2, GlVertexAttribPointerType.Double, GlVertexAttribType.Double);
        AssertNormal<Vec3d>(gl, 3, GlVertexAttribPointerType.Double, GlVertexAttribType.Double);
        AssertNormal<Vec4d>(gl, 4, GlVertexAttribPointerType.Double, GlVertexAttribType.Double);
        AssertNormal<Vec2h>(gl, 2, GlVertexAttribPointerType.HalfFloat, GlVertexAttribType.HalfFloat);
        AssertNormal<Vec3h>(gl, 3, GlVertexAttribPointerType.HalfFloat, GlVertexAttribType.HalfFloat);
        AssertNormal<Vec4h>(gl, 4, GlVertexAttribPointerType.HalfFloat, GlVertexAttribType.HalfFloat);
        AssertNormalFamily<Vec2i8, Vec3i8, Vec4i8>(gl, GlVertexAttribPointerType.Byte, GlVertexAttribType.Byte);
        AssertNormalFamily<Vec2u8, Vec3u8, Vec4u8>(gl, GlVertexAttribPointerType.UnsignedByte, GlVertexAttribType.UnsignedByte);
        AssertNormalFamily<Vec2i16, Vec3i16, Vec4i16>(gl, GlVertexAttribPointerType.Short, GlVertexAttribType.Short);
        AssertNormalFamily<Vec2u16, Vec3u16, Vec4u16>(
            gl,
            GlVertexAttribPointerType.UnsignedShort,
            GlVertexAttribType.UnsignedShort);
        AssertNormalFamily<Vec2i, Vec3i, Vec4i>(gl, GlVertexAttribPointerType.Int, GlVertexAttribType.Int);
        AssertNormalFamily<Vec2u, Vec3u, Vec4u>(gl, GlVertexAttribPointerType.UnsignedInt, GlVertexAttribType.UnsignedInt);

        AssertIntegerFamily<Vec2i8, Vec3i8, Vec4i8>(gl, GlVertexAttribIType.Byte);
        AssertIntegerFamily<Vec2u8, Vec3u8, Vec4u8>(gl, GlVertexAttribIType.UnsignedByte);
        AssertIntegerFamily<Vec2i16, Vec3i16, Vec4i16>(gl, GlVertexAttribIType.Short);
        AssertIntegerFamily<Vec2u16, Vec3u16, Vec4u16>(gl, GlVertexAttribIType.UnsignedShort);
        AssertIntegerFamily<Vec2i, Vec3i, Vec4i>(gl, GlVertexAttribIType.Int);
        AssertIntegerFamily<Vec2u, Vec3u, Vec4u>(gl, GlVertexAttribIType.UnsignedInt);

        AssertLong<Vec2d>(gl, 2);
        AssertLong<Vec3d>(gl, 3);
        AssertLong<Vec4d>(gl, 4);

        var calls = gl.CallCount;
        Assert.ThrowsExactly<NotSupportedException>(() => gl.VertexAttribPointer<Vec2i64>(2, false, 16, 0));
        Assert.AreEqual(calls, gl.CallCount);
    }

    /// <summary>Extensions invoked on a layer still dispatch through its virtual state override.</summary>
    [TestMethod]
    public void LayerReceiverPreservesVirtualDispatch()
    {
        var backend = new RecordingGl();
        using var layer = new GlLayer(backend);

        layer.ClearColor((0.25f, 0.5f, 0.75f, 1f));

        Assert.AreEqual(new Vec4(0.25f, 0.5f, 0.75f, 1f), backend.LastColor);
    }

    private static void AssertNormalFamily<T2, T3, T4>(
        RecordingGl gl,
        GlVertexAttribPointerType pointerType,
        GlVertexAttribType formatType)
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        AssertNormal<T2>(gl, 2, pointerType, formatType);
        AssertNormal<T3>(gl, 3, pointerType, formatType);
        AssertNormal<T4>(gl, 4, pointerType, formatType);
    }

    private static void AssertNormal<T>(
        RecordingGl gl,
        int componentCount,
        GlVertexAttribPointerType pointerType,
        GlVertexAttribType formatType) where T : unmanaged
    {
        gl.VertexAttribPointer<T>(0, false, 0, 0);
        Assert.AreEqual(componentCount, gl.LastComponentCount);
        Assert.AreEqual(pointerType, gl.LastPointerType);

        gl.VertexAttribFormat<T>(0, false, 0);
        Assert.AreEqual(componentCount, gl.LastComponentCount);
        Assert.AreEqual(formatType, gl.LastFormatType);
    }

    private static void AssertIntegerFamily<T2, T3, T4>(RecordingGl gl, GlVertexAttribIType type)
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        AssertInteger<T2>(gl, 2, type);
        AssertInteger<T3>(gl, 3, type);
        AssertInteger<T4>(gl, 4, type);
    }

    private static void AssertInteger<T>(RecordingGl gl, int componentCount, GlVertexAttribIType type)
        where T : unmanaged
    {
        gl.VertexAttribIPointer<T>(0, 0, 0);
        Assert.AreEqual(componentCount, gl.LastComponentCount);
        Assert.AreEqual(type, gl.LastIntegerType);

        gl.VertexAttribIFormat<T>(0, 0);
        Assert.AreEqual(componentCount, gl.LastComponentCount);
        Assert.AreEqual(type, gl.LastIntegerType);
    }

    private static void AssertLong<T>(RecordingGl gl, int componentCount) where T : unmanaged
    {
        gl.VertexAttribLPointer<T>(0, 0, 0);
        Assert.AreEqual(componentCount, gl.LastComponentCount);
        Assert.AreEqual(GlVertexAttribLType.Double, gl.LastLongType);

        gl.VertexAttribLFormat<T>(0, 0);
        Assert.AreEqual(componentCount, gl.LastComponentCount);
        Assert.AreEqual(GlVertexAttribLType.Double, gl.LastLongType);
    }
}

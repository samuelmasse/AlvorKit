namespace AlvorKit.OpenGL;

/// <summary>Provides maths-shaped OpenGL state overloads.</summary>
public static class GlStateMathsExtensions
{
    /// <summary>Calls <see cref="Gl.ClearColor(float, float, float, float)"/> for <c>glClearColor</c> with RGBA components.</summary>
    public static void ClearColor(this Gl gl, Vec4 color) =>
        gl.ClearColor(color.X, color.Y, color.Z, color.W);

    /// <summary>Calls <see cref="Gl.BlendColor(float, float, float, float)"/> for <c>glBlendColor</c> with RGBA components.</summary>
    public static void BlendColor(this Gl gl, Vec4 color) =>
        gl.BlendColor(color.X, color.Y, color.Z, color.W);

    /// <summary>Calls <see cref="Gl.ColorMask(bool, bool, bool, bool)"/> for <c>glColorMask</c> with RGBA components.</summary>
    public static void ColorMask(this Gl gl, Vec4b mask) =>
        gl.ColorMask(mask.X, mask.Y, mask.Z, mask.W);

    /// <summary>Calls <see cref="Gl.ColorMaski(uint, bool, bool, bool, bool)"/> for <c>glColorMaski</c> with RGBA components.</summary>
    public static void ColorMaski(this Gl gl, uint index, Vec4b mask) =>
        gl.ColorMaski(index, mask.X, mask.Y, mask.Z, mask.W);

    /// <summary>Calls <see cref="Gl.Viewport(int, int, int, int)"/> for <c>glViewport</c> at the zero origin.</summary>
    public static void Viewport(this Gl gl, Vec2u size)
    {
        var signedSize = GlMathsConversions.ToSize(size);
        gl.Viewport(0, 0, signedSize.X, signedSize.Y);
    }

    /// <summary>Calls <see cref="Gl.Viewport(int, int, int, int)"/> for <c>glViewport</c> with an origin and extent.</summary>
    public static void Viewport(this Gl gl, Vec2i origin, Vec2u size)
    {
        var signedSize = GlMathsConversions.ToSize(size);
        gl.Viewport(origin.X, origin.Y, signedSize.X, signedSize.Y);
    }

    /// <summary>Calls <see cref="Gl.ViewportIndexedf(uint, float, float, float, float)"/> for <c>glViewportIndexedf</c>.</summary>
    public static void ViewportIndexedf(this Gl gl, uint index, Vec4 viewport) =>
        gl.ViewportIndexedf(index, viewport.X, viewport.Y, viewport.Z, viewport.W);

    /// <summary>Calls <see cref="Gl.ViewportIndexedf(uint, float, float, float, float)"/> for <c>glViewportIndexedf</c>.</summary>
    public static void ViewportIndexedf(this Gl gl, uint index, Vec2 origin, Vec2 size) =>
        gl.ViewportIndexedf(index, origin.X, origin.Y, size.X, size.Y);

    /// <summary>Calls <see cref="Gl.ViewportArrayv(uint, int, ReadOnlySpan{float})"/> for <c>glViewportArrayv</c>.</summary>
    public static void ViewportArrayv(this Gl gl, uint first, ReadOnlySpan<Vec4> viewports) =>
        gl.ViewportArrayv(first, viewports.Length, MemoryMarshal.Cast<Vec4, float>(viewports));

    /// <summary>Calls <see cref="Gl.Scissor(int, int, int, int)"/> for <c>glScissor</c> at the zero origin.</summary>
    public static void Scissor(this Gl gl, Vec2u size)
    {
        var signedSize = GlMathsConversions.ToSize(size);
        gl.Scissor(0, 0, signedSize.X, signedSize.Y);
    }

    /// <summary>Calls <see cref="Gl.Scissor(int, int, int, int)"/> for <c>glScissor</c> with an origin and extent.</summary>
    public static void Scissor(this Gl gl, Vec2i origin, Vec2u size)
    {
        var signedSize = GlMathsConversions.ToSize(size);
        gl.Scissor(origin.X, origin.Y, signedSize.X, signedSize.Y);
    }

    /// <summary>Calls <see cref="Gl.ScissorIndexed(uint, int, int, int, int)"/> for <c>glScissorIndexed</c> at the zero origin.</summary>
    public static void ScissorIndexed(this Gl gl, uint index, Vec2u size)
    {
        var signedSize = GlMathsConversions.ToSize(size);
        gl.ScissorIndexed(index, 0, 0, signedSize.X, signedSize.Y);
    }

    /// <summary>Calls <see cref="Gl.ScissorIndexed(uint, int, int, int, int)"/> for <c>glScissorIndexed</c>.</summary>
    public static void ScissorIndexed(this Gl gl, uint index, Vec2i origin, Vec2u size)
    {
        var signedSize = GlMathsConversions.ToSize(size);
        gl.ScissorIndexed(index, origin.X, origin.Y, signedSize.X, signedSize.Y);
    }

    /// <summary>Calls <see cref="Gl.ScissorArrayv(uint, int, ReadOnlySpan{int})"/> for <c>glScissorArrayv</c>.</summary>
    public static void ScissorArrayv(this Gl gl, uint first, ReadOnlySpan<Vec4i> scissors) =>
        gl.ScissorArrayv(first, scissors.Length, MemoryMarshal.Cast<Vec4i, int>(scissors));

    /// <summary>Calls <see cref="Gl.DepthRange(double, double)"/> for <c>glDepthRange</c>.</summary>
    public static void DepthRange(this Gl gl, Intervald range) =>
        gl.DepthRange(range.Min, range.Max);

    /// <summary>Calls <see cref="Gl.DepthRangef(float, float)"/> for <c>glDepthRangef</c>.</summary>
    public static void DepthRangef(this Gl gl, Intervalf range) =>
        gl.DepthRangef(range.Min, range.Max);

    /// <summary>Calls <see cref="Gl.DepthRangeIndexed(uint, double, double)"/> for <c>glDepthRangeIndexed</c>.</summary>
    public static void DepthRangeIndexed(this Gl gl, uint index, Intervald range) =>
        gl.DepthRangeIndexed(index, range.Min, range.Max);

    /// <summary>Calls <see cref="Gl.DepthRangeArrayv(uint, int, ReadOnlySpan{double})"/> for <c>glDepthRangeArrayv</c>.</summary>
    public static void DepthRangeArrayv(this Gl gl, uint first, ReadOnlySpan<Intervald> ranges) =>
        gl.DepthRangeArrayv(first, ranges.Length, MemoryMarshal.Cast<Intervald, double>(ranges));
}

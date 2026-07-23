namespace AlvorKit.OpenGL;

/// <summary>Provides current-program double-precision matrix uniform overloads.</summary>
public static class GlDoubleUniformMatrixMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2dv</c> without transposition.</summary>
    public static void UniformMatrix2dv(this Gl gl, int location, in Mat2d value) =>
        gl.UniformMatrix2dv(location, false, MemoryMarshal.Cast<Mat2d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2dv</c> without transposition.</summary>
    public static void UniformMatrix2dv(this Gl gl, int location, ReadOnlySpan<Mat2d> values) =>
        gl.UniformMatrix2dv(location, false, MemoryMarshal.Cast<Mat2d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2x3dv</c> without transposition.</summary>
    public static void UniformMatrix2x3dv(this Gl gl, int location, in Mat2x3d value) =>
        gl.UniformMatrix2x3dv(location, false, MemoryMarshal.Cast<Mat2x3d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2x3dv</c> without transposition.</summary>
    public static void UniformMatrix2x3dv(this Gl gl, int location, ReadOnlySpan<Mat2x3d> values) =>
        gl.UniformMatrix2x3dv(location, false, MemoryMarshal.Cast<Mat2x3d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2x4dv</c> without transposition.</summary>
    public static void UniformMatrix2x4dv(this Gl gl, int location, in Mat2x4d value) =>
        gl.UniformMatrix2x4dv(location, false, MemoryMarshal.Cast<Mat2x4d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2x4dv</c> without transposition.</summary>
    public static void UniformMatrix2x4dv(this Gl gl, int location, ReadOnlySpan<Mat2x4d> values) =>
        gl.UniformMatrix2x4dv(location, false, MemoryMarshal.Cast<Mat2x4d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3x2dv</c> without transposition.</summary>
    public static void UniformMatrix3x2dv(this Gl gl, int location, in Mat3x2d value) =>
        gl.UniformMatrix3x2dv(location, false, MemoryMarshal.Cast<Mat3x2d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3x2dv</c> without transposition.</summary>
    public static void UniformMatrix3x2dv(this Gl gl, int location, ReadOnlySpan<Mat3x2d> values) =>
        gl.UniformMatrix3x2dv(location, false, MemoryMarshal.Cast<Mat3x2d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3dv</c> without transposition.</summary>
    public static void UniformMatrix3dv(this Gl gl, int location, in Mat3d value) =>
        gl.UniformMatrix3dv(location, false, MemoryMarshal.Cast<Mat3d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3dv</c> without transposition.</summary>
    public static void UniformMatrix3dv(this Gl gl, int location, ReadOnlySpan<Mat3d> values) =>
        gl.UniformMatrix3dv(location, false, MemoryMarshal.Cast<Mat3d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3x4dv</c> without transposition.</summary>
    public static void UniformMatrix3x4dv(this Gl gl, int location, in Mat3x4d value) =>
        gl.UniformMatrix3x4dv(location, false, MemoryMarshal.Cast<Mat3x4d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3x4dv</c> without transposition.</summary>
    public static void UniformMatrix3x4dv(this Gl gl, int location, ReadOnlySpan<Mat3x4d> values) =>
        gl.UniformMatrix3x4dv(location, false, MemoryMarshal.Cast<Mat3x4d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4x2dv</c> without transposition.</summary>
    public static void UniformMatrix4x2dv(this Gl gl, int location, in Mat4x2d value) =>
        gl.UniformMatrix4x2dv(location, false, MemoryMarshal.Cast<Mat4x2d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4x2dv</c> without transposition.</summary>
    public static void UniformMatrix4x2dv(this Gl gl, int location, ReadOnlySpan<Mat4x2d> values) =>
        gl.UniformMatrix4x2dv(location, false, MemoryMarshal.Cast<Mat4x2d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4x3dv</c> without transposition.</summary>
    public static void UniformMatrix4x3dv(this Gl gl, int location, in Mat4x3d value) =>
        gl.UniformMatrix4x3dv(location, false, MemoryMarshal.Cast<Mat4x3d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4x3dv</c> without transposition.</summary>
    public static void UniformMatrix4x3dv(this Gl gl, int location, ReadOnlySpan<Mat4x3d> values) =>
        gl.UniformMatrix4x3dv(location, false, MemoryMarshal.Cast<Mat4x3d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4dv</c> without transposition.</summary>
    public static void UniformMatrix4dv(this Gl gl, int location, in Mat4d value) =>
        gl.UniformMatrix4dv(location, false, MemoryMarshal.Cast<Mat4d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4dv</c> without transposition.</summary>
    public static void UniformMatrix4dv(this Gl gl, int location, ReadOnlySpan<Mat4d> values) =>
        gl.UniformMatrix4dv(location, false, MemoryMarshal.Cast<Mat4d, double>(values));
}

namespace AlvorKit.OpenGL;

/// <summary>Provides current-program single-precision matrix uniform overloads.</summary>
public static class GlFloatUniformMatrixMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2fv</c> without transposition.</summary>
    public static void UniformMatrix2fv(this Gl gl, int location, in Mat2 value) =>
        gl.UniformMatrix2fv(location, false, MemoryMarshal.Cast<Mat2, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2fv</c> without transposition.</summary>
    public static void UniformMatrix2fv(this Gl gl, int location, ReadOnlySpan<Mat2> values) =>
        gl.UniformMatrix2fv(location, false, MemoryMarshal.Cast<Mat2, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2x3fv</c> without transposition.</summary>
    public static void UniformMatrix2x3fv(this Gl gl, int location, in Mat2x3 value) =>
        gl.UniformMatrix2x3fv(location, false, MemoryMarshal.Cast<Mat2x3, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2x3fv</c> without transposition.</summary>
    public static void UniformMatrix2x3fv(this Gl gl, int location, ReadOnlySpan<Mat2x3> values) =>
        gl.UniformMatrix2x3fv(location, false, MemoryMarshal.Cast<Mat2x3, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2x4fv</c> without transposition.</summary>
    public static void UniformMatrix2x4fv(this Gl gl, int location, in Mat2x4 value) =>
        gl.UniformMatrix2x4fv(location, false, MemoryMarshal.Cast<Mat2x4, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix2x4fv</c> without transposition.</summary>
    public static void UniformMatrix2x4fv(this Gl gl, int location, ReadOnlySpan<Mat2x4> values) =>
        gl.UniformMatrix2x4fv(location, false, MemoryMarshal.Cast<Mat2x4, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3x2fv</c> without transposition.</summary>
    public static void UniformMatrix3x2fv(this Gl gl, int location, in Mat3x2 value) =>
        gl.UniformMatrix3x2fv(location, false, MemoryMarshal.Cast<Mat3x2, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3x2fv</c> without transposition.</summary>
    public static void UniformMatrix3x2fv(this Gl gl, int location, ReadOnlySpan<Mat3x2> values) =>
        gl.UniformMatrix3x2fv(location, false, MemoryMarshal.Cast<Mat3x2, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3fv</c> without transposition.</summary>
    public static void UniformMatrix3fv(this Gl gl, int location, in Mat3 value) =>
        gl.UniformMatrix3fv(location, false, MemoryMarshal.Cast<Mat3, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3fv</c> without transposition.</summary>
    public static void UniformMatrix3fv(this Gl gl, int location, ReadOnlySpan<Mat3> values) =>
        gl.UniformMatrix3fv(location, false, MemoryMarshal.Cast<Mat3, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3x4fv</c> without transposition.</summary>
    public static void UniformMatrix3x4fv(this Gl gl, int location, in Mat3x4 value) =>
        gl.UniformMatrix3x4fv(location, false, MemoryMarshal.Cast<Mat3x4, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix3x4fv</c> without transposition.</summary>
    public static void UniformMatrix3x4fv(this Gl gl, int location, ReadOnlySpan<Mat3x4> values) =>
        gl.UniformMatrix3x4fv(location, false, MemoryMarshal.Cast<Mat3x4, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4x2fv</c> without transposition.</summary>
    public static void UniformMatrix4x2fv(this Gl gl, int location, in Mat4x2 value) =>
        gl.UniformMatrix4x2fv(location, false, MemoryMarshal.Cast<Mat4x2, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4x2fv</c> without transposition.</summary>
    public static void UniformMatrix4x2fv(this Gl gl, int location, ReadOnlySpan<Mat4x2> values) =>
        gl.UniformMatrix4x2fv(location, false, MemoryMarshal.Cast<Mat4x2, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4x3fv</c> without transposition.</summary>
    public static void UniformMatrix4x3fv(this Gl gl, int location, in Mat4x3 value) =>
        gl.UniformMatrix4x3fv(location, false, MemoryMarshal.Cast<Mat4x3, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4x3fv</c> without transposition.</summary>
    public static void UniformMatrix4x3fv(this Gl gl, int location, ReadOnlySpan<Mat4x3> values) =>
        gl.UniformMatrix4x3fv(location, false, MemoryMarshal.Cast<Mat4x3, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4fv</c> without transposition.</summary>
    public static void UniformMatrix4fv(this Gl gl, int location, in Mat4 value) =>
        gl.UniformMatrix4fv(location, false, MemoryMarshal.Cast<Mat4, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glUniformMatrix4fv</c> without transposition.</summary>
    public static void UniformMatrix4fv(this Gl gl, int location, ReadOnlySpan<Mat4> values) =>
        gl.UniformMatrix4fv(location, false, MemoryMarshal.Cast<Mat4, float>(values));
}

namespace AlvorKit.OpenGL;

/// <summary>Provides direct-state-access single-precision matrix uniform overloads.</summary>
public static class GlFloatProgramUniformMatrixMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2fv(this Gl gl, GlProgramHandle program, int location, in Mat2 value) =>
        gl.ProgramUniformMatrix2fv(program, location, false, MemoryMarshal.Cast<Mat2, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat2> values) =>
        gl.ProgramUniformMatrix2fv(program, location, false, MemoryMarshal.Cast<Mat2, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2x3fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2x3fv(this Gl gl, GlProgramHandle program, int location, in Mat2x3 value) =>
        gl.ProgramUniformMatrix2x3fv(program, location, false, MemoryMarshal.Cast<Mat2x3, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2x3fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2x3fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat2x3> values) =>
        gl.ProgramUniformMatrix2x3fv(program, location, false, MemoryMarshal.Cast<Mat2x3, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2x4fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2x4fv(this Gl gl, GlProgramHandle program, int location, in Mat2x4 value) =>
        gl.ProgramUniformMatrix2x4fv(program, location, false, MemoryMarshal.Cast<Mat2x4, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2x4fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2x4fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat2x4> values) =>
        gl.ProgramUniformMatrix2x4fv(program, location, false, MemoryMarshal.Cast<Mat2x4, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3x2fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3x2fv(this Gl gl, GlProgramHandle program, int location, in Mat3x2 value) =>
        gl.ProgramUniformMatrix3x2fv(program, location, false, MemoryMarshal.Cast<Mat3x2, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3x2fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3x2fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat3x2> values) =>
        gl.ProgramUniformMatrix3x2fv(program, location, false, MemoryMarshal.Cast<Mat3x2, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3fv(this Gl gl, GlProgramHandle program, int location, in Mat3 value) =>
        gl.ProgramUniformMatrix3fv(program, location, false, MemoryMarshal.Cast<Mat3, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat3> values) =>
        gl.ProgramUniformMatrix3fv(program, location, false, MemoryMarshal.Cast<Mat3, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3x4fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3x4fv(this Gl gl, GlProgramHandle program, int location, in Mat3x4 value) =>
        gl.ProgramUniformMatrix3x4fv(program, location, false, MemoryMarshal.Cast<Mat3x4, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3x4fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3x4fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat3x4> values) =>
        gl.ProgramUniformMatrix3x4fv(program, location, false, MemoryMarshal.Cast<Mat3x4, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4x2fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4x2fv(this Gl gl, GlProgramHandle program, int location, in Mat4x2 value) =>
        gl.ProgramUniformMatrix4x2fv(program, location, false, MemoryMarshal.Cast<Mat4x2, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4x2fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4x2fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat4x2> values) =>
        gl.ProgramUniformMatrix4x2fv(program, location, false, MemoryMarshal.Cast<Mat4x2, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4x3fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4x3fv(this Gl gl, GlProgramHandle program, int location, in Mat4x3 value) =>
        gl.ProgramUniformMatrix4x3fv(program, location, false, MemoryMarshal.Cast<Mat4x3, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4x3fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4x3fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat4x3> values) =>
        gl.ProgramUniformMatrix4x3fv(program, location, false, MemoryMarshal.Cast<Mat4x3, float>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4fv(this Gl gl, GlProgramHandle program, int location, in Mat4 value) =>
        gl.ProgramUniformMatrix4fv(program, location, false, MemoryMarshal.Cast<Mat4, float>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4fv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat4> values) =>
        gl.ProgramUniformMatrix4fv(program, location, false, MemoryMarshal.Cast<Mat4, float>(values));
}

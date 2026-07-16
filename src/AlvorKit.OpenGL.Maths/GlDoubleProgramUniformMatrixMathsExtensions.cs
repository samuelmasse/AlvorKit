namespace AlvorKit.OpenGL;

/// <summary>Provides direct-state-access double-precision matrix uniform overloads.</summary>
public static class GlDoubleProgramUniformMatrixMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2dv(this Gl gl, GlProgramHandle program, int location, in Mat2d value) => gl.ProgramUniformMatrix2dv(program, location, false, MemoryMarshal.Cast<Mat2d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat2d> values) => gl.ProgramUniformMatrix2dv(program, location, false, MemoryMarshal.Cast<Mat2d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2x3dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2x3dv(this Gl gl, GlProgramHandle program, int location, in Mat2x3d value) => gl.ProgramUniformMatrix2x3dv(program, location, false, MemoryMarshal.Cast<Mat2x3d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2x3dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2x3dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat2x3d> values) => gl.ProgramUniformMatrix2x3dv(program, location, false, MemoryMarshal.Cast<Mat2x3d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2x4dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2x4dv(this Gl gl, GlProgramHandle program, int location, in Mat2x4d value) => gl.ProgramUniformMatrix2x4dv(program, location, false, MemoryMarshal.Cast<Mat2x4d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix2x4dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix2x4dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat2x4d> values) => gl.ProgramUniformMatrix2x4dv(program, location, false, MemoryMarshal.Cast<Mat2x4d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3x2dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3x2dv(this Gl gl, GlProgramHandle program, int location, in Mat3x2d value) => gl.ProgramUniformMatrix3x2dv(program, location, false, MemoryMarshal.Cast<Mat3x2d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3x2dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3x2dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat3x2d> values) => gl.ProgramUniformMatrix3x2dv(program, location, false, MemoryMarshal.Cast<Mat3x2d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3dv(this Gl gl, GlProgramHandle program, int location, in Mat3d value) => gl.ProgramUniformMatrix3dv(program, location, false, MemoryMarshal.Cast<Mat3d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat3d> values) => gl.ProgramUniformMatrix3dv(program, location, false, MemoryMarshal.Cast<Mat3d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3x4dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3x4dv(this Gl gl, GlProgramHandle program, int location, in Mat3x4d value) => gl.ProgramUniformMatrix3x4dv(program, location, false, MemoryMarshal.Cast<Mat3x4d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix3x4dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix3x4dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat3x4d> values) => gl.ProgramUniformMatrix3x4dv(program, location, false, MemoryMarshal.Cast<Mat3x4d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4x2dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4x2dv(this Gl gl, GlProgramHandle program, int location, in Mat4x2d value) => gl.ProgramUniformMatrix4x2dv(program, location, false, MemoryMarshal.Cast<Mat4x2d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4x2dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4x2dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat4x2d> values) => gl.ProgramUniformMatrix4x2dv(program, location, false, MemoryMarshal.Cast<Mat4x2d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4x3dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4x3dv(this Gl gl, GlProgramHandle program, int location, in Mat4x3d value) => gl.ProgramUniformMatrix4x3dv(program, location, false, MemoryMarshal.Cast<Mat4x3d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4x3dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4x3dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat4x3d> values) => gl.ProgramUniformMatrix4x3dv(program, location, false, MemoryMarshal.Cast<Mat4x3d, double>(values));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4dv(this Gl gl, GlProgramHandle program, int location, in Mat4d value) => gl.ProgramUniformMatrix4dv(program, location, false, MemoryMarshal.Cast<Mat4d, double>(MemoryMarshal.CreateReadOnlySpan(in value, 1)));
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glProgramUniformMatrix4dv</c> without transposition.</summary>
    public static void ProgramUniformMatrix4dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Mat4d> values) => gl.ProgramUniformMatrix4dv(program, location, false, MemoryMarshal.Cast<Mat4d, double>(values));
}

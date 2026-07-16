namespace AlvorKit.OpenGL;

/// <summary>Provides double-precision vector and quaternion uniform overloads.</summary>
public static class GlDoubleUniformMathsExtensions
{
    /// <summary>Calls <see cref="Gl.Uniform2d(int, double, double)"/> for <c>glUniform2d</c>.</summary>
    public static void Uniform2d(this Gl gl, int location, Vec2d value) => gl.Uniform2d(location, value.X, value.Y);

    /// <summary>Calls <see cref="Gl.Uniform2dv(int, ReadOnlySpan{double})"/> for <c>glUniform2dv</c>.</summary>
    public static void Uniform2dv(this Gl gl, int location, ReadOnlySpan<Vec2d> values) =>
        gl.Uniform2dv(location, MemoryMarshal.Cast<Vec2d, double>(values));

    /// <summary>Calls <see cref="Gl.ProgramUniform2d(GlProgramHandle, int, double, double)"/> for <c>glProgramUniform2d</c>.</summary>
    public static void ProgramUniform2d(this Gl gl, GlProgramHandle program, int location, Vec2d value) =>
        gl.ProgramUniform2d(program, location, value.X, value.Y);

    /// <summary>Calls <see cref="Gl.ProgramUniform2dv(GlProgramHandle, int, ReadOnlySpan{double})"/> for <c>glProgramUniform2dv</c>.</summary>
    public static void ProgramUniform2dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec2d> values) =>
        gl.ProgramUniform2dv(program, location, MemoryMarshal.Cast<Vec2d, double>(values));

    /// <summary>Calls <see cref="Gl.Uniform3d(int, double, double, double)"/> for <c>glUniform3d</c>.</summary>
    public static void Uniform3d(this Gl gl, int location, Vec3d value) => gl.Uniform3d(location, value.X, value.Y, value.Z);

    /// <summary>Calls <see cref="Gl.Uniform3dv(int, ReadOnlySpan{double})"/> for <c>glUniform3dv</c>.</summary>
    public static void Uniform3dv(this Gl gl, int location, ReadOnlySpan<Vec3d> values) =>
        gl.Uniform3dv(location, MemoryMarshal.Cast<Vec3d, double>(values));

    /// <summary>Calls <see cref="Gl.ProgramUniform3d(GlProgramHandle, int, double, double, double)"/> for <c>glProgramUniform3d</c>.</summary>
    public static void ProgramUniform3d(this Gl gl, GlProgramHandle program, int location, Vec3d value) =>
        gl.ProgramUniform3d(program, location, value.X, value.Y, value.Z);

    /// <summary>Calls <see cref="Gl.ProgramUniform3dv(GlProgramHandle, int, ReadOnlySpan{double})"/> for <c>glProgramUniform3dv</c>.</summary>
    public static void ProgramUniform3dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec3d> values) =>
        gl.ProgramUniform3dv(program, location, MemoryMarshal.Cast<Vec3d, double>(values));

    /// <summary>Calls <see cref="Gl.Uniform4d(int, double, double, double, double)"/> for <c>glUniform4d</c>.</summary>
    public static void Uniform4d(this Gl gl, int location, Vec4d value) => gl.Uniform4d(location, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.Uniform4dv(int, ReadOnlySpan{double})"/> for <c>glUniform4dv</c>.</summary>
    public static void Uniform4dv(this Gl gl, int location, ReadOnlySpan<Vec4d> values) =>
        gl.Uniform4dv(location, MemoryMarshal.Cast<Vec4d, double>(values));

    /// <summary>Calls <see cref="Gl.ProgramUniform4d(GlProgramHandle, int, double, double, double, double)"/> for <c>glProgramUniform4d</c>.</summary>
    public static void ProgramUniform4d(this Gl gl, GlProgramHandle program, int location, Vec4d value) =>
        gl.ProgramUniform4d(program, location, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.ProgramUniform4dv(GlProgramHandle, int, ReadOnlySpan{double})"/> for <c>glProgramUniform4dv</c>.</summary>
    public static void ProgramUniform4dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec4d> values) =>
        gl.ProgramUniform4dv(program, location, MemoryMarshal.Cast<Vec4d, double>(values));

    /// <summary>Calls <see cref="Gl.Uniform4d(int, double, double, double, double)"/> for <c>glUniform4d</c> in quaternion XYZW order.</summary>
    public static void Uniform4d(this Gl gl, int location, Quatd value) => gl.Uniform4d(location, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.Uniform4dv(int, ReadOnlySpan{double})"/> for <c>glUniform4dv</c> in quaternion XYZW order.</summary>
    public static void Uniform4dv(this Gl gl, int location, ReadOnlySpan<Quatd> values) =>
        gl.Uniform4dv(location, MemoryMarshal.Cast<Quatd, double>(values));

    /// <summary>Calls <see cref="Gl.ProgramUniform4d(GlProgramHandle, int, double, double, double, double)"/> for <c>glProgramUniform4d</c> in XYZW order.</summary>
    public static void ProgramUniform4d(this Gl gl, GlProgramHandle program, int location, Quatd value) =>
        gl.ProgramUniform4d(program, location, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.ProgramUniform4dv(GlProgramHandle, int, ReadOnlySpan{double})"/> for <c>glProgramUniform4dv</c> in XYZW order.</summary>
    public static void ProgramUniform4dv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Quatd> values) =>
        gl.ProgramUniform4dv(program, location, MemoryMarshal.Cast<Quatd, double>(values));
}

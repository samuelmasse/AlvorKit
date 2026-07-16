namespace AlvorKit.OpenGL;

/// <summary>Provides single-precision vector and quaternion uniform overloads.</summary>
public static class GlFloatUniformMathsExtensions
{
    /// <summary>Calls <see cref="Gl.Uniform2f(int, float, float)"/> for <c>glUniform2f</c>.</summary>
    public static void Uniform2f(this Gl gl, int location, Vec2 value) => gl.Uniform2f(location, value.X, value.Y);

    /// <summary>Calls <see cref="Gl.Uniform2fv(int, ReadOnlySpan{float})"/> for <c>glUniform2fv</c>.</summary>
    public static void Uniform2fv(this Gl gl, int location, ReadOnlySpan<Vec2> values) =>
        gl.Uniform2fv(location, MemoryMarshal.Cast<Vec2, float>(values));

    /// <summary>Calls <see cref="Gl.ProgramUniform2f(GlProgramHandle, int, float, float)"/> for <c>glProgramUniform2f</c>.</summary>
    public static void ProgramUniform2f(this Gl gl, GlProgramHandle program, int location, Vec2 value) =>
        gl.ProgramUniform2f(program, location, value.X, value.Y);

    /// <summary>Calls <see cref="Gl.ProgramUniform2fv(GlProgramHandle, int, ReadOnlySpan{float})"/> for <c>glProgramUniform2fv</c>.</summary>
    public static void ProgramUniform2fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec2> values) =>
        gl.ProgramUniform2fv(program, location, MemoryMarshal.Cast<Vec2, float>(values));

    /// <summary>Calls <see cref="Gl.Uniform3f(int, float, float, float)"/> for <c>glUniform3f</c>.</summary>
    public static void Uniform3f(this Gl gl, int location, Vec3 value) => gl.Uniform3f(location, value.X, value.Y, value.Z);

    /// <summary>Calls <see cref="Gl.Uniform3fv(int, ReadOnlySpan{float})"/> for <c>glUniform3fv</c>.</summary>
    public static void Uniform3fv(this Gl gl, int location, ReadOnlySpan<Vec3> values) =>
        gl.Uniform3fv(location, MemoryMarshal.Cast<Vec3, float>(values));

    /// <summary>Calls <see cref="Gl.ProgramUniform3f(GlProgramHandle, int, float, float, float)"/> for <c>glProgramUniform3f</c>.</summary>
    public static void ProgramUniform3f(this Gl gl, GlProgramHandle program, int location, Vec3 value) =>
        gl.ProgramUniform3f(program, location, value.X, value.Y, value.Z);

    /// <summary>Calls <see cref="Gl.ProgramUniform3fv(GlProgramHandle, int, ReadOnlySpan{float})"/> for <c>glProgramUniform3fv</c>.</summary>
    public static void ProgramUniform3fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec3> values) =>
        gl.ProgramUniform3fv(program, location, MemoryMarshal.Cast<Vec3, float>(values));

    /// <summary>Calls <see cref="Gl.Uniform4f(int, float, float, float, float)"/> for <c>glUniform4f</c>.</summary>
    public static void Uniform4f(this Gl gl, int location, Vec4 value) => gl.Uniform4f(location, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.Uniform4fv(int, ReadOnlySpan{float})"/> for <c>glUniform4fv</c>.</summary>
    public static void Uniform4fv(this Gl gl, int location, ReadOnlySpan<Vec4> values) =>
        gl.Uniform4fv(location, MemoryMarshal.Cast<Vec4, float>(values));

    /// <summary>Calls <see cref="Gl.ProgramUniform4f(GlProgramHandle, int, float, float, float, float)"/> for <c>glProgramUniform4f</c>.</summary>
    public static void ProgramUniform4f(this Gl gl, GlProgramHandle program, int location, Vec4 value) =>
        gl.ProgramUniform4f(program, location, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.ProgramUniform4fv(GlProgramHandle, int, ReadOnlySpan{float})"/> for <c>glProgramUniform4fv</c>.</summary>
    public static void ProgramUniform4fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec4> values) =>
        gl.ProgramUniform4fv(program, location, MemoryMarshal.Cast<Vec4, float>(values));

    /// <summary>Calls <see cref="Gl.Uniform4f(int, float, float, float, float)"/> for <c>glUniform4f</c> in quaternion XYZW order.</summary>
    public static void Uniform4f(this Gl gl, int location, Quat value) => gl.Uniform4f(location, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.Uniform4fv(int, ReadOnlySpan{float})"/> for <c>glUniform4fv</c> in quaternion XYZW order.</summary>
    public static void Uniform4fv(this Gl gl, int location, ReadOnlySpan<Quat> values) =>
        gl.Uniform4fv(location, MemoryMarshal.Cast<Quat, float>(values));

    /// <summary>Calls <see cref="Gl.ProgramUniform4f(GlProgramHandle, int, float, float, float, float)"/> for <c>glProgramUniform4f</c> in XYZW order.</summary>
    public static void ProgramUniform4f(this Gl gl, GlProgramHandle program, int location, Quat value) =>
        gl.ProgramUniform4f(program, location, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.ProgramUniform4fv(GlProgramHandle, int, ReadOnlySpan{float})"/> for <c>glProgramUniform4fv</c> in XYZW order.</summary>
    public static void ProgramUniform4fv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Quat> values) =>
        gl.ProgramUniform4fv(program, location, MemoryMarshal.Cast<Quat, float>(values));
}

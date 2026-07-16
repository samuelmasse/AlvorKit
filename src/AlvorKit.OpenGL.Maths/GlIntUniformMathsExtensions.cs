namespace AlvorKit.OpenGL;

/// <summary>Provides signed integer vector uniform overloads.</summary>
public static class GlIntUniformMathsExtensions
{
    /// <summary>Calls <see cref="Gl.Uniform2i(int, int, int)"/> for <c>glUniform2i</c>.</summary>
    public static void Uniform2i(this Gl gl, int location, Vec2i value) => gl.Uniform2i(location, value.X, value.Y);
    /// <summary>Calls <see cref="Gl.Uniform2iv(int, ReadOnlySpan{int})"/> for <c>glUniform2iv</c>.</summary>
    public static void Uniform2iv(this Gl gl, int location, ReadOnlySpan<Vec2i> values) => gl.Uniform2iv(location, MemoryMarshal.Cast<Vec2i, int>(values));
    /// <summary>Calls <see cref="Gl.ProgramUniform2i(GlProgramHandle, int, int, int)"/> for <c>glProgramUniform2i</c>.</summary>
    public static void ProgramUniform2i(this Gl gl, GlProgramHandle program, int location, Vec2i value) => gl.ProgramUniform2i(program, location, value.X, value.Y);
    /// <summary>Calls <see cref="Gl.ProgramUniform2iv(GlProgramHandle, int, ReadOnlySpan{int})"/> for <c>glProgramUniform2iv</c>.</summary>
    public static void ProgramUniform2iv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec2i> values) => gl.ProgramUniform2iv(program, location, MemoryMarshal.Cast<Vec2i, int>(values));

    /// <summary>Calls <see cref="Gl.Uniform3i(int, int, int, int)"/> for <c>glUniform3i</c>.</summary>
    public static void Uniform3i(this Gl gl, int location, Vec3i value) => gl.Uniform3i(location, value.X, value.Y, value.Z);
    /// <summary>Calls <see cref="Gl.Uniform3iv(int, ReadOnlySpan{int})"/> for <c>glUniform3iv</c>.</summary>
    public static void Uniform3iv(this Gl gl, int location, ReadOnlySpan<Vec3i> values) => gl.Uniform3iv(location, MemoryMarshal.Cast<Vec3i, int>(values));
    /// <summary>Calls <see cref="Gl.ProgramUniform3i(GlProgramHandle, int, int, int, int)"/> for <c>glProgramUniform3i</c>.</summary>
    public static void ProgramUniform3i(this Gl gl, GlProgramHandle program, int location, Vec3i value) => gl.ProgramUniform3i(program, location, value.X, value.Y, value.Z);
    /// <summary>Calls <see cref="Gl.ProgramUniform3iv(GlProgramHandle, int, ReadOnlySpan{int})"/> for <c>glProgramUniform3iv</c>.</summary>
    public static void ProgramUniform3iv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec3i> values) => gl.ProgramUniform3iv(program, location, MemoryMarshal.Cast<Vec3i, int>(values));

    /// <summary>Calls <see cref="Gl.Uniform4i(int, int, int, int, int)"/> for <c>glUniform4i</c>.</summary>
    public static void Uniform4i(this Gl gl, int location, Vec4i value) => gl.Uniform4i(location, value.X, value.Y, value.Z, value.W);
    /// <summary>Calls <see cref="Gl.Uniform4iv(int, ReadOnlySpan{int})"/> for <c>glUniform4iv</c>.</summary>
    public static void Uniform4iv(this Gl gl, int location, ReadOnlySpan<Vec4i> values) => gl.Uniform4iv(location, MemoryMarshal.Cast<Vec4i, int>(values));
    /// <summary>Calls <see cref="Gl.ProgramUniform4i(GlProgramHandle, int, int, int, int, int)"/> for <c>glProgramUniform4i</c>.</summary>
    public static void ProgramUniform4i(this Gl gl, GlProgramHandle program, int location, Vec4i value) => gl.ProgramUniform4i(program, location, value.X, value.Y, value.Z, value.W);
    /// <summary>Calls <see cref="Gl.ProgramUniform4iv(GlProgramHandle, int, ReadOnlySpan{int})"/> for <c>glProgramUniform4iv</c>.</summary>
    public static void ProgramUniform4iv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec4i> values) => gl.ProgramUniform4iv(program, location, MemoryMarshal.Cast<Vec4i, int>(values));
}

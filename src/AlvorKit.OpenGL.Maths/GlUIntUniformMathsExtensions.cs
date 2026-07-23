namespace AlvorKit.OpenGL;

/// <summary>Provides unsigned integer vector uniform overloads.</summary>
public static class GlUIntUniformMathsExtensions
{
    /// <summary>Calls <see cref="Gl.Uniform2ui(int, uint, uint)"/> for <c>glUniform2ui</c>.</summary>
    public static void Uniform2ui(this Gl gl, int location, Vec2u value) => gl.Uniform2ui(location, value.X, value.Y);
    /// <summary>Calls <see cref="Gl.Uniform2uiv(int, ReadOnlySpan{uint})"/> for <c>glUniform2uiv</c>.</summary>
    public static void Uniform2uiv(this Gl gl, int location, ReadOnlySpan<Vec2u> values) => gl.Uniform2uiv(location, MemoryMarshal.Cast<Vec2u, uint>(values));
    /// <summary>Calls <see cref="Gl.ProgramUniform2ui(GlProgramHandle, int, uint, uint)"/> for <c>glProgramUniform2ui</c>.</summary>
    public static void ProgramUniform2ui(this Gl gl, GlProgramHandle program, int location, Vec2u value) => gl.ProgramUniform2ui(program, location, value.X, value.Y);
    /// <summary>Calls <see cref="Gl.ProgramUniform2uiv(GlProgramHandle, int, ReadOnlySpan{uint})"/> for <c>glProgramUniform2uiv</c>.</summary>
    public static void ProgramUniform2uiv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec2u> values) =>
        gl.ProgramUniform2uiv(program, location, MemoryMarshal.Cast<Vec2u, uint>(values));

    /// <summary>Calls <see cref="Gl.Uniform3ui(int, uint, uint, uint)"/> for <c>glUniform3ui</c>.</summary>
    public static void Uniform3ui(this Gl gl, int location, Vec3u value) => gl.Uniform3ui(location, value.X, value.Y, value.Z);
    /// <summary>Calls <see cref="Gl.Uniform3uiv(int, ReadOnlySpan{uint})"/> for <c>glUniform3uiv</c>.</summary>
    public static void Uniform3uiv(this Gl gl, int location, ReadOnlySpan<Vec3u> values) => gl.Uniform3uiv(location, MemoryMarshal.Cast<Vec3u, uint>(values));
    /// <summary>Calls <see cref="Gl.ProgramUniform3ui(GlProgramHandle, int, uint, uint, uint)"/> for <c>glProgramUniform3ui</c>.</summary>
    public static void ProgramUniform3ui(this Gl gl, GlProgramHandle program, int location, Vec3u value) =>
        gl.ProgramUniform3ui(program, location, value.X, value.Y, value.Z);
    /// <summary>Calls <see cref="Gl.ProgramUniform3uiv(GlProgramHandle, int, ReadOnlySpan{uint})"/> for <c>glProgramUniform3uiv</c>.</summary>
    public static void ProgramUniform3uiv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec3u> values) =>
        gl.ProgramUniform3uiv(program, location, MemoryMarshal.Cast<Vec3u, uint>(values));

    /// <summary>Calls <see cref="Gl.Uniform4ui(int, uint, uint, uint, uint)"/> for <c>glUniform4ui</c>.</summary>
    public static void Uniform4ui(this Gl gl, int location, Vec4u value) => gl.Uniform4ui(location, value.X, value.Y, value.Z, value.W);
    /// <summary>Calls <see cref="Gl.Uniform4uiv(int, ReadOnlySpan{uint})"/> for <c>glUniform4uiv</c>.</summary>
    public static void Uniform4uiv(this Gl gl, int location, ReadOnlySpan<Vec4u> values) => gl.Uniform4uiv(location, MemoryMarshal.Cast<Vec4u, uint>(values));
    /// <summary>Calls <see cref="Gl.ProgramUniform4ui(GlProgramHandle, int, uint, uint, uint, uint)"/> for <c>glProgramUniform4ui</c>.</summary>
    public static void ProgramUniform4ui(this Gl gl, GlProgramHandle program, int location, Vec4u value) =>
        gl.ProgramUniform4ui(program, location, value.X, value.Y, value.Z, value.W);
    /// <summary>Calls <see cref="Gl.ProgramUniform4uiv(GlProgramHandle, int, ReadOnlySpan{uint})"/> for <c>glProgramUniform4uiv</c>.</summary>
    public static void ProgramUniform4uiv(this Gl gl, GlProgramHandle program, int location, ReadOnlySpan<Vec4u> values) =>
        gl.ProgramUniform4uiv(program, location, MemoryMarshal.Cast<Vec4u, uint>(values));
}

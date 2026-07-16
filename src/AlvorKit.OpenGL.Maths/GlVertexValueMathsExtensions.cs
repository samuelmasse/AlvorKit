namespace AlvorKit.OpenGL;

/// <summary>Provides maths-vector constant vertex attribute values.</summary>
public static class GlVertexValueMathsExtensions
{
    /// <summary>Calls <see cref="Gl.VertexAttrib2f(uint, float, float)"/> for <c>glVertexAttrib2f</c>.</summary>
    public static void VertexAttrib2f(this Gl gl, uint index, Vec2 value) => gl.VertexAttrib2f(index, value.X, value.Y);
    /// <summary>Calls <see cref="Gl.VertexAttrib3f(uint, float, float, float)"/> for <c>glVertexAttrib3f</c>.</summary>
    public static void VertexAttrib3f(this Gl gl, uint index, Vec3 value) => gl.VertexAttrib3f(index, value.X, value.Y, value.Z);
    /// <summary>Calls <see cref="Gl.VertexAttrib4f(uint, float, float, float, float)"/> for <c>glVertexAttrib4f</c>.</summary>
    public static void VertexAttrib4f(this Gl gl, uint index, Vec4 value) => gl.VertexAttrib4f(index, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.VertexAttrib2d(uint, double, double)"/> for <c>glVertexAttrib2d</c>.</summary>
    public static void VertexAttrib2d(this Gl gl, uint index, Vec2d value) => gl.VertexAttrib2d(index, value.X, value.Y);
    /// <summary>Calls <see cref="Gl.VertexAttrib3d(uint, double, double, double)"/> for <c>glVertexAttrib3d</c>.</summary>
    public static void VertexAttrib3d(this Gl gl, uint index, Vec3d value) => gl.VertexAttrib3d(index, value.X, value.Y, value.Z);
    /// <summary>Calls <see cref="Gl.VertexAttrib4d(uint, double, double, double, double)"/> for <c>glVertexAttrib4d</c>.</summary>
    public static void VertexAttrib4d(this Gl gl, uint index, Vec4d value) => gl.VertexAttrib4d(index, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.VertexAttribI2i(uint, int, int)"/> for <c>glVertexAttribI2i</c>.</summary>
    public static void VertexAttribI2i(this Gl gl, uint index, Vec2i value) => gl.VertexAttribI2i(index, value.X, value.Y);
    /// <summary>Calls <see cref="Gl.VertexAttribI3i(uint, int, int, int)"/> for <c>glVertexAttribI3i</c>.</summary>
    public static void VertexAttribI3i(this Gl gl, uint index, Vec3i value) => gl.VertexAttribI3i(index, value.X, value.Y, value.Z);
    /// <summary>Calls <see cref="Gl.VertexAttribI4i(uint, int, int, int, int)"/> for <c>glVertexAttribI4i</c>.</summary>
    public static void VertexAttribI4i(this Gl gl, uint index, Vec4i value) => gl.VertexAttribI4i(index, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.VertexAttribI2ui(uint, uint, uint)"/> for <c>glVertexAttribI2ui</c>.</summary>
    public static void VertexAttribI2ui(this Gl gl, uint index, Vec2u value) => gl.VertexAttribI2ui(index, value.X, value.Y);
    /// <summary>Calls <see cref="Gl.VertexAttribI3ui(uint, uint, uint, uint)"/> for <c>glVertexAttribI3ui</c>.</summary>
    public static void VertexAttribI3ui(this Gl gl, uint index, Vec3u value) => gl.VertexAttribI3ui(index, value.X, value.Y, value.Z);
    /// <summary>Calls <see cref="Gl.VertexAttribI4ui(uint, uint, uint, uint, uint)"/> for <c>glVertexAttribI4ui</c>.</summary>
    public static void VertexAttribI4ui(this Gl gl, uint index, Vec4u value) => gl.VertexAttribI4ui(index, value.X, value.Y, value.Z, value.W);

    /// <summary>Calls <see cref="Gl.VertexAttribL2d(uint, double, double)"/> for <c>glVertexAttribL2d</c>.</summary>
    public static void VertexAttribL2d(this Gl gl, uint index, Vec2d value) => gl.VertexAttribL2d(index, value.X, value.Y);
    /// <summary>Calls <see cref="Gl.VertexAttribL3d(uint, double, double, double)"/> for <c>glVertexAttribL3d</c>.</summary>
    public static void VertexAttribL3d(this Gl gl, uint index, Vec3d value) => gl.VertexAttribL3d(index, value.X, value.Y, value.Z);
    /// <summary>Calls <see cref="Gl.VertexAttribL4d(uint, double, double, double, double)"/> for <c>glVertexAttribL4d</c>.</summary>
    public static void VertexAttribL4d(this Gl gl, uint index, Vec4d value) => gl.VertexAttribL4d(index, value.X, value.Y, value.Z, value.W);
}

namespace AlvorKit.Engine;

/// <summary>Position, color, and texture-coordinate vertex used by built-in textured shaders.</summary>
[StructLayout(LayoutKind.Sequential)]
[ExcludeFromCodeCoverage(Justification = "Vertex attribute setup writes to a live OpenGL vertex-array state.")]
public readonly record struct PositionColorTextureVertex(Vec3 Position, Vec3 Color, Vec2 TexCoord) : IVertex
{
    /// <summary>Enables position, color, and texture-coordinate attributes for the currently bound vertex array.</summary>
    public static void SetAttributes(GlLayer gl)
    {
        gl.VertexAttribPointer(0, 3, GlVertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 3, GlVertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(2, 2, GlVertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        gl.EnableVertexAttribArray(2);
    }
}

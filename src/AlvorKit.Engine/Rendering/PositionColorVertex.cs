namespace AlvorKit.Engine;

/// <summary>Position and color vertex used by built-in simple color shaders.</summary>
[StructLayout(LayoutKind.Sequential)]
[ExcludeFromCodeCoverage(Justification = "Vertex attribute setup writes to a live OpenGL vertex-array state.")]
public readonly record struct PositionColorVertex(Vec3 Position, Vec3 Color) : IVertex
{
    /// <summary>Enables position and color attributes for the currently bound vertex array.</summary>
    public static void SetAttributes(GlLayer gl)
    {
        gl.VertexAttribPointer<Vec3>(0, false, 6 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer<Vec3>(1, false, 6 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);
    }
}

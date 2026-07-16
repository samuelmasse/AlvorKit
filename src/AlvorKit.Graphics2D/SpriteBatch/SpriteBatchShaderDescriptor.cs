namespace AlvorKit.Graphics2D;

/// <summary>Configures the vertex attributes consumed by the built-in sprite batch shader.</summary>
internal class SpriteBatchShaderDescriptor(GlLayer gl)
{
    /// <summary>Records the position, color, texture-coordinate, and texture-index attributes.</summary>
    internal void SetAttribPointers()
    {
        gl.VertexAttribPointer<Vec2>(0, false, SpriteBatchVertex.Size, 0);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer<Vec4>(1, false, SpriteBatchVertex.Size, sizeof(float) * 2);
        gl.EnableVertexAttribArray(1);

        gl.VertexAttribPointer<Vec2>(2, false, SpriteBatchVertex.Size, sizeof(float) * 6);
        gl.EnableVertexAttribArray(2);

        gl.VertexAttribPointer(3, 1, GlVertexAttribPointerType.Float, false, SpriteBatchVertex.Size, sizeof(float) * 8);
        gl.EnableVertexAttribArray(3);
    }
}

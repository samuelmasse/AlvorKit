namespace AlvorKit.Graphics2D;

/// <summary>Configures the vertex attributes consumed by the built-in sprite batch shader.</summary>
internal class SpriteBatchShaderDescriptor(GlLayer gl)
{
    /// <summary>Records the position, color, texture-coordinate, and texture-index attributes.</summary>
    internal void SetAttribPointers()
    {
        gl.VertexAttribPointer(0, 2, GlVertexAttribPointerType.Float, false, SpriteBatchVertex.Size, 0);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer(1, 4, GlVertexAttribPointerType.Float, false, SpriteBatchVertex.Size, sizeof(float) * 2);
        gl.EnableVertexAttribArray(1);

        gl.VertexAttribPointer(2, 2, GlVertexAttribPointerType.Float, false, SpriteBatchVertex.Size, sizeof(float) * 6);
        gl.EnableVertexAttribArray(2);

        gl.VertexAttribPointer(3, 1, GlVertexAttribPointerType.Float, false, SpriteBatchVertex.Size, sizeof(float) * 8);
        gl.EnableVertexAttribArray(3);
    }
}

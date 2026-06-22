namespace AlvorKit.Engine;

/// <summary>Root-owned reusable index buffer that expands quad vertices into triangles.</summary>
[Root]
[ExcludeFromCodeCoverage(Justification = "Uploads index data to a live OpenGL buffer.")]
public sealed class RootQuadIndexBuffer(RootGl gl)
{
    private readonly GlBufferHandle id = gl.GenBuffer();
    private int capacity;

    /// <summary>Gets the tracked element-array buffer handle.</summary>
    public GlBufferHandle Id => id;

    /// <summary>Gets the quad vertex capacity represented by the current index data.</summary>
    public int Capacity => capacity;

    /// <summary>Ensures enough indices exist for <paramref name="quadVertexCount"/> quad vertices.</summary>
    public void EnsureCapacity(int quadVertexCount)
    {
        if (quadVertexCount <= capacity)
            return;

        var newCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)quadVertexCount + 1u);
        var indexValues = new uint[IndexCount(newCapacity)];
        uint vertexIndex = 0;
        for (var i = 0; i < indexValues.Length; i += 6)
        {
            indexValues[i] = vertexIndex + 2u;
            indexValues[i + 1] = vertexIndex + 3u;
            indexValues[i + 2] = vertexIndex + 1u;
            indexValues[i + 3] = vertexIndex + 1u;
            indexValues[i + 4] = vertexIndex;
            indexValues[i + 5] = vertexIndex + 2u;
            vertexIndex += 4u;
        }

        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, id);
        gl.BufferData(GlBufferTarget.ElementArrayBuffer, indexValues, GlBufferUsage.StaticDraw);
        gl.UnbindBuffer(GlBufferTarget.ElementArrayBuffer);
        capacity = newCapacity;
    }

    /// <summary>Returns the triangle index count for a quad vertex count.</summary>
    public int IndexCount(int quadVertexCount) => quadVertexCount / 4 * 6;
}

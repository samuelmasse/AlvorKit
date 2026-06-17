namespace AlvorKit.Graphics2D;

/// <summary>Flushes collected sprite vertices to OpenGL draw calls.</summary>
internal class SpriteBatchRenderer(
    GlLayer gl,
    SpriteBatchVertices vertices,
    ShaderProgram program,
    QuadVertexArray vertexArray)
{
    /// <summary>Assigns each texture sampler uniform to the matching texture unit index.</summary>
    /// <param name="textureSlots">The number of texture units exposed to the fragment shader.</param>
    internal void SetupSamplerUniforms(int textureSlots)
    {
        gl.UseProgram(program.Id);

        for (var i = 0; i < textureSlots; i++)
        {
            var location = gl.GetUniformLocation(program.Id, $"texSamplers[{i}]");
            gl.Uniform1i(location, i);
        }

        gl.UnuseProgram();
    }

    /// <summary>Uploads pending vertices, binds section textures, and draws each section.</summary>
    internal void Render()
    {
        if (vertices.VertexCount == 0)
            return;

        vertexArray.VertexBuffer.Transfer(vertices.Vertices);

        gl.UseProgram(program.Id);
        gl.BindVertexArray(vertexArray.Id);
        vertexArray.IndexBuffer.EnsureCapacity(vertices.VertexCount);

        var offset = 0;
        for (var i = 0; i < vertices.Sections.Length; i++)
        {
            var section = vertices.Sections[i];
            var textures = vertices.SectionTextures(i);

            for (var j = 0; j < textures.Length; j++)
                textures[j].Bind(TextureUnit(j));

            gl.DrawElements(GlPrimitiveType.Triangles, section.Count / 4 * 6, GlDrawElementsType.UnsignedInt, (nint)(offset * 6));

            for (var j = textures.Length - 1; j >= 0; j--)
                textures[j].Unbind(TextureUnit(j));

            offset += section.Count;
        }

        gl.UnbindVertexArray();
        gl.UnuseProgram();
    }

    /// <summary>Converts a zero-based texture slot index into an OpenGL texture unit enum value.</summary>
    private static GlTextureUnit TextureUnit(int index) => (GlTextureUnit)((uint)GlTextureUnit.Texture0 + (uint)index);
}

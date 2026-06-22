RootLoop.RunGlfw<TriangleState>();

/// <summary>Draws the same three-vertex color triangle as the old AlvorEngine triangle demo.</summary>
[Root]
internal sealed class TriangleState(
    RootGl gl,
    RootScreen screen,
    RootBackbuffer backbuffer,
    RootCanvas canvas,
    RootPositionColorProgram positionColorProgram) : State
{
    /// <summary>The vertex array object that captures the triangle attribute layout.</summary>
    private GlVertexArrayHandle vao;

    /// <summary>Uploads the triangle vertices and shows the window once the state becomes current.</summary>
    public override void Load()
    {
        PositionColorVertex[] vertices =
        [
            new((0.5f, -0.5f, 0.0f), (1.0f, 0.0f, 0.0f)),
            new((-0.5f, -0.5f, 0.0f), (0.0f, 1.0f, 0.0f)),
            new((0.0f, 0.5f, 0.0f), (0.0f, 0.0f, 1.0f)),
        ];

        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, gl.GenBuffer());
        gl.BufferData(GlBufferTarget.ArrayBuffer, vertices.AsSpan(), GlBufferUsage.StaticDraw);
        positionColorProgram.SetAttributes();
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();

        screen.IsVisible = true;
    }

    /// <summary>Clears the backbuffer and draws the uploaded triangle every render frame.</summary>
    public override void Render()
    {
        backbuffer.Clear();
        gl.Viewport(0, 0, (int)canvas.Size.X, (int)canvas.Size.Y);
        gl.UseProgram(positionColorProgram.Id);
        gl.BindVertexArray(vao);
        gl.DrawArrays(GlPrimitiveType.Triangles, 0, 3);
        gl.UnbindVertexArray();
        gl.UnuseProgram();
        gl.ResetViewport();
    }
}

namespace AlvorStarter.App;

/// <summary>Draws a raw OpenGL color triangle through the root GL layer.</summary>
[App]
public class AppGlTriangle(
    RootGl gl,
    RootCanvas canvas,
    RootPositionColorProgram positionColorProgram)
{
    private GlVertexArrayHandle vertexArray;

    /// <summary>Uploads the triangle vertices once during app startup.</summary>
    public void Load()
    {
        PositionColorVertex[] vertices =
        [
            new((-0.06f, -0.50f, 0.0f), (0.93f, 0.30f, 0.22f)),
            new((-0.76f, -0.50f, 0.0f), (0.20f, 0.76f, 0.50f)),
            new((-0.40f, 0.38f, 0.0f), (0.30f, 0.55f, 0.96f)),
        ];

        vertexArray = gl.GenVertexArray();
        gl.BindVertexArray(vertexArray);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, gl.GenBuffer());
        gl.BufferData(GlBufferTarget.ArrayBuffer, vertices.AsSpan(), GlBufferUsage.StaticDraw);
        positionColorProgram.SetAttributes();
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();
    }

    /// <summary>Draws the uploaded triangle into the current backbuffer.</summary>
    public void Render()
    {
        gl.Viewport(0, 0, (int)canvas.Size.X, (int)canvas.Size.Y);
        gl.UseProgram(positionColorProgram.Id);
        gl.BindVertexArray(vertexArray);
        gl.DrawArrays(GlPrimitiveType.Triangles, 0, 3);
        gl.UnbindVertexArray();
        gl.UnuseProgram();
        gl.ResetViewport();
    }
}

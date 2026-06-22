RootLoop.RunGlfw<Triangle3DState>();

/// <summary>Draws a 3D color triangle with simple free-camera movement and debug text.</summary>
[Root]
internal sealed class Triangle3DState(
    RootInput input,
    RootMouse mouse,
    RootKeyboard keyboard,
    RootGl gl,
    RootScreen screen,
    RootBackbuffer backbuffer,
    RootCanvas canvas,
    RootPositionColorProgram3D positionColorProgram3D,
    RootSprites sprites,
    RootRoboto roboto,
    RootText text,
    RootScale scale) : State
{
    private readonly Camera3D camera = new();
    private readonly Perspective3D perspective = new();

    /// <summary>The VAO that captures the triangle attribute layout.</summary>
    private GlVertexArrayHandle vao;

    private bool paused;

    /// <summary>Uploads the triangle vertices, initializes the camera, and shows the window.</summary>
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
        positionColorProgram3D.SetAttributes();
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();

        camera.Offset = (0, 0, 10);
        screen.IsVisible = true;
    }

    /// <summary>Moves and rotates the camera while unpaused, and lets Escape pause mouselook.</summary>
    public override void Update(double delta)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
            paused = !paused;

        input.Track = !paused;
        input.CursorMode = paused ? CursorMode.Normal : CursorMode.Disabled;

        if (!paused)
        {
            var speed = (float)(delta * 10);

            if (keyboard.IsKeyDown(Keys.W))
                camera.Offset += camera.Front * speed;
            if (keyboard.IsKeyDown(Keys.A))
                camera.Offset -= camera.Right * speed;
            if (keyboard.IsKeyDown(Keys.S))
                camera.Offset -= camera.Front * speed;
            if (keyboard.IsKeyDown(Keys.D))
                camera.Offset += camera.Right * speed;

            if (keyboard.IsKeyDown(Keys.Space))
                camera.Offset.Y += speed;
            if (keyboard.IsKeyDown(Keys.LeftControl))
                camera.Offset.Y -= speed;
        }

        if (keyboard.IsKeyPressedRepeated(Keys.Minus) && scale.Numerator > scale.Denominator)
            scale.Numerator--;
        if (keyboard.IsKeyPressedRepeated(Keys.Equal) && scale.Numerator < scale.Denominator * 4)
            scale.Numerator++;

        camera.Rotate(-mouse.Delta / 300);
        camera.PreventBackFlipsAndFrontFlips();
    }

    /// <summary>Computes the camera matrices and renders the uploaded triangle.</summary>
    public override void Render()
    {
        camera.ComputeVectors();
        perspective.ComputeMatrix(canvas.Size, camera);

        backbuffer.Clear();
        gl.Viewport(0, 0, (int)canvas.Size.X, (int)canvas.Size.Y);
        gl.UseProgram(positionColorProgram3D.Id);
        positionColorProgram3D.View = perspective.View;
        positionColorProgram3D.Projection = perspective.Projection;
        gl.BindVertexArray(vao);
        gl.DrawArrays(GlPrimitiveType.Triangles, 0, 3);
        gl.UnbindVertexArray();
        gl.UnuseProgram();
        gl.ResetViewport();
    }

    /// <summary>Draws camera debug text and a translucent pause overlay.</summary>
    public override void Draw()
    {
        var topLeft = new Vec2(scale[25], scale[25]);
        var font = roboto[scale[27]];
        sprites.Batch.Write(font, text.Format("Position: {0:F3}", camera.Offset), topLeft);
        sprites.Batch.Write(font, text.Format("Rotation: {0:F3}", camera.Rotation), topLeft + (0, font.Metrics.Height));

        if (paused)
            sprites.Batch.Draw((0, 0), canvas.Size, (0.3f, 0.3f, 0.3f, 0.3f));
    }
}

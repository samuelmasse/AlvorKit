RootLoop.RunGlfw<TextureCubeState>();

/// <summary>Draws the old textured cube demo with TOML controls, camera movement, and a tiny HUD.</summary>
[Root]
internal sealed class TextureCubeState(
    RootInput input,
    RootMouse mouse,
    RootGl gl,
    RootScreen screen,
    RootBackbuffer backbuffer,
    RootCanvas canvas,
    RootQuadIndexBuffer quadIndexBuffer,
    RootPositionColorTextureProgram3D positionColorTextureProgram3D,
    RootCube cube,
    RootPngs pngs,
    RootSprites sprites,
    RootRoboto roboto,
    RootText text,
    RootScale scale,
    RootMetrics metrics,
    RootControlsToml controlsToml,
    TextureCubeControls controls) : State
{
    private readonly Camera3D camera = new();
    private readonly Perspective3D perspective = new();
    private Texture texture = null!;
    private GlVertexArrayHandle vao;
    private int count;
    private bool paused;

    /// <summary>Loads the texture, uploads cube vertices, binds controls, and shows the window.</summary>
    public override void Load()
    {
        var image = pngs["Noise.png"];
        texture = new Texture2D(gl, image.Size)
        {
            PixelsMipmap = image.Pixels.Span,
            MagFilter = GlTextureMagFilter.Nearest,
            MinFilter = GlTextureMinFilter.NearestMipmapLinear,
        };

        PositionColorTextureVertex[] vertices =
        [
            .. VertexQuad(cube.Front.Quad, 1),
            .. VertexQuad(cube.Back.Quad, 1),
            .. VertexQuad(cube.Top.Quad, 0.8f),
            .. VertexQuad(cube.Bottom.Quad, 0.8f),
            .. VertexQuad(cube.Left.Quad, 0.5f),
            .. VertexQuad(cube.Right.Quad, 0.5f),
        ];

        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, gl.GenBuffer());
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, quadIndexBuffer.Id);
        gl.BufferData(GlBufferTarget.ArrayBuffer, vertices.AsSpan(), GlBufferUsage.StaticDraw);
        positionColorTextureProgram3D.SetAttributes();
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();

        camera.Offset = (0, 0, 10);
        quadIndexBuffer.EnsureCapacity(vertices.Length);
        count = quadIndexBuffer.IndexCount(vertices.Length);

        controlsToml.AddFromFile("Controls.toml");

        screen.Title = "AlvorKit.Engine.TextureCube.Demo";
        screen.IsVisible = true;
    }

    /// <summary>Prints the same simple exit message as the old demo.</summary>
    public override void Unload()
    {
        Console.WriteLine("Exiting...");
    }

    /// <summary>Updates pause state, camera movement, UI scale, and the demo exception trigger.</summary>
    public override void Update(double time)
    {
        if (controls.Pause.Run())
            paused = !paused;

        input.Track = !paused;
        input.CursorMode = paused ? CursorMode.Normal : CursorMode.Disabled;

        if (!paused)
        {
            var speed = (float)(time * 10);

            if (controls.CameraFront.Run())
                camera.Offset += camera.Front * speed;
            if (controls.CameraLeft.Run())
                camera.Offset -= camera.Right * speed;
            if (controls.CameraBack.Run())
                camera.Offset -= camera.Front * speed;
            if (controls.CameraRight.Run())
                camera.Offset += camera.Right * speed;

            if (controls.CameraUp.Run())
                camera.Offset.Y += speed;
            if (controls.CameraDown.Run())
                camera.Offset.Y -= speed;
        }

        if (scale.Numerator > scale.Denominator && controls.ZoomOut.Run())
            scale.Numerator--;
        if (scale.Numerator < scale.Denominator * 4 && controls.ZoomIn.Run())
            scale.Numerator++;

        if (controls.ThrowException.Run())
            throw new Exception("This is an example exception!");

        camera.Rotate(-mouse.Delta / 300);
        camera.PreventBackFlipsAndFrontFlips();
    }

    /// <summary>Computes camera matrices and draws the textured cube with depth test and back-face culling.</summary>
    public override void Render()
    {
        camera.ComputeVectors();
        perspective.ComputeMatrix(canvas.Size, camera);

        backbuffer.Clear();
        gl.Viewport(0, 0, (int)canvas.Size.X, (int)canvas.Size.Y);
        gl.Enable(GlEnableCap.DepthTest);
        gl.DepthFunc(GlDepthFunction.Less);
        gl.Enable(GlEnableCap.CullFace);
        gl.CullFace(GlTriangleFace.Back);

        gl.UseProgram(positionColorTextureProgram3D.Id);
        positionColorTextureProgram3D.View = perspective.View;
        positionColorTextureProgram3D.Projection = perspective.Projection;
        texture.Bind(positionColorTextureProgram3D.SamplerTexture);
        gl.BindVertexArray(vao);
        gl.DrawElements(GlPrimitiveType.Triangles, count, GlDrawElementsType.UnsignedInt, 0);
        gl.UnbindVertexArray();
        texture.Unbind(positionColorTextureProgram3D.SamplerTexture);
        gl.UnuseProgram();

        gl.ResetCullFace();
        gl.Disable(GlEnableCap.CullFace);
        gl.ResetDepthFunc();
        gl.Disable(GlEnableCap.DepthTest);
        gl.ResetViewport();
    }

    /// <summary>Draws the slower FPS HUD, camera debug text, and pause overlay.</summary>
    public override void Draw()
    {
        var topLeft = new Vec2(scale[25], scale[25]);
        var font = roboto[scale[27]];
        sprites.Batch.Write(
            font,
            text.Format("Frame: {0}. {1:F3} ms ({2} FPS)", metrics.Frame.Ticks, metrics.FrameWindow.Average, metrics.FrameWindow.Ticks),
            topLeft);
        sprites.Batch.Write(font, text.Format("Position: {0:F3}", camera.Offset), topLeft + (0, font.Metrics.Height));
        sprites.Batch.Write(font, text.Format("Rotation: {0:F3}", camera.Rotation), topLeft + (0, font.Metrics.Height * 2));

        if (paused)
            sprites.Batch.Draw((0, 0), canvas.Size, (0.3f, 0.3f, 0.3f, 0.3f));
    }

    /// <summary>Builds one textured cube face with a simple per-face color multiplier.</summary>
    private static PositionColorTextureVertex[] VertexQuad(Quad quad, float shadow) =>
    [
        new(quad.TopLeft, new Vec3(1, 0, 0) * shadow, (0, 1)),
        new(quad.TopRight, new Vec3(0, 1, 0) * shadow, (1, 1)),
        new(quad.BottomLeft, new Vec3(0, 0, 1) * shadow, (0, 0)),
        new(quad.BottomRight, new Vec3(1, 0, 1) * shadow, (1, 0)),
    ];
}

/// <summary>Names the controls used by the texture cube demo.</summary>
internal sealed record TextureCubeControls(
    Control Pause,
    Control ZoomIn,
    Control ZoomOut,
    Control CameraFront,
    Control CameraRight,
    Control CameraBack,
    Control CameraLeft,
    Control CameraUp,
    Control CameraDown,
    Control ThrowException) : ControlList;

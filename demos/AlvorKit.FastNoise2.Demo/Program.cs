RootLoop.RunGlfw<FastNoise2DemoState>();

/// <summary>Shows a basic Craftdig-style FastNoise2 FBm field through the RootLoop sprite pipeline.</summary>
[Root]
internal sealed class FastNoise2DemoState(
    RootInput input,
    RootMouse mouse,
    RootKeyboard keyboard,
    RootGl gl,
    RootScreen screen,
    RootBackbuffer backbuffer,
    RootCanvas canvas,
    RootSprites sprites) : State
{
    private static readonly Vec2u InitialSize = (1100u, 720u);

    private FastNoise2Field? field;
    private Vec2 lastDragMousePosition;
    private bool wasDragging;

    /// <summary>Creates the noise texture and shows the demo window.</summary>
    public override void Load()
    {
        input.Track = true;
        input.CursorMode = CursorMode.Normal;

        screen.Title = "AlvorKit FastNoise2 Craftdig FBm";
        screen.Size = InitialSize;

        field = new FastNoise2Field(gl, InitialSize);
        field.Regenerate();

        Console.WriteLine(
            "FastNoise2 Craftdig FBm controls: drag or WASD/arrow keys pan, wheel or +/- zoom, " +
            "R reseeds, F11 fullscreen, Esc exits.");

        screen.IsVisible = true;
    }

    /// <summary>Releases the FastNoise2 node and GPU texture.</summary>
    public override void Unload()
    {
        field?.Dispose();
        field = null;
    }

    /// <summary>Handles input and regenerates the field only when the visual state changed.</summary>
    public override void Update(double delta)
    {
        if (field is not { } noise)
            return;

        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            screen.Close();
            return;
        }

        if (keyboard.IsKeyPressed(Keys.F11))
            screen.ToggleFullscreen();

        var changed = false;

        if (keyboard.IsKeyPressed(Keys.R))
        {
            noise.Seed = unchecked(noise.Seed + 1337);
            changed = true;
        }

        changed |= UpdateScale(noise);
        changed |= UpdateKeyboardOffset(noise, delta);
        changed |= UpdateMousePan(noise);

        if (changed)
            noise.Regenerate();
    }

    /// <summary>Clears the backbuffer before the RootLoop sprite pass draws the noise texture.</summary>
    public override void Render() => backbuffer.Clear((0f, 0f, 0f, 1f));

    /// <summary>Draws the generated grayscale noise texture across the canvas.</summary>
    public override void Draw()
    {
        if (field is not { } noise)
            return;

        sprites.Batch.Draw(noise.Texture, (0f, 0f), new Vec2(canvas.Size.X, canvas.Size.Y), Vec4.One);
    }

    private bool UpdateScale(FastNoise2Field noise)
    {
        var multiplier = 1f;

        if (keyboard.IsKeyPressedRepeated(Keys.Equal))
            multiplier *= 0.9f;
        if (keyboard.IsKeyPressedRepeated(Keys.Minus))
            multiplier *= 1.1f;

        var wheel = mouse.Wheel.Y;
        if (MathF.Abs(wheel) > 0f)
            multiplier *= MathF.Pow(0.9f, wheel);

        if (MathF.Abs(multiplier - 1f) < 0.0001f)
            return false;

        return noise.ZoomAround((noise.Width * 0.5f, noise.Height * 0.5f), noise.Step * multiplier);
    }

    private bool UpdateKeyboardOffset(FastNoise2Field noise, double delta)
    {
        var speed = (float)(delta * Math.Min(noise.Width, noise.Height) * 0.65f);
        var offset = Vec2.Zero;

        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
            offset.X -= speed;
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
            offset.X += speed;
        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
            offset.Y -= speed;
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
            offset.Y += speed;

        if (offset.LengthSquared <= 0f)
            return false;

        noise.Offset += offset;
        return true;
    }

    private bool UpdateMousePan(FastNoise2Field noise)
    {
        if (!mouse.IsMainDown())
        {
            wasDragging = false;
            return false;
        }

        if (!wasDragging)
        {
            lastDragMousePosition = mouse.Position;
            wasDragging = true;
            return false;
        }

        var dragDelta = mouse.Position - lastDragMousePosition;
        lastDragMousePosition = mouse.Position;
        if (dragDelta.LengthSquared <= 0f)
            return false;

        var canvasWidth = MathF.Max(canvas.Size.X, 1f);
        var canvasHeight = MathF.Max(canvas.Size.Y, 1f);

        noise.Offset = (
            noise.Offset.X - (dragDelta.X * noise.Width / canvasWidth),
            noise.Offset.Y + (dragDelta.Y * noise.Height / canvasHeight));
        return true;
    }
}

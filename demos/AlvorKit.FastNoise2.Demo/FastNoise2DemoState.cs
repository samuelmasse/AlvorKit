namespace AlvorKit;

/// <summary>Runs the verified FastNoise2 node gallery through the RootLoop sprite pipeline.</summary>
[Root]
internal class FastNoise2DemoState(
    FastNoise2GalleryGraphs graphs,
    RootKeyboard keyboard,
    RootGl gl,
    RootScreen screen,
    RootBackbuffer backbuffer,
    RootCanvas canvas,
    RootSprites sprites) : State
{
    private static readonly Vec2u InitialSize = (1100u, 720u);

    private FastNoise2Gallery? gallery;

    /// <summary>Creates the first typed gallery graph and shows the demo window.</summary>
    public override void Load()
    {
        screen.Title = "AlvorKit FastNoise2 feature gallery";
        screen.Size = InitialSize;

        var database = FastNoise2FeatureCatalog.Load();
        gallery = new(graphs, gl, InitialSize, database);
        UpdateTitle();

        Console.WriteLine(
            "Controls: Left/Right changes node, Space changes generation mode, R reseeds, " +
            "F11 toggles fullscreen, Esc exits.");
        screen.IsVisible = true;
    }

    /// <summary>Drops the preview; the injected graph and root GL layer own native resources.</summary>
    public override void Unload() => gallery = null;

    /// <summary>Handles feature, generation-shape, seed, fullscreen, and exit controls.</summary>
    public override void Update(double delta)
    {
        if (gallery is not { } featureGallery)
            return;

        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            screen.Close();
            return;
        }

        if (keyboard.IsKeyPressed(Keys.F11))
            screen.ToggleFullscreen();

        if (keyboard.IsKeyPressed(Keys.Right))
        {
            featureGallery.Next();
            UpdateTitle();
        }

        if (keyboard.IsKeyPressed(Keys.Left))
        {
            featureGallery.Previous();
            UpdateTitle();
        }

        if (keyboard.IsKeyPressed(Keys.Space))
        {
            featureGallery.NextMode();
            UpdateTitle();
        }

        if (keyboard.IsKeyPressed(Keys.R))
        {
            featureGallery.Reseed();
            UpdateTitle();
        }
    }

    /// <summary>Clears the backbuffer before the sprite pass draws the generated preview.</summary>
    public override void Render() => backbuffer.Clear((0.015f, 0.018f, 0.025f, 1f));

    /// <summary>Draws the current FastNoise2 output across the visible canvas.</summary>
    public override void Draw()
    {
        if (gallery is not { } featureGallery)
            return;

        sprites.Batch.Draw(featureGallery.Texture, (0f, 0f), new Vec2(canvas.Size.X, canvas.Size.Y), Vec4.One);
    }

    private void UpdateTitle()
    {
        if (gallery is { } featureGallery)
            screen.Title = featureGallery.Title;
    }
}

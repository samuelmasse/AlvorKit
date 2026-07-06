RootLoop.RunGlfw<RawApisState>();

/// <summary>Shows the native-style APIs that <see cref="RootLoop"/> seeds into the root injector.</summary>
[Root]
internal sealed unsafe class RawApisState(
    Gl gl,
    Glfw glfw,
    GlfwWindow window,
    Fn fn,
    Ft ft,
    Ma ma,
    Xxh xxh,
    RootInput input,
    RootScreen screen,
    RootCanvas canvas,
    RootSprites sprites,
    RootRoboto roboto,
    RootText text,
    RootScale scale) : State
{
    private static readonly Vec2u InitialSize = (1120u, 680u);
    private const uint XxHashSeed = 0xC0DE_51E5u;
    private const double TitleRefreshSeconds = 0.35;

    private RawApiSnapshot snapshot = RawApiSnapshot.Empty;
    private Vec2i framebufferSize;
    private Vec4 clearColor = (0.05f, 0.07f, 0.09f, 1f);
    private double nextTitleRefresh;
    private bool reloadWasDown;

    /// <summary>Runs one-shot native probes, configures direct input tracking, and shows the window.</summary>
    public override void Load()
    {
        input.Track = true;
        input.CursorMode = CursorMode.Normal;

        RefreshSnapshot();
        screen.IsVisible = true;

        Console.WriteLine("AlvorKit.Engine.Demo.RawApis");
        Console.WriteLine("RootLoop injected Gl, Glfw, GlfwWindow, Fn, Ft, Ma, and Xxh directly into this state.");
        Console.WriteLine("Esc exits. R re-runs the one-shot raw API probes.");
    }

    /// <summary>Polls input through raw GLFW and updates the native window title.</summary>
    public override void Update(double delta)
    {
        if (glfw.GetKey(window, GlfwKey.Escape) == GlfwInputAction.Press)
            glfw.SetWindowShouldClose(window, true);

        var reloadIsDown = glfw.GetKey(window, GlfwKey.R) == GlfwInputAction.Press;
        if (reloadIsDown && !reloadWasDown)
            RefreshSnapshot();
        reloadWasDown = reloadIsDown;

        if (glfw.GetTime() >= nextTitleRefresh)
        {
            nextTitleRefresh = glfw.GetTime() + TitleRefreshSeconds;
            glfw.SetWindowTitle(
                window,
                $"AlvorKit.Engine.Demo.RawApis | hash {snapshot.Hash32:X8} | noise {snapshot.NoiseSample:0.000}");
        }
    }

    /// <summary>Clears the backbuffer through the raw OpenGL binding instead of the RootGl layer.</summary>
    public override void Render()
    {
        glfw.GetFramebufferSize(window, out var width, out var height);
        framebufferSize = (width, height);

        gl.Viewport(0, 0, Math.Max(width, 1), Math.Max(height, 1));
        gl.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
        gl.Clear(GlClearBufferMask.ColorBufferBit);
    }

    /// <summary>Draws the values produced by the raw API calls so the sample can be inspected visually.</summary>
    public override void Draw()
    {
        var font = roboto[scale[22]];
        var lineHeight = font.Metrics.Height;
        var cursor = new Vec2(scale[24], scale[24]);

        WriteLine(font, ref cursor, lineHeight, "Injected raw APIs in this RootLoop state");
        WriteLine(font, ref cursor, lineHeight, "----------------------------------------");
        WriteLine(font, ref cursor, lineHeight, text.Format("Gl.GetString(Version): {0}", snapshot.OpenGlVersion));
        WriteLine(font, ref cursor, lineHeight, text.Format("Gl.GetString(Renderer): {0}", snapshot.OpenGlRenderer));
        WriteLine(font, ref cursor, lineHeight, text.Format("Glfw.GetVersion: {0}", snapshot.GlfwVersion));
        WriteLine(font, ref cursor, lineHeight, text.Format("GlfwWindow handle: 0x{0:X}", window.Handle));
        WriteLine(font, ref cursor, lineHeight, text.Format("Glfw.GetFramebufferSize: {0} x {1}", framebufferSize.X, framebufferSize.Y));
        WriteLine(font, ref cursor, lineHeight, text.Format("RootCanvas.Size: {0:0} x {1:0}", canvas.Size.X, canvas.Size.Y));
        WriteLine(font, ref cursor, lineHeight, text.Format("Fn.GenUniformGrid3D(Simplex): {0:0.000000}", snapshot.NoiseSample));
        WriteLine(font, ref cursor, lineHeight, text.Format("Xxh.Hash32(snapshot): 0x{0:X8}", snapshot.Hash32));
        WriteLine(font, ref cursor, lineHeight, text.Format("Xxh.GetVersionNumber: {0}", snapshot.XxHashVersion));
        WriteLine(font, ref cursor, lineHeight, text.Format("Ft.LibraryVersion: {0}", snapshot.FreeTypeVersion));
        WriteLine(font, ref cursor, lineHeight, text.Format("Ma.VersionString: {0}", snapshot.MiniAudioVersion));
        WriteLine(font, ref cursor, lineHeight, "");
        WriteLine(font, ref cursor, lineHeight, "Raw GLFW controls: Esc closes, R refreshes the native probes.");
        WriteLine(font, ref cursor, lineHeight, "Raw GL clears the scene; RootSprites draws this HUD afterward.");
    }

    /// <summary>Collects native binding results that are safe and cheap to present as demo diagnostics.</summary>
    private void RefreshSnapshot()
    {
        gl.GetString(GlStringName.Version, out var openGlVersion);
        gl.GetString(GlStringName.Renderer, out var openGlRenderer);

        glfw.GetVersion(out var glfwMajor, out var glfwMinor, out var glfwRevision);
        var glfwVersion = $"{glfwMajor}.{glfwMinor}.{glfwRevision}";
        var noiseSample = SampleFastNoise();
        var xxHashVersion = VersionString(xxh.GetVersionNumber());
        var freeTypeVersion = ProbeFreeTypeVersion();
        var miniAudioVersion = ProbeMiniAudioVersion();

        var hash = HashSnapshot(
            openGlVersion,
            openGlRenderer,
            glfwVersion,
            noiseSample,
            xxHashVersion,
            freeTypeVersion,
            miniAudioVersion);

        snapshot = new(
            openGlVersion ?? "(null)",
            openGlRenderer ?? "(null)",
            glfwVersion,
            noiseSample,
            hash,
            xxHashVersion,
            freeTypeVersion,
            miniAudioVersion);

        clearColor = ColorFromHash(hash, noiseSample);
    }

    /// <summary>Creates one FastNoise2 metadata node, samples a one-cell 3D grid, and releases the node.</summary>
    private float SampleFastNoise()
    {
        var node = CreateFastNoiseNode("Simplex");
        Span<float> sample = stackalloc float[1];
        Span<float> minMax = stackalloc float[2];

        try
        {
            fn.GenUniformGrid3D(node, sample, 18.75f, -4.5f, 60f, 1, 1, 1, 0.9f, 0.9f, 0.9f, 4242, minMax);
            return sample[0];
        }
        finally
        {
            fn.DeleteNodeRef(node);
        }
    }

    /// <summary>Finds a FastNoise2 metadata node by name and creates a native node reference.</summary>
    private FnNode CreateFastNoiseNode(string metadataName)
    {
        var count = fn.GetMetadataCount();
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataName(i, out var name);
            if (!string.Equals(name, metadataName, StringComparison.OrdinalIgnoreCase))
                continue;

            var node = fn.NewFromMetadata(i, uint.MaxValue);
            if (node != default)
                return node;
        }

        throw new InvalidOperationException($"FastNoise2 did not expose a creatable {metadataName} node.");
    }

    /// <summary>Initializes FreeType only long enough to prove the injected raw Ft backend is callable.</summary>
    private string ProbeFreeTypeVersion()
    {
        RequireFreeType("FT_Init_FreeType", ft.InitFreeType(out var library));
        try
        {
            ft.LibraryVersion(library, out var major, out var minor, out var patch);
            return $"{major}.{minor}.{patch}";
        }
        finally
        {
            _ = ft.DoneFreeType(library);
        }
    }

    /// <summary>Reads miniaudio runtime metadata without opening an audio device.</summary>
    private string ProbeMiniAudioVersion()
    {
        ma.VersionString(out var version);
        return version ?? "(null)";
    }

    /// <summary>Hashes the captured native snapshot through the injected xxHash backend.</summary>
    private uint HashSnapshot(
        string? openGlVersion,
        string? openGlRenderer,
        string glfwVersion,
        float noiseSample,
        string xxHashVersion,
        string freeTypeVersion,
        string miniAudioVersion)
    {
        Span<byte> bytes = stackalloc byte[512];
        var textBytes = Encoding.UTF8.GetBytes(
            string.Create(
                CultureInfo.InvariantCulture,
                $"{openGlVersion}|{openGlRenderer}|{glfwVersion}|{noiseSample:R}|{xxHashVersion}|{freeTypeVersion}|{miniAudioVersion}"));

        if (textBytes.Length > bytes.Length)
            return xxh.Hash32(textBytes, XxHashSeed);

        textBytes.CopyTo(bytes);
        return xxh.Hash32(bytes[..textBytes.Length], XxHashSeed);
    }

    /// <summary>Uses the snapshot hash and noise sample to make the raw GL clear visibly data-driven.</summary>
    private static Vec4 ColorFromHash(uint hash, float noiseSample)
    {
        var red = 0.04f + ((hash & 0xFFu) / 255f * 0.18f);
        var green = 0.05f + (((hash >> 8) & 0xFFu) / 255f * 0.16f);
        var blue = 0.08f + ((noiseSample + 1f) * 0.08f);
        return (red, green, Math.Clamp(blue, 0.08f, 0.32f), 1f);
    }

    /// <summary>Draws one HUD line and advances the text cursor.</summary>
    private void WriteLine(FontSize font, ref Vec2 cursor, float lineHeight, ReadOnlySpan<char> value)
    {
        if (value.Length > 0)
            sprites.Batch.Write(font, value, cursor);

        cursor.Y += lineHeight;
    }

    private static void RequireFreeType(string nativeName, int error)
    {
        if (error != 0)
            throw new InvalidOperationException($"{nativeName} returned FT_Error {error}.");
    }

    private static string VersionString(uint versionNumber) =>
        $"{versionNumber / 10000}.{versionNumber / 100 % 100}.{versionNumber % 100}";
}

/// <summary>Values captured through direct native-style API calls and displayed by the raw API demo.</summary>
internal sealed record RawApiSnapshot(
    string OpenGlVersion,
    string OpenGlRenderer,
    string GlfwVersion,
    float NoiseSample,
    uint Hash32,
    string XxHashVersion,
    string FreeTypeVersion,
    string MiniAudioVersion)
{
    public static RawApiSnapshot Empty { get; } = new(
        "(not loaded)",
        "(not loaded)",
        "(not loaded)",
        0f,
        0,
        "(not loaded)",
        "(not loaded)",
        "(not loaded)");
}

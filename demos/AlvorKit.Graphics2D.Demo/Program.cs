const int SmallTextureSize = 256;
const int BigTextureSize = 1024;
const int GridCells = 64;

var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);

var primaryMonitor = glfw.GetPrimaryMonitor();
glfw.GetMonitorWorkarea(primaryMonitor, out _, out _, out var monitorWidth, out var monitorHeight);
var initialWidth = Math.Max(800, monitorWidth * 3 / 4);
var initialHeight = Math.Max(600, monitorHeight * 3 / 4);

var window = glfw.CreateWindow(initialWidth, initialHeight, "AlvorKit Graphics2D demo", default, default);
if (window == default)
{
    glfw.Terminate();
    throw new InvalidOperationException("Failed to create the GLFW window.");
}

glfw.MakeContextCurrent(window);
glfw.SwapInterval(1);

var rawGl = new GlBackend(glfw.GetProcAddress);
var gl = new GlLayer(rawGl);
gl.GetString(GlStringName.Version, out var version);
gl.GetString(GlStringName.ShadingLanguageVersion, out var glsl);
Console.WriteLine("OpenGL {0} (GLSL {1}) - Esc exits. G/O/T/Y/C toggle draw modes; arrows rotate; H/V flip; F11 fullscreen.", version, glsl);

var sprites = new SpriteBatch(gl);
var texture = new Texture2D(gl, "grid-noise", (SmallTextureSize, SmallTextureSize))
{
    PixelsMipmap = Noise.Generate(SmallTextureSize),
    MinFilter = GlTextureMinFilter.LinearMipmapLinear,
    MagFilter = GlTextureMagFilter.Linear
};
var bigTexture = new Texture2D(gl, "large-noise", (BigTextureSize, BigTextureSize))
{
    PixelsMipmap = Noise.Generate(BigTextureSize),
    MinFilter = GlTextureMinFilter.LinearMipmapLinear,
    MagFilter = GlTextureMagFilter.Linear
};

var rotation = SpriteBatchRotation.None;
var flip = SpriteBatchFlip.None;
var textureOffset = false;
var textureSizeOffset = false;
var enableGrid = true;
var enableOverlay = true;
var enableClip = false;

var keyWasDown = new bool[512];
var fullscreen = false;
var windowedX = 0;
var windowedY = 0;
var windowedWidth = initialWidth;
var windowedHeight = initialHeight;

// Draws one frame from the current window, input, and sprite-mode state.
void RenderFrame(int framebufferWidth, int framebufferHeight)
{
    if (framebufferWidth <= 0 || framebufferHeight <= 0)
        return;

    glfw.GetWindowSize(window, out var windowWidth, out var windowHeight);
    Vec2 canvasSize = (framebufferWidth, framebufferHeight);
    var mousePosition = MousePosition(framebufferWidth, framebufferHeight, windowWidth, windowHeight);

    gl.Viewport(0, 0, framebufferWidth, framebufferHeight);
    gl.ClearColor(0.2f, 0.5f, 0.1f, 1f);
    gl.Clear(GlClearBufferMask.ColorBufferBit);
    gl.Enable(GlEnableCap.Blend);
    gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha);

    sprites.Begin(canvasSize);
    DrawSprites(canvasSize, mousePosition);
    sprites.End();

    gl.ResetBlendFunc();
    gl.Disable(GlEnableCap.Blend);
    gl.ResetClearColor();
    gl.ResetViewport();
    glfw.SwapBuffers(window);
}

// Draws the grid, large transformed texture, overlay, and guide lines.
void DrawSprites(Vec2 canvasSize, Vec2 mousePosition)
{
    var grid = (float)GridCells;
    var unit = canvasSize / grid;
    sprites.Writer.Clip = enableClip ? new SpriteBatchClip(mousePosition - (unit * 12f), mousePosition + (unit * 12f)) : null;

    if (enableGrid)
        DrawGrid(canvasSize, mousePosition, unit, grid);

    Vec2 bigBox = (canvasSize.Y * 0.5f, canvasSize.Y * 0.5f);
    var sourcePosition = Vec2.Zero;
    Vec2 sourceSize = bigTexture.Size;

    if (textureOffset)
    {
        sourcePosition += sourceSize * 0.5f;
        sourceSize *= 0.5f;
    }

    if (textureSizeOffset)
        sourceSize *= 0.5f;

    sprites.Writer.Draw(
        bigTexture,
        mousePosition - (bigBox * 0.5f),
        bigBox,
        sourcePosition,
        sourceSize,
        (1f, 1f, 1f, 0.25f),
        rotation,
        flip);

    if (enableOverlay)
        sprites.Writer.Draw(texture, Vec2.Zero, canvasSize, (1f, 1f, 1f, 0.1f));

    sprites.Writer.DrawLine(Vec2.Zero, mousePosition);
    sprites.Writer.DrawLine(canvasSize * 0.5f, mousePosition, 20f, (1f, 0f, 0f, 1f));
}

// Draws the coloured cell grid that makes clipping, alpha, batching, and texture reuse visible.
void DrawGrid(Vec2 canvasSize, Vec2 mousePosition, Vec2 unit, float grid)
{
    for (var y = 0; y < GridCells; y++)
    {
        for (var x = 0; x < GridCells; x++)
        {
            Vec2 gridPosition = (x, y);
            var start = gridPosition * unit;
            var end = start + unit;
            var brightness = 1f;

            if (mousePosition.X >= start.X && mousePosition.X <= end.X)
                brightness -= 0.25f;
            if (mousePosition.Y >= start.Y && mousePosition.Y <= end.Y)
                brightness -= 0.25f;

            sprites.Writer.Draw(texture, start, unit, (y / grid, x / grid, brightness, 1f));
        }
    }
}

// Reads input once per poll-loop turn, preserving the old edge-triggered toggle behaviour.
void UpdateInput()
{
    if (glfw.GetKey(window, GlfwKey.Escape) == GlfwInputAction.Press)
        glfw.SetWindowShouldClose(window, true);

    if (KeyPressed(GlfwKey.F11))
        ToggleFullscreen();
    if (KeyPressed(GlfwKey.G))
        enableGrid = !enableGrid;
    if (KeyPressed(GlfwKey.O))
        enableOverlay = !enableOverlay;
    if (KeyPressed(GlfwKey.T))
        textureOffset = !textureOffset;
    if (KeyPressed(GlfwKey.Y))
        textureSizeOffset = !textureSizeOffset;
    if (KeyPressed(GlfwKey.C))
        enableClip = !enableClip;
    if (KeyPressed(GlfwKey.Left))
        rotation = RotateLeft(rotation);
    if (KeyPressed(GlfwKey.Right))
        rotation = RotateRight(rotation);
    if (KeyPressed(GlfwKey.H))
        flip ^= SpriteBatchFlip.Horizontal;
    if (KeyPressed(GlfwKey.V))
        flip ^= SpriteBatchFlip.Vertical;
}

// Returns true once when a key transitions from released to pressed.
bool KeyPressed(GlfwKey key)
{
    var index = (int)key;
    var down = glfw.GetKey(window, key) == GlfwInputAction.Press;
    if ((uint)index >= keyWasDown.Length)
        return down;

    var pressed = down && !keyWasDown[index];
    keyWasDown[index] = down;
    return pressed;
}

// Toggles between windowed mode and the primary monitor's current fullscreen mode.
void ToggleFullscreen()
{
    if (!fullscreen)
    {
        glfw.GetWindowPos(window, out windowedX, out windowedY);
        glfw.GetWindowSize(window, out windowedWidth, out windowedHeight);
        glfw.GetMonitorWorkarea(primaryMonitor, out _, out _, out var fullWidth, out var fullHeight);
        glfw.SetWindowMonitor(window, primaryMonitor, 0, 0, fullWidth, fullHeight, (int)GlfwEnum.DontCare);
        fullscreen = true;
        return;
    }

    glfw.SetWindowMonitor(window, default, windowedX, windowedY, windowedWidth, windowedHeight, (int)GlfwEnum.DontCare);
    fullscreen = false;
}

// Converts GLFW content-area cursor coordinates into framebuffer pixels for high-DPI windows.
Vec2 MousePosition(int framebufferWidth, int framebufferHeight, int windowWidth, int windowHeight)
{
    glfw.GetCursorPos(window, out var mouseX, out var mouseY);
    var scaleX = windowWidth <= 0 ? 1f : framebufferWidth / (float)windowWidth;
    var scaleY = windowHeight <= 0 ? 1f : framebufferHeight / (float)windowHeight;
    return ((float)mouseX * scaleX, (float)mouseY * scaleY);
}

// Rotates texture coordinates one quarter-turn counter-clockwise.
static SpriteBatchRotation RotateLeft(SpriteBatchRotation value) =>
    value switch
    {
        SpriteBatchRotation.None => SpriteBatchRotation.Clockwise270,
        SpriteBatchRotation.Clockwise90 => SpriteBatchRotation.None,
        SpriteBatchRotation.Clockwise180 => SpriteBatchRotation.Clockwise90,
        _ => SpriteBatchRotation.Clockwise180
    };

// Rotates texture coordinates one quarter-turn clockwise.
static SpriteBatchRotation RotateRight(SpriteBatchRotation value) =>
    value switch
    {
        SpriteBatchRotation.None => SpriteBatchRotation.Clockwise90,
        SpriteBatchRotation.Clockwise90 => SpriteBatchRotation.Clockwise180,
        SpriteBatchRotation.Clockwise180 => SpriteBatchRotation.Clockwise270,
        _ => SpriteBatchRotation.None
    };

// The callback repaints while the platform is inside modal resize loops.
glfw.SetFramebufferSizeCallback(window, (_, width, height) => RenderFrame(width, height));

while (!glfw.WindowShouldClose(window))
{
    glfw.PollEvents();
    UpdateInput();
    glfw.GetFramebufferSize(window, out var width, out var height);
    RenderFrame(width, height);
}

bigTexture.Dispose();
texture.Dispose();
sprites.Dispose();
gl.Dispose();
glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

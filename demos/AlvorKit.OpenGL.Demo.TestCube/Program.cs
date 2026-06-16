var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);

var window = glfw.CreateWindow(960, 720, "AlvorKit testcube", default, default);
if (window == default)
{
    glfw.Terminate();
    throw new InvalidOperationException("Failed to create the GLFW window.");
}

glfw.MakeContextCurrent(window);

// Vsync keeps the rotation pace comfortable and avoids burning a full CPU core for a teaching demo.
glfw.SwapInterval(1);

var rawGl = new GlBackend(glfw.GetProcAddress);
var gl = new GlLayer(rawGl);
gl.GetString(GlStringName.Version, out var version);
gl.GetString(GlStringName.ShadingLanguageVersion, out var glsl);
Console.WriteLine($"OpenGL {version} (GLSL {glsl}) - press Escape or close the window to exit.");

var cube = TestCube.Load(gl);
var clock = Stopwatch.StartNew();
gl.Enable(GlEnableCap.DepthTest);

// Draws one frame with all transient layer state reset before returning to the event loop.
void RenderFrame(int width, int height)
{
    if (width <= 0 || height <= 0)
        return;

    gl.Viewport(0, 0, width, height);
    gl.ClearColor(0.075f, 0.085f, 0.105f, 1f);
    gl.ClearDepth(1.0);
    gl.Clear(GlClearBufferMask.ColorBufferBit | GlClearBufferMask.DepthBufferBit);

    cube.Render((float)clock.Elapsed.TotalSeconds, width, height);

    gl.ResetClearDepth();
    gl.ResetClearColor();
    gl.ResetViewport();
    glfw.SwapBuffers(window);
}

// The callback repaints during platform resize loops, where the normal poll loop may be paused.
glfw.SetFramebufferSizeCallback(window, (_, width, height) => RenderFrame(width, height));

while (!glfw.WindowShouldClose(window))
{
    glfw.PollEvents();
    if (glfw.GetKey(window, GlfwKey.Escape) == GlfwInputAction.Press)
        glfw.SetWindowShouldClose(window, true);

    glfw.GetFramebufferSize(window, out var width, out var height);
    RenderFrame(width, height);
}

gl.Disable(GlEnableCap.DepthTest);
cube.Dispose();
gl.Dispose();
glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

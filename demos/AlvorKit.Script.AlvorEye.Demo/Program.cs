var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);

var window = glfw.CreateWindow(900, 640, "AlvorEye demo game", default, default);
if (window == default)
{
    glfw.Terminate();
    throw new InvalidOperationException("Failed to create the GLFW window.");
}

glfw.MakeContextCurrent(window);
glfw.SwapInterval(1);

Gl gl = new GlBackend(glfw.GetProcAddress);
gl.GetString(GlStringName.Version, out var version);
gl.GetString(GlStringName.ShadingLanguageVersion, out var glsl);
Console.WriteLine($"OpenGL {version} (GLSL {glsl}) - read AGENT_GOAL.md and solve the AlvorEye demo game.");

using var renderer = AlvorEyeDemoRenderer.Load(gl);
var state = new AlvorEyeDemoState();
var clock = Stopwatch.StartNew();
var lastFrameSeconds = 0.0;

// Text input is intentionally handled through the platform text path so AlvorEye's text action can solve one lock.
glfw.SetCharCallback(window, (_, codepoint) => state.AcceptCharacter(codepoint));
glfw.SetCursorPosCallback(window, (_, x, y) => state.AcceptCursor(x, y));
glfw.SetMouseButtonCallback(window, (_, button, action, _) => state.AcceptMouseButton(button, action));

// Draws one frame while the main loop and resize callback share the same rendering path.
void RenderFrame(int width, int height)
{
    if (width <= 0 || height <= 0)
        return;

    gl.Viewport(0, 0, width, height);
    gl.ClearColor(0.055f, 0.065f, 0.075f, 1f);
    gl.Clear(GlClearBufferMask.ColorBufferBit);
    renderer.Render(state, (float)clock.Elapsed.TotalSeconds, width, height);
    glfw.SwapBuffers(window);
}

// The callback repaints while Windows is inside modal resize loops.
glfw.SetFramebufferSizeCallback(window, (_, width, height) => RenderFrame(width, height));

while (!glfw.WindowShouldClose(window))
{
    glfw.PollEvents();
    if (glfw.GetKey(window, GlfwKey.Escape) == GlfwInputAction.Press)
        glfw.SetWindowShouldClose(window, true);

    var now = clock.Elapsed.TotalSeconds;
    var elapsed = Math.Clamp(now - lastFrameSeconds, 0.0, 0.05);
    lastFrameSeconds = now;
    state.Update(glfw, window, (float)elapsed);

    glfw.GetFramebufferSize(window, out var width, out var height);
    RenderFrame(width, height);
}

var result = JsonSerializer.Serialize(state.CreateResult(clock.Elapsed));
Console.WriteLine($"ALVOREYE_DEMO_RESULT {result}");
if (Environment.GetEnvironmentVariable("ALVOREYE_DEMO_RESULT_PATH") is { Length: > 0 } resultPath)
{
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(resultPath))!);
    File.WriteAllText(resultPath, result);
}

glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

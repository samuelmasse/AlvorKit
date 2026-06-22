var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);
glfw.WindowHint(GlfwWindowHint.Visible, false);
var window = glfw.CreateWindow(900, 640, "AlvorEye demo game", default, default);
if (window == default)
{
    glfw.Terminate();
    throw new InvalidOperationException("Failed to create the GLFW window.");
}

glfw.MakeContextCurrent(window);
var gl = new GlLayer(new GlBackend(glfw.GetProcAddress));
var host = new AgentGlfwWindowHost(glfw, window, gl)
{
    IsVSyncEnabled = true
};
var loop = new WindowLoop(host);
var canvas = new WindowCanvas(loop);
var screen = new WindowScreen(loop);
var keyboard = new Keyboard(loop);
var mouse = new Mouse(loop);
var input = new WindowInput(loop);

gl.GetString(GlStringName.Version, out var version);
gl.GetString(GlStringName.ShadingLanguageVersion, out var glsl);
Console.WriteLine($"OpenGL {version} (GLSL {glsl}) - read AGENT_GOAL.md and solve the AlvorEye demo game.");

var renderer = AlvorEyeDemoRenderer.Load(gl);
var state = new AlvorEyeDemoState();
double totalSeconds = 0;

input.Track = true;
screen.Title = "AlvorEye demo game";
screen.IsVisible = true;
loop.Update += Update;
loop.Render += Render;
loop.Run();

var result = JsonSerializer.Serialize(state.CreateResult(TimeSpan.FromSeconds(totalSeconds)));
Console.WriteLine($"ALVOREYE_DEMO_RESULT {result}");
if (Environment.GetEnvironmentVariable("ALVOREYE_DEMO_RESULT_PATH") is { Length: > 0 } resultPath)
{
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(resultPath))!);
    File.WriteAllText(resultPath, result);
}

renderer.Dispose();
glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

// Advances player movement, mouse locks, text locks, and close handling from windowing facades.
void Update(double elapsedSeconds)
{
    totalSeconds += elapsedSeconds;

    if (keyboard.IsKeyPressed(Keys.Escape))
        screen.Close();

    state.Update(keyboard, mouse, (float)elapsedSeconds);
}

// Draws one game frame through the current agent GLFW host OpenGL context.
void Render()
{
    var width = checked((int)canvas.Size.X);
    var height = checked((int)canvas.Size.Y);
    gl.Viewport(0, 0, width, height);
    gl.ClearColor(0.055f, 0.065f, 0.075f, 1f);
    gl.Clear(GlClearBufferMask.ColorBufferBit);
    renderer.Render(state, (float)totalSeconds, width, height);
    gl.ResetClearColor();
    gl.ResetViewport();
}

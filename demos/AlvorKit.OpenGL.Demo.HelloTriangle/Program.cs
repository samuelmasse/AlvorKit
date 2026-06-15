using AlvorKit.GLFW;
using AlvorKit.OpenGL;
using AlvorKit.OpenGL.Demo.HelloTriangle;

var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

var window = glfw.CreateWindow(800, 600, "AlvorKit HelloTriangle", default, default);
if (window == default)
{
    glfw.Terminate();
    throw new InvalidOperationException("Failed to create the GLFW window.");
}

glfw.MakeContextCurrent(window);

// Pace the loop with vsync: the buffer swap blocks until the next display refresh.
glfw.SwapInterval(1);

Gl gl = new GlBackend(glfw.GetProcAddress);
gl.GetString(GlStringName.Version, out var version);
gl.GetString(GlStringName.ShadingLanguageVersion, out var glsl);
Console.WriteLine($"OpenGL {version} (GLSL {glsl}) - press Escape or close the window to exit.");

using var triangle = HelloTriangle.Load(gl);

void RenderFrame(int width, int height)
{
    gl.Viewport(0, 0, width, height);
    gl.ClearColor(0.392f, 0.584f, 0.929f, 1f);
    gl.Clear(GlClearBufferMask.ColorBufferBit);
    triangle.Render();
    glfw.SwapBuffers(window);
}

// Repaint from the framebuffer-size callback so the triangle keeps drawing while the platform's modal
// resize loop holds the main thread. The binding roots the delegate on the Glfw instance.
glfw.SetFramebufferSizeCallback(window, (_, width, height) => RenderFrame(width, height));

while (!glfw.WindowShouldClose(window))
{
    glfw.PollEvents();
    if (glfw.GetKey(window, GlfwKey.Escape) == GlfwInputAction.Press)
        glfw.SetWindowShouldClose(window, true);

    glfw.GetFramebufferSize(window, out var width, out var height);
    RenderFrame(width, height);
}

glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

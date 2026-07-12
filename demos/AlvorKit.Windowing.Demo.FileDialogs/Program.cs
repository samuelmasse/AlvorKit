var filters = new FileDialogFilter[]
{
    new("Images", "png,jpg,jpeg,gif"),
    new("Text", "txt,md,json")
};

var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);
var window = glfw.CreateWindow(900, 520, "AlvorKit file dialogs", default, default);
if (window == default)
{
    glfw.Terminate();
    throw new InvalidOperationException("Failed to create the GLFW window.");
}

glfw.MakeContextCurrent(window);
var gl = new GlLayer(new GlBackend(glfw.GetProcAddress));
using var host = new AgentGlfwWindowHost(glfw, window, gl) { IsVSyncEnabled = true };
using var fileDialogHost = new GlfwFileDialogHost(glfw, window);
var dialogs = new FileDialogs(fileDialogHost);
var loop = new WindowLoop(host);
var keyboard = new Keyboard(loop);
var screen = new WindowScreen(loop)
{
    Title = "AlvorKit file dialogs",
    IsVisible = true
};

Console.WriteLine("O open file; M open files; S save file; F pick folder; P pick folders; Esc exits.");

loop.Update += Update;
loop.Render += Render;
if (args.Contains("--open", StringComparer.OrdinalIgnoreCase))
    Print("Open file", dialogs.OpenFile(filters));
loop.Run();

glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

// Opens each dialog only on its key-press edge and prints cancellation or selected paths.
void Update(double _)
{
    if (keyboard.IsKeyPressed(Keys.Escape))
        screen.Close();
    if (keyboard.IsKeyPressed(Keys.O))
        Print("Open file", dialogs.OpenFile(filters));
    if (keyboard.IsKeyPressed(Keys.M))
        PrintMany("Open files", dialogs.OpenFiles(filters));
    if (keyboard.IsKeyPressed(Keys.S))
        Print("Save file", dialogs.SaveFile(filters, defaultName: "untitled.txt"));
    if (keyboard.IsKeyPressed(Keys.F))
        Print("Pick folder", dialogs.PickFolder());
    if (keyboard.IsKeyPressed(Keys.P))
        PrintMany("Pick folders", dialogs.PickFolders());
}

// Keeps the GLFW swap loop visibly active behind the modal native dialog.
void Render()
{
    gl.ClearColor(0.06f, 0.09f, 0.14f, 1f);
    gl.Clear(GlClearBufferMask.ColorBufferBit);
    gl.ResetClearColor();
}

// Prints a single selection or cancellation.
void Print(string operation, string? path) =>
    Console.WriteLine(path is null ? $"{operation}: cancelled" : $"{operation}: {path}");

// Prints multiple selections or cancellation.
void PrintMany(string operation, string[]? paths)
{
    if (paths is null)
    {
        Console.WriteLine($"{operation}: cancelled");
        return;
    }

    Console.WriteLine($"{operation}: {paths.Length} selected");
    foreach (var path in paths)
        Console.WriteLine($"  {path}");
}

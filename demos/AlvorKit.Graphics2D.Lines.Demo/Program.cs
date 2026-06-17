const string WindowTitle = "AlvorKit Graphics2D line cases";
const int WindowWidth = 1120;
const int WindowHeight = 780;
const int Columns = 4;
const int Rows = 4;
const int DirectionCase = 0;
const int ThickCase = 2;
const int ClipHorizontalCase = 3;
const int ClipDiagonalCase = 4;
const int DotCount = 13;
const float CellPadding = 38f;
const float CaseLineHalfWidth = 9f;
const float CenterDotSize = 4f;
const float RailDotSize = 3f;
const float EndpointSize = 11f;
const float DigitScale = 1.6f;

var okColor = new Vector4(0.2f, 0.86f, 0.42f, 0.9f);
var guideColor = new Vector4(0.9f, 0.95f, 1f, 0.58f);
var railColor = new Vector4(0.74f, 0.82f, 0.9f, 0.55f);
var startColor = new Vector4(0.18f, 0.58f, 1f, 1f);
var endColor = new Vector4(0.95f, 0.98f, 1f, 1f);

(string Name, Vector2 Direction, Vector4 Color, string Description, int Mode)[] lineCases =
[
    ("horizontal right", new Vector2(1f, 0f), okColor, "axis-aligned line", DirectionCase),
    ("horizontal left", new Vector2(-1f, 0f), okColor, "axis-aligned line", DirectionCase),
    ("vertical down", new Vector2(0f, 1f), okColor, "axis-aligned line", DirectionCase),
    ("vertical up", new Vector2(0f, -1f), okColor, "axis-aligned line", DirectionCase),
    ("45 down-right", new Vector2(1f, 1f), okColor, "exact diagonal line", DirectionCase),
    ("45 up-left", new Vector2(-1f, -1f), okColor, "exact diagonal line", DirectionCase),
    ("45 up-right", new Vector2(1f, -1f), okColor, "exact diagonal line", DirectionCase),
    ("45 down-left", new Vector2(-1f, 1f), okColor, "exact diagonal line", DirectionCase),
    ("shallow positive", new Vector2(2f, 1f), okColor, "off-axis diagonal line", DirectionCase),
    ("steep positive", new Vector2(1f, 2f), okColor, "off-axis diagonal line", DirectionCase),
    ("shallow negative", new Vector2(2f, -1f), okColor, "off-axis diagonal line", DirectionCase),
    ("steep negative", new Vector2(1f, -2f), okColor, "off-axis diagonal line", DirectionCase),
    ("zero length", Vector2.Zero, okColor, "zero-length no-op", DirectionCase),
    ("thick horizontal", new Vector2(1f, 0f), okColor, "wide line sample", ThickCase),
    ("clipped horizontal", new Vector2(1f, 0f), okColor, "clip rectangle sample", ClipHorizontalCase),
    ("clipped diagonal", new Vector2(1f, 1f), okColor, "clip rectangle sample", ClipDiagonalCase)
];

var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);

var window = glfw.CreateWindow(WindowWidth, WindowHeight, WindowTitle, default, default);
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
Console.WriteLine("OpenGL {0} (GLSL {1}) - Esc exits.", version, glsl);
Console.WriteLine("Line case board is numbered left-to-right, top-to-bottom.");
Console.WriteLine("Dotted white center and pale rails show the expected line lane; colored fill is SpriteBatchWriter.DrawLine output.");
PrintCaseLegend();

var sprites = new SpriteBatch(gl);

// Draws one deterministic frame from the current framebuffer size.
void RenderFrame(int framebufferWidth, int framebufferHeight)
{
    if (framebufferWidth <= 0 || framebufferHeight <= 0)
        return;

    var canvasSize = new Vector2(framebufferWidth, framebufferHeight);

    gl.Viewport(0, 0, framebufferWidth, framebufferHeight);
    gl.ClearColor(0.055f, 0.065f, 0.08f, 1f);
    gl.Clear(GlClearBufferMask.ColorBufferBit);
    gl.Enable(GlEnableCap.Blend);
    gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha);

    sprites.Begin(canvasSize);
    DrawLineCaseBoard(canvasSize);
    sprites.End();

    gl.ResetBlendFunc();
    gl.Disable(GlEnableCap.Blend);
    gl.ResetClearColor();
    gl.ResetViewport();
    glfw.SwapBuffers(window);
}

// Draws the full line-case matrix, using independent dotted guides for the expected geometry.
void DrawLineCaseBoard(Vector2 canvasSize)
{
    var cellSize = canvasSize / new Vector2(Columns, Rows);

    for (var i = 0; i < lineCases.Length; i++)
    {
        var column = i % Columns;
        var row = i / Columns;
        var cellMin = new Vector2(column * cellSize.X, row * cellSize.Y);
        DrawLineCase(i + 1, lineCases[i], cellMin, cellSize);
    }
}

// Draws one numbered cell with background, expected rails, actual DrawLine output, and endpoint markers.
void DrawLineCase(int number, (string Name, Vector2 Direction, Vector4 Color, string Description, int Mode) lineCase, Vector2 cellMin, Vector2 cellSize)
{
    var panelColor = new Vector4(lineCase.Color.X * 0.08f, lineCase.Color.Y * 0.08f, lineCase.Color.Z * 0.08f, 0.55f);
    sprites.Writer.Draw(cellMin + new Vector2(8f, 8f), cellSize - new Vector2(16f, 16f), panelColor);
    sprites.Writer.Draw(cellMin + new Vector2(8f, 8f), new Vector2(cellSize.X - 16f, 5f), lineCase.Color);
    DrawCaseNumber(cellMin + new Vector2(22f, 24f), number, lineCase.Color);

    if (lineCase.Direction == Vector2.Zero)
    {
        DrawZeroLengthCase(cellMin, cellSize, lineCase.Color);
        return;
    }

    if (lineCase.Mode == ThickCase)
    {
        DrawThickLineCase(cellMin, cellSize, lineCase.Color);
        return;
    }

    if (lineCase.Mode is ClipHorizontalCase or ClipDiagonalCase)
    {
        DrawClipCase(cellMin, cellSize, lineCase.Color, lineCase.Mode == ClipDiagonalCase);
        return;
    }

    var direction = Vector2.Normalize(lineCase.Direction);
    var length = MathF.Min(cellSize.X, cellSize.Y) - (CellPadding * 2f);
    var center = cellMin + (cellSize * 0.5f) + new Vector2(0f, 14f);
    var start = center - (direction * length * 0.5f);
    var end = center + (direction * length * 0.5f);

    DrawExpectedLane(start, end, CaseLineHalfWidth);
    sprites.Writer.DrawLine(start, end, CaseLineHalfWidth, lineCase.Color);
    DrawDottedSegment(start, end, CenterDotSize, guideColor);
    DrawSquare(start, EndpointSize, startColor);
    DrawSquare(end, EndpointSize, endColor);
}

// Shows the zero-length no-op behavior with a center marker.
void DrawZeroLengthCase(Vector2 cellMin, Vector2 cellSize, Vector4 color)
{
    var center = cellMin + (cellSize * 0.5f) + new Vector2(0f, 14f);
    var expectedSize = CaseLineHalfWidth * 2f;

    DrawSquare(center, expectedSize, new Vector4(0.75f, 0.82f, 0.9f, 0.45f));
    sprites.Writer.DrawLine(center, center, CaseLineHalfWidth, color);
    DrawSquare(center, EndpointSize, startColor);
}

// Shows a deliberately wider horizontal line.
void DrawThickLineCase(Vector2 cellMin, Vector2 cellSize, Vector4 color)
{
    var center = cellMin + (cellSize * 0.5f) + new Vector2(0f, 14f);
    var length = MathF.Min(cellSize.X, cellSize.Y) - (CellPadding * 2f);
    var start = center - new Vector2(length * 0.5f, 0f);
    var end = center + new Vector2(length * 0.5f, 0f);

    DrawExpectedLane(start, end, CaseLineHalfWidth);
    sprites.Writer.DrawLine(start, end, CaseLineHalfWidth, color);
    DrawDottedSegment(start, end, CenterDotSize, guideColor);
    DrawSquare(start, EndpointSize, startColor);
    DrawSquare(end, EndpointSize, endColor);
}

// Shows line clipping against a rectangular sprite-batch clip.
void DrawClipCase(Vector2 cellMin, Vector2 cellSize, Vector4 color, bool diagonal)
{
    var center = cellMin + (cellSize * 0.5f) + new Vector2(0f, 14f);
    var length = MathF.Min(cellSize.X, cellSize.Y) - (CellPadding * 2f);
    var fullDirection = diagonal ? Vector2.Normalize(new Vector2(1f, 1f)) : new Vector2(1f, 0f);
    var fullStart = center - (fullDirection * length * 0.62f);
    var fullEnd = center + (fullDirection * length * 0.62f);
    var clipMin = center - new Vector2(length * 0.22f, length * 0.22f);
    var clipMax = center + new Vector2(length * 0.22f, length * 0.22f);
    var clippedStart = center - (fullDirection * length * 0.22f);
    var clippedEnd = center + (fullDirection * length * 0.22f);

    DrawRectOutline(clipMin, clipMax - clipMin, new Vector4(0.85f, 0.9f, 1f, 0.65f));
    DrawExpectedLane(clippedStart, clippedEnd, CaseLineHalfWidth);

    sprites.Writer.Clip = new SpriteBatchClip(clipMin, clipMax);
    sprites.Writer.DrawLine(fullStart, fullEnd, CaseLineHalfWidth, color);
    sprites.Writer.Clip = null;

    DrawDottedSegment(clippedStart, clippedEnd, CenterDotSize, guideColor);
    DrawSquare(fullStart, EndpointSize, startColor);
    DrawSquare(fullEnd, EndpointSize, endColor);
}

// Draws the expected edge rails for a line without using DrawLine.
void DrawExpectedLane(Vector2 start, Vector2 end, float halfWidth)
{
    var normal = CorrectLineNormal(Vector2.Normalize(end - start));
    DrawDottedSegment(start + (normal * halfWidth), end + (normal * halfWidth), RailDotSize, railColor);
    DrawDottedSegment(start - (normal * halfWidth), end - (normal * halfWidth), RailDotSize, railColor);
}

// Draws a rectangle outline with plain sprite rectangles, avoiding DrawLine for the reference shape.
void DrawRectOutline(Vector2 position, Vector2 size, Vector4 color)
{
    const float thickness = 3f;

    sprites.Writer.Draw(position, new Vector2(size.X, thickness), color);
    sprites.Writer.Draw(position + new Vector2(0f, size.Y - thickness), new Vector2(size.X, thickness), color);
    sprites.Writer.Draw(position, new Vector2(thickness, size.Y), color);
    sprites.Writer.Draw(position + new Vector2(size.X - thickness, 0f), new Vector2(thickness, size.Y), color);
}

// Draws evenly spaced square dots along a segment without using DrawLine.
void DrawDottedSegment(Vector2 start, Vector2 end, float size, Vector4 color)
{
    var delta = end - start;

    for (var i = 0; i < DotCount; i++)
    {
        var t = i / (float)(DotCount - 1);
        DrawSquare(start + (delta * t), size, color);
    }
}

// Draws a square centered on a point.
void DrawSquare(Vector2 center, float size, Vector4 color) =>
    sprites.Writer.Draw(center - new Vector2(size * 0.5f, size * 0.5f), new Vector2(size, size), color);

// Draws a compact seven-segment case number so screenshots can be mapped back to console legend rows.
void DrawCaseNumber(Vector2 position, int number, Vector4 color)
{
    if (number >= 10)
    {
        DrawDigit(position, 1, color);
        DrawDigit(position + new Vector2(15f * DigitScale, 0f), number - 10, color);
        return;
    }

    DrawDigit(position, number, color);
}

// Draws one seven-segment digit with SpriteBatch rectangles.
void DrawDigit(Vector2 position, int digit, Vector4 color)
{
    var mask = digit switch
    {
        0 => 0b0111111,
        1 => 0b0000110,
        2 => 0b1011011,
        3 => 0b1001111,
        4 => 0b1100110,
        5 => 0b1101101,
        6 => 0b1111101,
        7 => 0b0000111,
        8 => 0b1111111,
        _ => 0b1101111
    };

    for (var segment = 0; segment < 7; segment++)
    {
        if ((mask & (1 << segment)) != 0)
            DrawDigitSegment(position, segment, color);
    }
}

// Draws one seven-segment digit bar.
void DrawDigitSegment(Vector2 position, int segment, Vector4 color)
{
    var stroke = 2.5f * DigitScale;
    var width = 10f * DigitScale;
    var height = 18f * DigitScale;
    var half = height * 0.5f;

    switch (segment)
    {
        case 0:
            sprites.Writer.Draw(position, new Vector2(width, stroke), color);
            break;
        case 1:
            sprites.Writer.Draw(position + new Vector2(width - stroke, 0f), new Vector2(stroke, half), color);
            break;
        case 2:
            sprites.Writer.Draw(position + new Vector2(width - stroke, half), new Vector2(stroke, half), color);
            break;
        case 3:
            sprites.Writer.Draw(position + new Vector2(0f, height - stroke), new Vector2(width, stroke), color);
            break;
        case 4:
            sprites.Writer.Draw(position + new Vector2(0f, half), new Vector2(stroke, half), color);
            break;
        case 5:
            sprites.Writer.Draw(position, new Vector2(stroke, half), color);
            break;
        default:
            sprites.Writer.Draw(position + new Vector2(0f, half - (stroke * 0.5f)), new Vector2(width, stroke), color);
            break;
    }
}

// Prints the exact cell ordering for the visual board.
void PrintCaseLegend()
{
    for (var i = 0; i < lineCases.Length; i++)
        Console.WriteLine("{0,2}. {1,-18} {2}", i + 1, lineCases[i].Name + ":", lineCases[i].Description);
}

// Computes the perpendicular normal the DrawLine implementation should use for a screen-space direction.
static Vector2 CorrectLineNormal(Vector2 direction) => Vector2.Normalize(new Vector2(-direction.Y, direction.X));

// The callback repaints while the platform is inside modal resize loops.
glfw.SetFramebufferSizeCallback(window, (_, width, height) => RenderFrame(width, height));

while (!glfw.WindowShouldClose(window))
{
    glfw.PollEvents();

    if (glfw.GetKey(window, GlfwKey.Escape) == GlfwInputAction.Press)
        glfw.SetWindowShouldClose(window, true);

    glfw.GetFramebufferSize(window, out var width, out var height);
    RenderFrame(width, height);
}

sprites.Dispose();
gl.Dispose();
glfw.DestroyWindow(window);
glfw.Terminate();
return 0;

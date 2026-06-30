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

Vec4 okColor = (0.2f, 0.86f, 0.42f, 0.9f);
Vec4 guideColor = (0.9f, 0.95f, 1f, 0.58f);
Vec4 railColor = (0.74f, 0.82f, 0.9f, 0.55f);
Vec4 startColor = (0.18f, 0.58f, 1f, 1f);
Vec4 endColor = (0.95f, 0.98f, 1f, 1f);
(string Name, Vec2 Direction, Vec4 Color, string Description, int Mode)[] lineCases =
[
    ("horizontal right", (1f, 0f), okColor, "axis-aligned line", DirectionCase),
    ("horizontal left", (-1f, 0f), okColor, "axis-aligned line", DirectionCase),
    ("vertical down", (0f, 1f), okColor, "axis-aligned line", DirectionCase),
    ("vertical up", (0f, -1f), okColor, "axis-aligned line", DirectionCase),
    ("45 down-right", (1f, 1f), okColor, "exact diagonal line", DirectionCase),
    ("45 up-left", (-1f, -1f), okColor, "exact diagonal line", DirectionCase),
    ("45 up-right", (1f, -1f), okColor, "exact diagonal line", DirectionCase),
    ("45 down-left", (-1f, 1f), okColor, "exact diagonal line", DirectionCase),
    ("shallow positive", (2f, 1f), okColor, "off-axis diagonal line", DirectionCase),
    ("steep positive", (1f, 2f), okColor, "off-axis diagonal line", DirectionCase),
    ("shallow negative", (2f, -1f), okColor, "off-axis diagonal line", DirectionCase),
    ("steep negative", (1f, -2f), okColor, "off-axis diagonal line", DirectionCase),
    ("zero length", Vec2.Zero, okColor, "zero-length no-op", DirectionCase),
    ("thick horizontal", (1f, 0f), okColor, "wide line sample", ThickCase),
    ("clipped horizontal", (1f, 0f), okColor, "clip rectangle sample", ClipHorizontalCase),
    ("clipped diagonal", (1f, 1f), okColor, "clip rectangle sample", ClipDiagonalCase)
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

    Vec2 canvasSize = (framebufferWidth, framebufferHeight);
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
void DrawLineCaseBoard(Vec2 canvasSize)
{
    Vec2 boardSize = (Columns, Rows);
    var cellSize = canvasSize / boardSize;

    for (var i = 0; i < lineCases.Length; i++)
    {
        var column = i % Columns;
        var row = i / Columns;
        Vec2 cellMin = (column * cellSize.X, row * cellSize.Y);
        DrawLineCase(i + 1, lineCases[i], cellMin, cellSize);
    }
}

// Draws one numbered cell with background, expected rails, actual DrawLine output, and endpoint markers.
void DrawLineCase(int number, (string Name, Vec2 Direction, Vec4 Color, string Description, int Mode) lineCase, Vec2 cellMin, Vec2 cellSize)
{
    Vec2 cellInset = (8f, 8f);
    Vec2 numberOffset = (22f, 24f);
    Vec4 panelColor = (lineCase.Color.X * 0.08f, lineCase.Color.Y * 0.08f, lineCase.Color.Z * 0.08f, 0.55f);
    sprites.Writer.Draw(cellMin + cellInset, cellSize - (cellInset * 2f), panelColor);
    sprites.Writer.Draw(cellMin + cellInset, (cellSize.X - 16f, 5f), lineCase.Color);
    DrawCaseNumber(cellMin + numberOffset, number, lineCase.Color);

    if (lineCase.Direction == Vec2.Zero)
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

    var direction = Vec2.Normalize(lineCase.Direction);
    var length = MathF.Min(cellSize.X, cellSize.Y) - (CellPadding * 2f);
    var center = CaseCenter(cellMin, cellSize);
    var start = center - (direction * length * 0.5f);
    var end = center + (direction * length * 0.5f);

    DrawExpectedLane(start, end, CaseLineHalfWidth);
    sprites.Writer.DrawLine(start, end, CaseLineHalfWidth, lineCase.Color);
    DrawDottedSegment(start, end, CenterDotSize, guideColor);
    DrawSquare(start, EndpointSize, startColor);
    DrawSquare(end, EndpointSize, endColor);
}

// Shows the zero-length no-op behavior with a center marker.
void DrawZeroLengthCase(Vec2 cellMin, Vec2 cellSize, Vec4 color)
{
    var center = CaseCenter(cellMin, cellSize);
    var expectedSize = CaseLineHalfWidth * 2f;

    DrawSquare(center, expectedSize, (0.75f, 0.82f, 0.9f, 0.45f));
    sprites.Writer.DrawLine(center, center, CaseLineHalfWidth, color);
    DrawSquare(center, EndpointSize, startColor);
}

// Shows a deliberately wider horizontal line.
void DrawThickLineCase(Vec2 cellMin, Vec2 cellSize, Vec4 color)
{
    var center = CaseCenter(cellMin, cellSize);
    var length = MathF.Min(cellSize.X, cellSize.Y) - (CellPadding * 2f);
    Vec2 halfLength = (length * 0.5f, 0f);
    var start = center - halfLength;
    var end = center + halfLength;

    DrawExpectedLane(start, end, CaseLineHalfWidth);
    sprites.Writer.DrawLine(start, end, CaseLineHalfWidth, color);
    DrawDottedSegment(start, end, CenterDotSize, guideColor);
    DrawSquare(start, EndpointSize, startColor);
    DrawSquare(end, EndpointSize, endColor);
}

// Shows line clipping against a rectangular sprite-batch clip.
void DrawClipCase(Vec2 cellMin, Vec2 cellSize, Vec4 color, bool diagonal)
{
    var center = CaseCenter(cellMin, cellSize);
    var length = MathF.Min(cellSize.X, cellSize.Y) - (CellPadding * 2f);
    var fullDirection = diagonal ? Vec2.Normalize((1f, 1f)) : Vec2.UnitX;
    var fullStart = center - (fullDirection * length * 0.62f);
    var fullEnd = center + (fullDirection * length * 0.62f);
    Vec2 clipExtent = (length * 0.22f, length * 0.22f);
    var clipMin = center - clipExtent;
    var clipMax = center + clipExtent;
    var clippedStart = center - (fullDirection * length * 0.22f);
    var clippedEnd = center + (fullDirection * length * 0.22f);

    DrawRectOutline(clipMin, clipMax - clipMin, (0.85f, 0.9f, 1f, 0.65f));
    DrawExpectedLane(clippedStart, clippedEnd, CaseLineHalfWidth);

    sprites.Writer.Clip = new SpriteBatchClip(clipMin, clipMax);
    sprites.Writer.DrawLine(fullStart, fullEnd, CaseLineHalfWidth, color);
    sprites.Writer.Clip = null;

    DrawDottedSegment(clippedStart, clippedEnd, CenterDotSize, guideColor);
    DrawSquare(fullStart, EndpointSize, startColor);
    DrawSquare(fullEnd, EndpointSize, endColor);
}

// Draws the expected edge rails for a line without using DrawLine.
void DrawExpectedLane(Vec2 start, Vec2 end, float halfWidth)
{
    var normal = CorrectLineNormal(Vec2.Normalize(end - start));
    DrawDottedSegment(start + (normal * halfWidth), end + (normal * halfWidth), RailDotSize, railColor);
    DrawDottedSegment(start - (normal * halfWidth), end - (normal * halfWidth), RailDotSize, railColor);
}

// Draws a rectangle outline with plain sprite rectangles, avoiding DrawLine for the reference shape.
void DrawRectOutline(Vec2 position, Vec2 size, Vec4 color)
{
    const float thickness = 3f;

    Vec2 bottomOffset = (0f, size.Y - thickness);
    Vec2 rightOffset = (size.X - thickness, 0f);
    sprites.Writer.Draw(position, (size.X, thickness), color);
    sprites.Writer.Draw(position + bottomOffset, (size.X, thickness), color);
    sprites.Writer.Draw(position, (thickness, size.Y), color);
    sprites.Writer.Draw(position + rightOffset, (thickness, size.Y), color);
}

// Draws evenly spaced square dots along a segment without using DrawLine.
void DrawDottedSegment(Vec2 start, Vec2 end, float size, Vec4 color)
{
    var delta = end - start;

    for (var i = 0; i < DotCount; i++)
    {
        var t = i / (float)(DotCount - 1);
        DrawSquare(start + (delta * t), size, color);
    }
}

// Draws a square centered on a point.
void DrawSquare(Vec2 center, float size, Vec4 color) =>
    sprites.Writer.Draw(center - HalfSize(size), (size, size), color);

// Draws a compact seven-segment case number so screenshots can be mapped back to console legend rows.
void DrawCaseNumber(Vec2 position, int number, Vec4 color)
{
    if (number >= 10)
    {
        Vec2 secondDigitOffset = (15f * DigitScale, 0f);
        DrawDigit(position, 1, color);
        DrawDigit(position + secondDigitOffset, number - 10, color);
        return;
    }

    DrawDigit(position, number, color);
}

// Draws one seven-segment digit with SpriteBatch rectangles.
void DrawDigit(Vec2 position, int digit, Vec4 color)
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
void DrawDigitSegment(Vec2 position, int segment, Vec4 color)
{
    var stroke = 2.5f * DigitScale;
    var width = 10f * DigitScale;
    var height = 18f * DigitScale;
    var half = height * 0.5f;

    switch (segment)
    {
        case 0:
            sprites.Writer.Draw(position, (width, stroke), color);
            break;
        case 1:
            DrawSegment((width - stroke, 0f), (stroke, half));
            break;
        case 2:
            DrawSegment((width - stroke, half), (stroke, half));
            break;
        case 3:
            DrawSegment((0f, height - stroke), (width, stroke));
            break;
        case 4:
            DrawSegment((0f, half), (stroke, half));
            break;
        case 5:
            sprites.Writer.Draw(position, (stroke, half), color);
            break;
        default:
            DrawSegment((0f, half - (stroke * 0.5f)), (width, stroke));
            break;
    }

    void DrawSegment(Vec2 offset, Vec2 size) => sprites.Writer.Draw(position + offset, size, color);
}

// Returns the common center used by every numbered line-case cell.
static Vec2 CaseCenter(Vec2 cellMin, Vec2 cellSize)
{
    Vec2 centerOffset = (0f, 14f);
    return cellMin + (cellSize * 0.5f) + centerOffset;
}

// Returns a square half-size vector for centering a drawn marker.
static Vec2 HalfSize(float size) => (size * 0.5f, size * 0.5f);

// Prints the exact cell ordering for the visual board.
void PrintCaseLegend()
{
    for (var i = 0; i < lineCases.Length; i++)
        Console.WriteLine("{0,2}. {1,-18} {2}", i + 1, lineCases[i].Name + ":", lineCases[i].Description);
}

// Computes the perpendicular normal the DrawLine implementation should use for a screen-space direction.
static Vec2 CorrectLineNormal(Vec2 direction) => Vec2.Normalize((-direction.Y, direction.X));

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

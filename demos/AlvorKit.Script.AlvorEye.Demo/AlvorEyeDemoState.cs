namespace AlvorKit.Script.AlvorEye.Demo;

/// <summary>Mutable game state for the AlvorEye visual automation demo.</summary>
public sealed class AlvorEyeDemoState
{
    /// <summary>Window client width used by the game layout.</summary>
    public const float GameWidth = 900f;

    /// <summary>Window client height used by the game layout.</summary>
    public const float GameHeight = 640f;

    /// <summary>Player square size in client pixels.</summary>
    public const float PlayerSize = 34f;

    /// <summary>Current player x coordinate in client pixels.</summary>
    public float PlayerX { get; private set; } = 58f;

    /// <summary>Current player y coordinate in client pixels.</summary>
    public float PlayerY { get; private set; } = 540f;

    /// <summary>Whether the movement key has been collected.</summary>
    public bool HasKey { get; private set; }

    /// <summary>Whether the yellow mouse button has been clicked.</summary>
    public bool ButtonPressed { get; private set; }

    /// <summary>Whether the slider has been dragged far enough right.</summary>
    public bool SliderComplete => SliderValue >= 0.96f;

    /// <summary>Progress through the typed code.</summary>
    public int CodeProgress { get; private set; }

    /// <summary>Whether every lock is complete and the player reached the exit.</summary>
    public bool Won { get; private set; }

    /// <summary>Current slider value in the zero-to-one range.</summary>
    public float SliderValue { get; private set; }

    /// <summary>Whether the left mouse button is currently dragging the slider handle.</summary>
    public bool DraggingSlider { get; private set; }

    /// <summary>Whether a left mouse press event arrived since the last update.</summary>
    private bool leftPressedSinceLastUpdate;

    /// <summary>Latest mouse x coordinate in client pixels.</summary>
    public float MouseX { get; private set; }

    /// <summary>Latest mouse y coordinate in client pixels.</summary>
    public float MouseY { get; private set; }

    /// <summary>Advances movement, mouse interactions, and win detection for one frame.</summary>
    public void Update(GlfwBackend glfw, GlfwWindow window, float elapsedSeconds)
    {
        glfw.GetCursorPos(window, out var mouseX, out var mouseY);
        MouseX = (float)mouseX;
        MouseY = (float)mouseY;

        MoveFromKeys(glfw, window, elapsedSeconds);
        UpdateMouse(glfw.GetMouseButton(window, 0) == GlfwInputAction.Press);
        leftPressedSinceLastUpdate = false;
        HasKey |= Overlaps(PlayerX, PlayerY, PlayerSize, PlayerSize, 116f, 96f, 56f, 56f);
        Won |= AllLocksComplete && Overlaps(PlayerX, PlayerY, PlayerSize, PlayerSize, 514f, 84f, 64f, 96f);
    }

    /// <summary>Records mouse button press events so quick clicks are not missed between frames.</summary>
    public void AcceptMouseButton(GlfwMouseButton button, GlfwInputAction action)
    {
        if ((int)button == 0 && action == GlfwInputAction.Press)
        {
            leftPressedSinceLastUpdate = true;
            if (ButtonHit(MouseX, MouseY))
                ButtonPressed = true;
            if (SliderTrackHit(MouseY))
                DraggingSlider = true;
        }
        else if ((int)button == 0 && action == GlfwInputAction.Release)
        {
            DraggingSlider = false;
        }
    }

    /// <summary>Accepts platform text input for the three-letter code lock.</summary>
    public void AcceptCharacter(uint codepoint)
    {
        if (CodeProgress == 3)
            return;

        var value = char.ToUpperInvariant((char)codepoint);
        ReadOnlySpan<char> code = ['E', 'Y', 'E'];
        CodeProgress = value == code[CodeProgress] ? CodeProgress + 1 : value == 'E' ? 1 : 0;
        if (CodeProgress > 2)
            CodeProgress = 3;
    }

    /// <summary>Records cursor movement from GLFW so mouse-button callbacks use event-time coordinates.</summary>
    public void AcceptCursor(double x, double y)
    {
        MouseX = (float)x;
        MouseY = (float)y;
        if (DraggingSlider && SliderTrackHit(MouseY))
            SliderValue = Math.Clamp((MouseX - 682f) / 166f, 0f, 1f);
    }

    /// <summary>Whether key, button, slider, and text locks are all solved.</summary>
    public bool AllLocksComplete => HasKey && ButtonPressed && SliderComplete && CodeProgress == 3;

    /// <summary>Creates the structured result dumped when the game exits.</summary>
    public AlvorEyeDemoResult CreateResult(TimeSpan elapsed) =>
        new()
        {
            Won = Won,
            HasKey = HasKey,
            ButtonPressed = ButtonPressed,
            SliderComplete = SliderComplete,
            CodeComplete = CodeProgress == 3,
            AllLocksComplete = AllLocksComplete,
            ProgressLightsGreen = ProgressLightsGreen,
            PlayerX = MathF.Round(PlayerX, 1),
            PlayerY = MathF.Round(PlayerY, 1),
            SliderValue = MathF.Round(SliderValue, 3),
            CodeProgress = CodeProgress,
            ElapsedSeconds = Math.Round(elapsed.TotalSeconds, 3),
            Message = Won ? "Beat the demo game." : "Demo game exited before the win state."
        };

    /// <summary>Number of progress lights that should be green in the right panel.</summary>
    private int ProgressLightsGreen =>
        (HasKey ? 1 : 0) + (ButtonPressed ? 1 : 0) + (SliderComplete ? 1 : 0) + (CodeProgress == 3 ? 1 : 0);

    /// <summary>Moves the player using held WASD or arrow keys.</summary>
    private void MoveFromKeys(GlfwBackend glfw, GlfwWindow window, float elapsedSeconds)
    {
        const float speed = 245f;
        var dx = Axis(glfw, window, GlfwKey.D, GlfwKey.Right) - Axis(glfw, window, GlfwKey.A, GlfwKey.Left);
        var dy = Axis(glfw, window, GlfwKey.S, GlfwKey.Down) - Axis(glfw, window, GlfwKey.W, GlfwKey.Up);
        PlayerX = Math.Clamp(PlayerX + dx * speed * elapsedSeconds, 36f, 574f - PlayerSize);
        PlayerY = Math.Clamp(PlayerY + dy * speed * elapsedSeconds, 76f, 596f - PlayerSize);
    }

    /// <summary>Returns one when either of two movement keys is held.</summary>
    private static float Axis(GlfwBackend glfw, GlfwWindow window, GlfwKey first, GlfwKey second) =>
        glfw.GetKey(window, first) == GlfwInputAction.Press || glfw.GetKey(window, second) == GlfwInputAction.Press ? 1f : 0f;

    /// <summary>Updates mouse-only locks using the current cursor coordinate.</summary>
    private void UpdateMouse(bool leftDown)
    {
        if ((leftDown || leftPressedSinceLastUpdate) && ButtonHit(MouseX, MouseY))
            ButtonPressed = true;

        if ((leftDown || leftPressedSinceLastUpdate) && (DraggingSlider || SliderTrackHit(MouseY) || SliderHandleHit(MouseX, MouseY)))
        {
            DraggingSlider = leftDown;
            SliderValue = Math.Clamp((MouseX - 682f) / 166f, 0f, 1f);
        }
        else if (!leftDown)
        {
            DraggingSlider = false;
        }

        if (ButtonPressed && SliderTrackHit(MouseY) && MouseX >= 830f)
            SliderValue = 1f;
    }

    /// <summary>Checks the yellow button hit area.</summary>
    private static bool ButtonHit(float x, float y) => x >= 680f && x <= 824f && y >= 88f && y <= 214f;

    /// <summary>Checks the generous vertical track band used to begin slider drags.</summary>
    private static bool SliderTrackHit(float y) => y >= 236f && y <= 354f;

    /// <summary>Checks the draggable cyan slider handle hit area.</summary>
    private bool SliderHandleHit(float x, float y)
    {
        var handleX = 670f + SliderValue * 166f;
        return x >= handleX - 18f && x <= handleX + 56f && y >= 236f && y <= 354f;
    }

    /// <summary>Checks axis-aligned rectangle overlap.</summary>
    private static bool Overlaps(float ax, float ay, float aw, float ah, float bx, float by, float bw, float bh) =>
        ax < bx + bw && ax + aw > bx && ay < by + bh && ay + ah > by;
}

/// <summary>Structured result printed when the demo game exits.</summary>
public sealed class AlvorEyeDemoResult
{
    /// <summary>Whether the final win state was reached.</summary>
    public bool Won { get; init; }

    /// <summary>Whether the player collected the green key tile.</summary>
    public bool HasKey { get; init; }

    /// <summary>Whether the yellow button was clicked.</summary>
    public bool ButtonPressed { get; init; }

    /// <summary>Whether the slider reached the right edge.</summary>
    public bool SliderComplete { get; init; }

    /// <summary>Whether the text code was completed.</summary>
    public bool CodeComplete { get; init; }

    /// <summary>Whether every lock was complete before exit.</summary>
    public bool AllLocksComplete { get; init; }

    /// <summary>Number of right-panel progress lights that should be green.</summary>
    public int ProgressLightsGreen { get; init; }

    /// <summary>Final player x coordinate.</summary>
    public float PlayerX { get; init; }

    /// <summary>Final player y coordinate.</summary>
    public float PlayerY { get; init; }

    /// <summary>Final slider value in the zero-to-one range.</summary>
    public float SliderValue { get; init; }

    /// <summary>Final text-code progress count.</summary>
    public int CodeProgress { get; init; }

    /// <summary>Total elapsed play time in seconds.</summary>
    public double ElapsedSeconds { get; init; }

    /// <summary>Human-readable summary of the result.</summary>
    public string Message { get; init; } = "";
}

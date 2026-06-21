namespace AlvorKit.Windowing;

/// <summary>Tracks mouse button and wheel state for a window loop.</summary>
internal sealed class WindowMouse
{
    private readonly IWindowHost window;
    private Vec2 wheel;
    private bool[] down;
    private bool[] previous;

    /// <summary>Creates a mouse tracker from host mouse events.</summary>
    internal WindowMouse(IWindowHost window)
    {
        this.window = window;
        var buttons = (int)MouseButton.Last + 1;
        down = new bool[buttons];
        previous = new bool[buttons];
        window.MouseDown += OnMouseDown;
        window.MouseUp += OnMouseUp;
        window.MouseWheel += OnMouseWheel;
    }

    /// <summary>Gets the mouse wheel offset for the current tick.</summary>
    internal Vec2 Wheel => wheel;

    /// <summary>Gets or sets the cursor capture and visibility mode.</summary>
    internal CursorMode CursorMode
    {
        get => window.CursorMode;
        set => window.CursorMode = value;
    }

    /// <summary>Advances button transition state by one tick.</summary>
    internal void Tick()
    {
        (down, previous) = (previous, down);
        Array.Copy(previous, down, down.Length);
        wheel = default;
    }

    /// <summary>Returns whether a button is currently down.</summary>
    internal bool IsButtonDown(MouseButton button) => Down(button);

    /// <summary>Returns whether a button is currently up.</summary>
    internal bool IsButtonUp(MouseButton button) => !Down(button);

    /// <summary>Returns whether a button transitioned down this tick.</summary>
    internal bool IsButtonPressed(MouseButton button) => !Previous(button) && Down(button);

    private void OnMouseDown(WindowMouseButtonEvent e) => Down(e.Button) = true;

    private void OnMouseUp(WindowMouseButtonEvent e) => Down(e.Button) = false;

    private void OnMouseWheel(WindowMouseWheelEvent e) => wheel = e.Offset;

    private ref bool Down(MouseButton button)
    {
        ValidateButton(button);
        return ref down[(int)button];
    }

    private ref bool Previous(MouseButton button)
    {
        ValidateButton(button);
        return ref previous[(int)button];
    }

    private void ValidateButton(MouseButton button)
    {
        var index = (int)button;
        if ((uint)index >= down.Length)
            throw new InvalidOperationException("Invalid mouse button.");
    }
}

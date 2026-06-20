namespace AlvorKit.Windowing;

/// <summary>Raises deterministic agent window events while updating owned simulated state.</summary>
internal sealed class AgentWindowEventDriver(
    AgentWindowState state,
    Action closing,
    Action<WindowFrameEvent> updateFrame,
    Action<WindowFrameEvent> renderFrame,
    Action<WindowKeyEvent> keyDown,
    Action<WindowKeyEvent> keyUp,
    Action<WindowMouseButtonEvent> mouseDown,
    Action<WindowMouseButtonEvent> mouseUp,
    Action<WindowMouseMoveEvent> mouseMove,
    Action<WindowMouseWheelEvent> mouseWheel,
    Action<WindowTextInputEvent> textInput,
    Action<WindowResizeEvent> resize,
    Action<WindowPositionEvent> move,
    Action<Vector2> setClientSize)
{
    /// <summary>Closes the simulated window and raises the closing event once.</summary>
    internal void Close() { if (state.TryClose()) closing(); }

    /// <summary>Injects a key press into the next exact agent-controlled frame.</summary>
    internal void PressKey(WindowKey key) => keyDown(new(key, false));

    /// <summary>Injects a key repeat into the next exact agent-controlled frame.</summary>
    internal void RepeatKey(WindowKey key) => keyDown(new(key, true));

    /// <summary>Injects a key release into the next exact agent-controlled frame.</summary>
    internal void ReleaseKey(WindowKey key) => keyUp(new(key, false));

    /// <summary>Injects a mouse button press into the next exact agent-controlled frame.</summary>
    internal void PressMouse(WindowMouseButton button) => mouseDown(new(button));

    /// <summary>Injects a mouse button release into the next exact agent-controlled frame.</summary>
    internal void ReleaseMouse(WindowMouseButton button) => mouseUp(new(button));

    /// <summary>Injects an absolute cursor move in simulated window coordinates.</summary>
    internal void MoveMouse(Vector2 position)
    {
        state.MousePosition = position;
        mouseMove(new(position));
    }

    /// <summary>Injects a relative cursor move in simulated window coordinates.</summary>
    internal void PanMouse(Vector2 delta) => MoveMouse(state.MousePosition + delta);

    /// <summary>Injects a mouse wheel offset into the next exact agent-controlled frame.</summary>
    internal void ScrollMouse(Vector2 offset) => mouseWheel(new(offset));

    /// <summary>Injects one Unicode scalar of text input.</summary>
    internal void EnterText(Rune rune) => textInput(new(rune));

    /// <summary>Injects all Unicode scalar values from a string as text input.</summary>
    internal void EnterText(string text)
    {
        foreach (var rune in text.EnumerateRunes())
            textInput(new(rune));
    }

    /// <summary>Changes focus state observed by the window loop.</summary>
    internal void SetFocus(bool isFocused) => state.IsFocused = isFocused;

    /// <summary>Injects a window resize event and updates the simulated drawable size.</summary>
    internal void ResizeWindow(Vector2 size)
    {
        setClientSize(size);
        resize(new(state.ClientSize));
    }

    /// <summary>Injects a simulated top-level window move event.</summary>
    internal void MoveWindow(Vector2 position) => move(new(position));

    /// <summary>Invokes exactly one logical update with the supplied delta.</summary>
    internal void Update(double deltaSeconds)
    {
        if (state.TryUpdate(deltaSeconds))
            updateFrame(new(deltaSeconds, state.Time));
    }

    /// <summary>Pans the mouse, then invokes exactly one logical update with the supplied delta.</summary>
    internal void Update(double deltaSeconds, Vector2 mouseDelta)
    {
        PanMouse(mouseDelta);
        Update(deltaSeconds);
    }

    /// <summary>Invokes exactly one render frame without advancing host time.</summary>
    internal void Render(double deltaSeconds = 0)
    {
        if (state.TryRender(deltaSeconds))
            renderFrame(new(deltaSeconds, state.Time));
    }

    /// <summary>Invokes exactly one update and one render using the same explicit delta.</summary>
    internal void Step(double deltaSeconds)
    {
        Update(deltaSeconds);
        Render(deltaSeconds);
    }

    /// <summary>Invokes a fixed number of updates with the same explicit delta.</summary>
    internal void Advance(int count, double deltaSeconds)
    {
        AgentWindowState.ValidateCount(count);
        for (var i = 0; i < count && !state.IsExiting; i++)
            Update(deltaSeconds);
    }

    /// <summary>Invokes fixed updates while applying the same cursor delta before each update.</summary>
    internal void Advance(int count, double deltaSeconds, Vector2 mouseDeltaPerUpdate)
    {
        AgentWindowState.ValidateCount(count);
        for (var i = 0; i < count && !state.IsExiting; i++)
            Update(deltaSeconds, mouseDeltaPerUpdate);
    }
}

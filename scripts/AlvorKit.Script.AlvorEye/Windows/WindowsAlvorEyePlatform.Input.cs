namespace AlvorKit.Script.AlvorEye;

internal sealed partial class WindowsAlvorEyePlatform
{
    /// <summary>SendInput keyboard event type.</summary>
    private const uint InputKeyboard = 1;

    /// <summary>SendInput mouse event type.</summary>
    private const uint InputMouse = 0;

    /// <summary>Keyboard key-up flag.</summary>
    private const uint KeyUp = 0x0002;

    /// <summary>Keyboard Unicode flag.</summary>
    private const uint KeyUnicode = 0x0004;

    /// <summary>Left mouse down flag.</summary>
    private const uint MouseLeftDown = 0x0002;

    /// <summary>Left mouse up flag.</summary>
    private const uint MouseLeftUp = 0x0004;

    /// <summary>Right mouse down flag.</summary>
    private const uint MouseRightDown = 0x0008;

    /// <summary>Right mouse up flag.</summary>
    private const uint MouseRightUp = 0x0010;

    /// <inheritdoc/>
    public void SendKey(TargetWindow window, string key, KeyInputMode mode)
    {
        WindowsNative.SetForegroundWindow(window.Handle);
        var vk = KeyCode(key);
        Span<WindowsInputNative.Input> inputs = stackalloc WindowsInputNative.Input[mode == KeyInputMode.Press ? 2 : 1];
        inputs[0] = KeyInput(vk, mode == KeyInputMode.Up);
        if (mode == KeyInputMode.Press)
            inputs[1] = KeyInput(vk, true);
        Send(inputs);
    }

    /// <inheritdoc/>
    public void SendText(TargetWindow window, string text)
    {
        WindowsNative.SetForegroundWindow(window.Handle);
        foreach (var rune in text)
        {
            Span<WindowsInputNative.Input> inputs = [UnicodeInput(rune, false), UnicodeInput(rune, true)];
            Send(inputs);
        }
    }

    /// <inheritdoc/>
    public void MoveMouse(TargetWindow window, int x, int y) => MoveCursor(window, x, y);

    /// <inheritdoc/>
    public void ClickMouse(TargetWindow window, int x, int y, string button)
    {
        MoveCursor(window, x, y);
        var (down, up) = MouseFlags(button);
        Span<WindowsInputNative.Input> inputs = [MouseInput(down), MouseInput(up)];
        Send(inputs);
    }

    /// <inheritdoc/>
    public void DragMouse(TargetWindow window, int x, int y, int toX, int toY, string button, TimeSpan duration)
    {
        MoveCursor(window, x, y);
        Send([MouseInput(MouseFlags(button).Down)]);
        Thread.Sleep(duration);
        MoveCursor(window, toX, toY);
        Send([MouseInput(MouseFlags(button).Up)]);
    }

    /// <summary>Sends a span of Win32 input events.</summary>
    private static void Send(ReadOnlySpan<WindowsInputNative.Input> inputs)
    {
        var sent = WindowsInputNative.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<WindowsInputNative.Input>());
        if (sent != inputs.Length)
            throw new InvalidOperationException("SendInput did not accept every input event.");
    }

    /// <summary>Creates one virtual-key input event.</summary>
    private static WindowsInputNative.Input KeyInput(ushort key, bool up) =>
        new() { Type = InputKeyboard, Data = new() { Keyboard = new() { Vk = key, Flags = up ? KeyUp : 0 } } };

    /// <summary>Creates one Unicode input event.</summary>
    private static WindowsInputNative.Input UnicodeInput(char value, bool up) =>
        new() { Type = InputKeyboard, Data = new() { Keyboard = new() { Scan = value, Flags = KeyUnicode | (up ? KeyUp : 0) } } };

    /// <summary>Creates one mouse input event.</summary>
    private static WindowsInputNative.Input MouseInput(uint flags) =>
        new() { Type = InputMouse, Data = new() { Mouse = new() { Flags = flags } } };

    /// <summary>Moves the cursor to a window-relative point.</summary>
    private static void MoveCursor(TargetWindow window, int x, int y)
    {
        var rect = ReadWindowRect(window);
        WindowsNative.SetCursorPos(rect.Left + x, rect.Top + y);
    }
}

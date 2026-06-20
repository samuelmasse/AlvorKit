namespace AlvorKit.Script.AlvorEye;

internal sealed partial class WindowsAlvorEyePlatform
{
    /// <summary>SendInput keyboard event type.</summary>
    private const uint InputKeyboard = 1;

    /// <summary>SendInput mouse event type.</summary>
    private const uint InputMouse = 0;

    /// <summary>Keyboard key-up flag.</summary>
    private const uint KeyUp = 0x0002;

    /// <summary>Keyboard extended-key flag required for arrows and other enhanced-keyboard keys.</summary>
    private const uint KeyExtended = 0x0001;

    /// <summary>Keyboard Unicode flag.</summary>
    private const uint KeyUnicode = 0x0004;

    /// <summary>Keyboard hardware scan-code flag.</summary>
    private const uint KeyScanCode = 0x0008;

    /// <summary>Default key hold duration for press-and-release gestures.</summary>
    private const int KeyPressMilliseconds = 50;

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
        var keyCode = KeyCode(key);
        if (mode == KeyInputMode.Press)
        {
            SendKeyInput(keyCode, false);
            Thread.Sleep(KeyPressMilliseconds);
            SendKeyInput(keyCode, true);
            return;
        }

        SendKeyInput(keyCode, mode == KeyInputMode.Up);
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

    /// <summary>Sends one key input event.</summary>
    private static void SendKeyInput(WindowsKeyCode key, bool up)
    {
        Span<WindowsInputNative.Input> inputs = [KeyInput(key, up)];
        Send(inputs);
    }

    /// <summary>Creates one virtual-key input event.</summary>
    internal static WindowsInputNative.Input KeyInput(WindowsKeyCode key, bool up)
    {
        var useScanCode = key.ScanCode != 0;
        var flags = (up ? KeyUp : 0) | (key.Extended ? KeyExtended : 0) | (useScanCode ? KeyScanCode : 0);
        return new()
        {
            Type = InputKeyboard,
            Data = new() { Keyboard = new() { Vk = useScanCode ? (ushort)0 : key.VirtualKey, Scan = key.ScanCode, Flags = flags } }
        };
    }

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

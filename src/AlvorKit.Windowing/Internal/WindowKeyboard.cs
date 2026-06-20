namespace AlvorKit.Windowing;

/// <summary>Tracks key down, press, and repeat state for a window loop.</summary>
internal sealed class WindowKeyboard
{
    private readonly bool[] repeated;
    private bool[] down;
    private bool[] previous;

    /// <summary>Creates a keyboard tracker from host key events.</summary>
    internal WindowKeyboard(IWindowHost window)
    {
        var keys = (int)WindowKey.Last + 1;
        down = new bool[keys];
        previous = new bool[keys];
        repeated = new bool[keys];
        window.KeyDown += OnKeyDown;
        window.KeyUp += OnKeyUp;
    }

    /// <summary>Advances key transition state by one tick.</summary>
    internal void Tick()
    {
        (down, previous) = (previous, down);
        Array.Copy(previous, down, down.Length);
        Array.Clear(repeated);
    }

    /// <summary>Returns whether the key is currently down.</summary>
    internal bool IsKeyDown(WindowKey key) => Down(key);

    /// <summary>Returns whether the key is currently up.</summary>
    internal bool IsKeyUp(WindowKey key) => !Down(key);

    /// <summary>Returns whether the key transitioned down this tick.</summary>
    internal bool IsKeyPressed(WindowKey key) => !Previous(key) && Down(key);

    /// <summary>Returns whether the key pressed or repeated this tick.</summary>
    internal bool IsKeyPressedRepeated(WindowKey key) => IsKeyPressed(key) || Repeated(key);

    private void OnKeyDown(WindowKeyEvent e)
    {
        if (e.Key == WindowKey.Unknown)
            return;

        Down(e.Key) = true;

        if (e.IsRepeat)
            repeated[(int)e.Key] = true;
    }

    private void OnKeyUp(WindowKeyEvent e)
    {
        if (e.Key != WindowKey.Unknown)
            Down(e.Key) = false;
    }

    private ref bool Down(WindowKey key)
    {
        ValidateKey(key);
        return ref down[(int)key];
    }

    private ref bool Previous(WindowKey key)
    {
        ValidateKey(key);
        return ref previous[(int)key];
    }

    private ref bool Repeated(WindowKey key)
    {
        ValidateKey(key);
        return ref repeated[(int)key];
    }

    private void ValidateKey(WindowKey key)
    {
        var index = (int)key;
        if ((uint)index >= down.Length)
            throw new InvalidOperationException("Invalid window key.");
    }
}

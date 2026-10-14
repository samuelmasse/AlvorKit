namespace AlvorKit;

/// <summary>Tracks key down, press, and repeat state for a window loop.</summary>
internal class WindowKeyboard
{
    /// <summary>Native repeat events pending consumption at the end of the tick.</summary>
    private readonly bool[] repeated;
    /// <summary>Current held state, independent of transition history.</summary>
    private readonly bool[] down;
    /// <summary>Press edges retained until Tick so a release in the same poll cannot erase a tap.</summary>
    private readonly bool[] pressed;

    /// <summary>Creates a keyboard tracker from host key events.</summary>
    internal WindowKeyboard(IWindowHost window)
    {
        var keys = (int)Keys.Last + 1;
        down = new bool[keys];
        pressed = new bool[keys];
        repeated = new bool[keys];
        window.KeyDown += OnKeyDown;
        window.KeyUp += OnKeyUp;
    }

    /// <summary>Advances key transition state by one tick.</summary>
    internal void Tick()
    {
        Array.Clear(pressed);
        Array.Clear(repeated);
    }

    /// <summary>Clears all held, previous, and repeat state at an exclusive-input boundary.</summary>
    internal void Reset()
    {
        Array.Clear(down);
        Array.Clear(pressed);
        Array.Clear(repeated);
    }

    /// <summary>Returns whether the key is currently down.</summary>
    internal bool IsKeyDown(Keys key) => Down(key);

    /// <summary>Returns whether the key is currently up.</summary>
    internal bool IsKeyUp(Keys key) => !Down(key);

    /// <summary>Returns whether the key transitioned down this tick, even if it was released before the next update.</summary>
    internal bool IsKeyPressed(Keys key) => Pressed(key);

    /// <summary>Returns whether the key pressed or repeated this tick.</summary>
    internal bool IsKeyPressedRepeated(Keys key) => IsKeyPressed(key) || Repeated(key);

    private void OnKeyDown(WindowKeyEvent e)
    {
        if (e.Key == Keys.Unknown)
            return;

        if (!Down(e.Key) && !e.IsRepeat)
            Pressed(e.Key) = true;

        Down(e.Key) = true;

        if (e.IsRepeat)
            repeated[(int)e.Key] = true;
    }

    private void OnKeyUp(WindowKeyEvent e)
    {
        if (e.Key != Keys.Unknown)
            Down(e.Key) = false;
    }

    private ref bool Down(Keys key)
    {
        ValidateKey(key);
        return ref down[(int)key];
    }

    /// <summary>Accesses the latched press independently of the current held state.</summary>
    private ref bool Pressed(Keys key)
    {
        ValidateKey(key);
        return ref pressed[(int)key];
    }

    private ref bool Repeated(Keys key)
    {
        ValidateKey(key);
        return ref repeated[(int)key];
    }

    private void ValidateKey(Keys key)
    {
        var index = (int)key;
        if ((uint)index >= down.Length)
            throw new InvalidOperationException("Invalid window key.");
    }
}

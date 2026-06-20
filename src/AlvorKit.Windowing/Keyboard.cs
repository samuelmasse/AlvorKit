namespace AlvorKit.Windowing;

/// <summary>Reads keyboard state and text input from a window loop.</summary>
public sealed class Keyboard(WindowLoop window)
{
    /// <summary>Gets text input runes entered since the previous tick.</summary>
    public IReadOnlyList<Rune> Text => window.Text.Runes;

    /// <summary>Gets or sets the host clipboard text.</summary>
    public string Clipboard
    {
        get => window.Text.Clipboard;
        set => window.Text.Clipboard = value;
    }

    /// <summary>Returns whether the key is currently down.</summary>
    public bool IsKeyDown(WindowKey key) => window.Keyboard.IsKeyDown(key);

    /// <summary>Returns whether the key is currently up.</summary>
    public bool IsKeyUp(WindowKey key) => window.Keyboard.IsKeyUp(key);

    /// <summary>Returns whether the key transitioned to down this tick.</summary>
    public bool IsKeyPressed(WindowKey key) => window.Keyboard.IsKeyPressed(key);

    /// <summary>Returns whether the key pressed or repeated this tick.</summary>
    public bool IsKeyPressedRepeated(WindowKey key) => window.Keyboard.IsKeyPressedRepeated(key);

    /// <summary>Advances keyboard and text state by one tick.</summary>
    public void Tick()
    {
        window.Keyboard.Tick();
        window.Text.Tick();
    }
}

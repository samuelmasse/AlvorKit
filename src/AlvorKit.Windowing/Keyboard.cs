namespace AlvorKit.Windowing;

/// <summary>Reads keyboard state and text input from a window loop.</summary>
public class Keyboard(WindowLoop window)
{
    /// <summary>Gets text input runes entered since the previous tick.</summary>
    public IReadOnlyList<Rune> Text => window.Text.Runes;

    /// <summary>Returns whether the key is currently down.</summary>
    public bool IsKeyDown(Keys key) => window.Keyboard.IsKeyDown(key);

    /// <summary>Returns whether the key is currently up.</summary>
    public bool IsKeyUp(Keys key) => window.Keyboard.IsKeyUp(key);

    /// <summary>Returns whether the key transitioned to down this tick.</summary>
    public bool IsKeyPressed(Keys key) => window.Keyboard.IsKeyPressed(key);

    /// <summary>Returns whether the key pressed or repeated this tick.</summary>
    public bool IsKeyPressedRepeated(Keys key) => window.Keyboard.IsKeyPressedRepeated(key);
}

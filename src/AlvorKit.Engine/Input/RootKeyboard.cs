namespace AlvorKit.Engine;

/// <summary>Root-scoped keyboard and text-input reader.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootKeyboard(WindowLoop window)
{
    private readonly Keyboard keyboard = new(window);

    /// <summary>Gets text input runes entered since the previous tick.</summary>
    public IReadOnlyList<Rune> Text => keyboard.Text;

    /// <summary>Returns whether a key is currently down.</summary>
    public bool IsKeyDown(Keys key) => keyboard.IsKeyDown(key);

    /// <summary>Returns whether a key is currently up.</summary>
    public bool IsKeyUp(Keys key) => keyboard.IsKeyUp(key);

    /// <summary>Returns whether a key transitioned to down this tick.</summary>
    public bool IsKeyPressed(Keys key) => keyboard.IsKeyPressed(key);

    /// <summary>Returns whether a key pressed or repeated this tick.</summary>
    public bool IsKeyPressedRepeated(Keys key) => keyboard.IsKeyPressedRepeated(key);
}

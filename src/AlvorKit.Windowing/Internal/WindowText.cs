namespace AlvorKit.Windowing;

/// <summary>Buffers text input and clipboard access for a window loop.</summary>
internal sealed class WindowText
{
    private readonly IWindowHost window;
    private readonly List<Rune> runes = [];

    /// <summary>Creates a text input buffer from host text events.</summary>
    internal WindowText(IWindowHost window)
    {
        this.window = window;
        window.TextInput += OnTextInput;
    }

    /// <summary>Gets runes entered during the current tick.</summary>
    internal IReadOnlyList<Rune> Runes => runes;

    /// <summary>Gets or sets host clipboard text.</summary>
    internal string Clipboard
    {
        get => window.Clipboard;
        set => window.Clipboard = value;
    }

    /// <summary>Clears text entered during the previous tick.</summary>
    internal void Tick() => runes.Clear();

    private void OnTextInput(WindowTextInputEvent e) => runes.Add(e.Rune);
}

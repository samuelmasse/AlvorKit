namespace AlvorKit.Windowing;

/// <summary>Tracks deterministic input values injected through the agent event driver.</summary>
internal sealed class AgentWindowInputState
{
    private readonly bool[] keys = new bool[(int)Keys.Last + 1];
    private readonly bool[] mouseButtons = new bool[(int)MouseButton.Last + 1];
    private readonly StringBuilder text = new();

    /// <summary>Gets held keyboard keys in enum order.</summary>
    internal IEnumerable<Keys> HeldKeys
    {
        get
        {
            for (var i = 0; i < keys.Length; i++)
            {
                if (keys[i])
                    yield return (Keys)i;
            }
        }
    }

    /// <summary>Gets held mouse buttons in enum order.</summary>
    internal IEnumerable<MouseButton> HeldMouseButtons
    {
        get
        {
            for (var i = 0; i < mouseButtons.Length; i++)
            {
                if (mouseButtons[i])
                    yield return (MouseButton)i;
            }
        }
    }

    /// <summary>Gets text queued since the last update frame.</summary>
    internal string PendingText => text.ToString();

    /// <summary>Records that a key is currently held.</summary>
    /// <param name="key">Key injected by the agent.</param>
    internal void PressKey(Keys key)
    {
        if (key == Keys.Unknown)
            return;
        keys[Index(key, keys.Length, nameof(key))] = true;
    }

    /// <summary>Records that a key is no longer held.</summary>
    /// <param name="key">Key injected by the agent.</param>
    internal void ReleaseKey(Keys key)
    {
        if (key == Keys.Unknown)
            return;
        keys[Index(key, keys.Length, nameof(key))] = false;
    }

    /// <summary>Records that a mouse button is currently held.</summary>
    /// <param name="button">Mouse button injected by the agent.</param>
    internal void PressMouse(MouseButton button) => mouseButtons[Index(button, mouseButtons.Length, nameof(button))] = true;

    /// <summary>Records that a mouse button is no longer held.</summary>
    /// <param name="button">Mouse button injected by the agent.</param>
    internal void ReleaseMouse(MouseButton button) => mouseButtons[Index(button, mouseButtons.Length, nameof(button))] = false;

    /// <summary>Appends text that is pending for the next update frame.</summary>
    /// <param name="rune">Unicode scalar injected by the agent.</param>
    internal void AddText(Rune rune) => text.Append(rune);

    /// <summary>Clears text after it has had a frame to be observed by consumers.</summary>
    internal void ClearPendingText() => text.Clear();

    private static int Index<T>(T value, int length, string paramName) where T : struct, Enum
    {
        var index = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        return index >= 0 && index < length
            ? index
            : throw new ArgumentOutOfRangeException(paramName, "Input enum value is outside the supported range.");
    }
}

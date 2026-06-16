namespace AlvorKit.Script.AlvorEye;

internal sealed partial class WindowsAlvorEyePlatform
{
    /// <summary>Maps common key names to Windows virtual-key codes.</summary>
    private static ushort KeyCode(string key)
    {
        if (key.Length == 1)
        {
            var value = char.ToUpperInvariant(key[0]);
            if (value is >= 'A' and <= 'Z' or >= '0' and <= '9')
                return value;
        }

        return key.ToLowerInvariant() switch
        {
            "space" => 0x20,
            "enter" => 0x0D,
            "escape" or "esc" => 0x1B,
            "tab" => 0x09,
            "left" => 0x25,
            "up" => 0x26,
            "right" => 0x27,
            "down" => 0x28,
            "shift" => 0x10,
            "ctrl" or "control" => 0x11,
            "alt" => 0x12,
            _ => throw new ArgumentException($"Unsupported key name '{key}'.")
        };
    }

    /// <summary>Maps mouse button names to down and up flags.</summary>
    private static (uint Down, uint Up) MouseFlags(string button) =>
        button.ToLowerInvariant() switch
        {
            "left" => (MouseLeftDown, MouseLeftUp),
            "right" => (MouseRightDown, MouseRightUp),
            _ => throw new ArgumentException($"Unsupported mouse button '{button}'.")
        };
}

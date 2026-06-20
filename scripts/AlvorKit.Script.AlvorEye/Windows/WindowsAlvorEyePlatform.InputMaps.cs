namespace AlvorKit.Script.AlvorEye;

internal sealed partial class WindowsAlvorEyePlatform
{
    /// <summary>Maps common key names to Windows virtual-key codes.</summary>
    internal static WindowsKeyCode KeyCode(string key)
    {
        if (key.Length == 1)
        {
            var value = char.ToUpperInvariant(key[0]);
            if (value is >= 'A' and <= 'Z' or >= '0' and <= '9')
                return StandardKey((ushort)value);
        }

        return key.ToLowerInvariant() switch
        {
            "space" => StandardKey(0x20),
            "enter" => StandardKey(0x0D),
            "escape" or "esc" => StandardKey(0x1B),
            "tab" => StandardKey(0x09),
            "left" => ExtendedKey(0x25, 0x4B),
            "up" => ExtendedKey(0x26, 0x48),
            "right" => ExtendedKey(0x27, 0x4D),
            "down" => ExtendedKey(0x28, 0x50),
            "shift" => StandardKey(0x10),
            "ctrl" or "control" => StandardKey(0x11),
            "alt" => StandardKey(0x12),
            _ => throw new ArgumentException($"Unsupported key name '{key}'.")
        };
    }

    /// <summary>Creates a non-extended virtual key mapping.</summary>
    private static WindowsKeyCode StandardKey(ushort virtualKey) => new(virtualKey, 0, false);

    /// <summary>Creates an extended key mapping for keys such as arrows that require the Win32 extended flag.</summary>
    private static WindowsKeyCode ExtendedKey(ushort virtualKey, ushort scanCode) => new(virtualKey, scanCode, true);

    /// <summary>Maps mouse button names to down and up flags.</summary>
    private static (uint Down, uint Up) MouseFlags(string button) =>
        button.ToLowerInvariant() switch
        {
            "left" => (MouseLeftDown, MouseLeftUp),
            "right" => (MouseRightDown, MouseRightUp),
            _ => throw new ArgumentException($"Unsupported mouse button '{button}'.")
        };
}

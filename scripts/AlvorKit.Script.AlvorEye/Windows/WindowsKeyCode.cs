namespace AlvorKit.Script.AlvorEye;

/// <summary>Describes the Windows key payload used by SendInput.</summary>
/// <param name="VirtualKey">The Win32 virtual-key code, used when <paramref name="ScanCode" /> is zero.</param>
/// <param name="ScanCode">The hardware scan code, or zero to inject the virtual key directly.</param>
/// <param name="Extended">Whether to set KEYEVENTF_EXTENDEDKEY when injecting the key.</param>
internal readonly record struct WindowsKeyCode(ushort VirtualKey, ushort ScanCode, bool Extended);

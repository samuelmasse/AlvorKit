namespace AlvorKit.Script.AlvorEye;

/// <summary>Win32 input injection imports and structures.</summary>
[ExcludeFromCodeCoverage]
internal static partial class WindowsInputNative
{
    /// <summary>One input event for SendInput.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Input
    {
        /// <summary>Input event type.</summary>
        public uint Type;

        /// <summary>Mouse, keyboard, or hardware payload.</summary>
        public InputUnion Data;
    }

    /// <summary>Union of input payload shapes.</summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        /// <summary>Mouse input payload.</summary>
        [FieldOffset(0)]
        public MouseInput Mouse;

        /// <summary>Keyboard input payload.</summary>
        [FieldOffset(0)]
        public KeyboardInput Keyboard;
    }

    /// <summary>Keyboard input payload.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        /// <summary>Virtual key code.</summary>
        public ushort Vk;

        /// <summary>Hardware scan code or Unicode character.</summary>
        public ushort Scan;

        /// <summary>Keyboard event flags.</summary>
        public uint Flags;

        /// <summary>Timestamp supplied by the system.</summary>
        public uint Time;

        /// <summary>Caller-defined extra info.</summary>
        public nint ExtraInfo;
    }

    /// <summary>Mouse input payload.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        /// <summary>Relative x movement.</summary>
        public int Dx;

        /// <summary>Relative y movement.</summary>
        public int Dy;

        /// <summary>Mouse data such as wheel delta.</summary>
        public uint MouseData;

        /// <summary>Mouse event flags.</summary>
        public uint Flags;

        /// <summary>Timestamp supplied by the system.</summary>
        public uint Time;

        /// <summary>Caller-defined extra info.</summary>
        public nint ExtraInfo;
    }

    /// <summary>Injects keyboard or mouse input.</summary>
    [LibraryImport("user32.dll")]
    public static partial uint SendInput(uint count, ReadOnlySpan<Input> inputs, int size);
}

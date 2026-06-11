namespace AlvorKit.RGFW;

/// <summary>
/// Maps the RGFW_event union: a 32-byte tagged union with the event type at
/// offset 0, the source window at offset 8, and the per-variant payload at
/// offset 16. Layout verified against MSVC sizeof/offsetof for RGFW 1.8.1.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct RgfwEvent
{
    [FieldOffset(0)] public RgfwEventType Type;
    [FieldOffset(8)] public nint Window;

    // RGFW_keyEvent
    [FieldOffset(16)] public RgfwKey Key;
    [FieldOffset(17)] public byte KeySym;
    [FieldOffset(18)] public byte KeyRepeat;
    [FieldOffset(19)] public byte KeyMod;

    // RGFW_mouseButtonEvent
    [FieldOffset(16)] public RgfwMouseButton Button;

    // RGFW_mouseScrollEvent
    [FieldOffset(16)] public float ScrollX;
    [FieldOffset(20)] public float ScrollY;

    // RGFW_mousePosEvent
    [FieldOffset(16)] public int MouseX;
    [FieldOffset(20)] public int MouseY;
    [FieldOffset(24)] public float MouseVecX;
    [FieldOffset(28)] public float MouseVecY;
}

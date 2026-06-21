namespace AlvorKit.Windowing;

/// <summary>Describes a keyboard, mouse wheel, and modifier combination for a control.</summary>
public sealed record class KeyBinding
{
    /// <summary>Gets the key that activates while held.</summary>
    public Keys? KeyDown { get; init; }

    /// <summary>Gets the key that activates on the initial press.</summary>
    public Keys? KeyPress { get; init; }

    /// <summary>Gets the key that activates on initial press and key-repeat events.</summary>
    public Keys? KeyPressRepeat { get; init; }

    /// <summary>Gets the mouse wheel direction that activates the binding.</summary>
    public MouseScrollDirection? MouseScroll { get; init; }

    /// <summary>Gets the required shift state.</summary>
    public KeyModifierState Shift { get; init; }

    /// <summary>Gets the required control state.</summary>
    public KeyModifierState Control { get; init; }

    /// <summary>Gets the required alt state.</summary>
    public KeyModifierState Alt { get; init; }
}

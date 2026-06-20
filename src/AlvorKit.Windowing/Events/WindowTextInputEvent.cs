namespace AlvorKit.Windowing;

/// <summary>Unicode text input data reported by a host window.</summary>
/// <param name="Rune">The Unicode scalar value entered by the user.</param>
public readonly record struct WindowTextInputEvent(Rune Rune);

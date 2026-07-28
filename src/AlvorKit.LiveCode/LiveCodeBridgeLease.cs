namespace AlvorKit.LiveCode;

/// <summary>Declares the runtime reservation a structured bridge requires.</summary>
public enum LiveCodeBridgeLease
{
    /// <summary>The operation does not reserve a shared runtime resource.</summary>
    None,

    /// <summary>The operation temporarily owns keyboard, pointer, and text input.</summary>
    ExclusiveInput
}

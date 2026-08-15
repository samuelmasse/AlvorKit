namespace AlvorKit;

/// <summary>Configuration for a single-line text field.</summary>
public sealed record BlendTextFieldOptions
{
    /// <summary>Gets the muted placeholder shown while the field is empty and not being edited.</summary>
    public string Placeholder { get; init; } = string.Empty;

    /// <summary>Gets the committed text reader.</summary>
    public required Func<string> Get { get; init; }

    /// <summary>Gets the commit writer, called on Enter, Tab, or blur.</summary>
    public required Action<string> Set { get; init; }

    /// <summary>
    /// Gets the optional live edit writer, called after each text change while the field is active.
    /// </summary>
    public Action<string>? OnChanged { get; init; }

    /// <summary>
    /// Gets whether Escape publishes the field's original value through <see cref="OnChanged"/> before
    /// ending the edit. Disable this for persistent live filters where Escape closes a surrounding popup.
    /// </summary>
    public bool RevertOnEscape { get; init; } = true;
}

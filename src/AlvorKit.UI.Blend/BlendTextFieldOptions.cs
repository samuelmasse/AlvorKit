namespace AlvorKit.UI.Blend;

/// <summary>Configuration for a single-line text field.</summary>
public sealed record BlendTextFieldOptions
{
    /// <summary>Gets the muted placeholder shown while the field is empty and not being edited.</summary>
    public string Placeholder { get; init; } = string.Empty;

    /// <summary>Gets the committed text reader.</summary>
    public required Func<string> Get { get; init; }

    /// <summary>Gets the commit writer, called on Enter, Tab, or blur; Escape reverts without calling it.</summary>
    public required Action<string> Set { get; init; }
}

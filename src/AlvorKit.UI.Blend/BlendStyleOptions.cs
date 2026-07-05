namespace AlvorKit.UI.Blend;

/// <summary>Configuration used to create a reusable Blend style.</summary>
public sealed record BlendStyleOptions
{
    /// <summary>Gets the regular font face used by the style.</summary>
    public required Font Font { get; init; }

    /// <summary>Gets the optional emphasis font face; regular text font is used when this is omitted.</summary>
    public Font? EmphasisFont { get; init; }

    /// <summary>Gets the optional generated control chrome used for rounded controls.</summary>
    public BlendControlChrome? Chrome { get; init; }

    /// <summary>Gets the optional keyboard; focused controls run their click on Enter when it is provided.</summary>
    public Keyboard? Keyboard { get; init; }
}

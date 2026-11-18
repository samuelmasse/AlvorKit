namespace AlvorKit;

/// <summary>Injected reference dependency captured by the editable service.</summary>
[Root]
public sealed class PulsePalette
{
    /// <summary>Gets the initial visual identity.</summary>
    public Vec4 Original { get; } = (0.05f, 0.16f, 0.42f, 1f);

    /// <summary>Gets the visibly different source-updated identity.</summary>
    public Vec4 Updated { get; } = (0.52f, 0.04f, 0.24f, 1f);
}

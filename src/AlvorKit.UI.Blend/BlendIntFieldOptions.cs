namespace AlvorKit.UI.Blend;

/// <summary>Configuration for an integer drag/edit field.</summary>
public sealed record BlendIntFieldOptions
{
    /// <summary>Gets the muted label shown inside the field.</summary>
    public required string Label { get; init; }

    /// <summary>Gets the value reader; called every frame for display and at drag/edit start.</summary>
    public required Func<int> Get { get; init; }

    /// <summary>Gets the value writer; called only when the value actually changes.</summary>
    public required Action<int> Set { get; init; }

    /// <summary>Gets the inclusive minimum; values are clamped on drag, arrow steps, and edit commit.</summary>
    public int Min { get; init; } = int.MinValue;

    /// <summary>Gets the inclusive maximum; values are clamped on drag, arrow steps, and edit commit.</summary>
    public int Max { get; init; } = int.MaxValue;
}

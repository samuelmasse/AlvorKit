namespace AlvorKit.UI.Blend;

/// <summary>Configuration for a float drag/edit field; leave the range infinite for an unbounded number field.</summary>
public sealed record BlendNumberFieldOptions
{
    /// <summary>Gets the muted label shown inside the field.</summary>
    public required string Label { get; init; }

    /// <summary>Gets the value reader; called every frame for display and at drag/edit start.</summary>
    public required Func<float> Get { get; init; }

    /// <summary>Gets the value writer; called only when the value actually changes.</summary>
    public required Action<float> Set { get; init; }

    /// <summary>Gets the value change per <see cref="BlendMetrics.DragPixelsPerStep"/> of drag, per arrow click, and per Ctrl snap increment.</summary>
    public float Step { get; init; } = 0.01f;

    /// <summary>Gets the inclusive minimum; values are clamped on drag and edit commit.</summary>
    public float Min { get; init; } = float.NegativeInfinity;

    /// <summary>Gets the inclusive maximum; values are clamped on drag and edit commit.</summary>
    public float Max { get; init; } = float.PositiveInfinity;

    /// <summary>Gets the numeric display format.</summary>
    public string Format { get; init; } = "0.000";
}

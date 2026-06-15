namespace AlvorKit.Script.Bindgen;

/// <summary>Mutable transformation plan for one generated combined overload.</summary>
/// <param name="Command">Command being transformed.</param>
internal sealed class GlCombinedOverloadPlan(GlCommand Command)
{
    /// <summary>Command being transformed.</summary>
    public GlCommand Command { get; } = Command;

    /// <summary>Command parameters for convenient indexed access.</summary>
    public IReadOnlyList<GlParameter> Parameters { get; } = Command.Parameters;

    /// <summary>Transformation selected for each parameter.</summary>
    public GlExtensionPlanKind[] Plans { get; } = new GlExtensionPlanKind[Command.Parameters.Count];

    /// <summary>Raw call argument expression for each parameter.</summary>
    public string?[] Arguments { get; } = new string?[Command.Parameters.Count];

    /// <summary>Pointer parameter indexes already represented by managed substitutes.</summary>
    public HashSet<int> SpannedPointers { get; } = [];

    /// <summary>Pointer references grouped by count-parameter index.</summary>
    public Dictionary<int, List<GlCountReference>> ReferencesByCount { get; } = [];

    /// <summary>Configured raw pointer names that should become unsized spans.</summary>
    public IReadOnlySet<string> ConfiguredSpanParams { get; set; } = new HashSet<string>();

    /// <summary>Records that a count parameter controls a pointer parameter.</summary>
    public void AddCountReference(int countIndex, int pointerIndex, int divisor)
    {
        if (!ReferencesByCount.TryGetValue(countIndex, out var references))
            ReferencesByCount[countIndex] = references = [];
        references.Add(new(pointerIndex, divisor));
    }

    /// <summary>Returns whether the plan contains a public transformation worth emitting.</summary>
    public bool HasPublicTransform() =>
        Plans.Any(plan => plan != GlExtensionPlanKind.Keep && plan != GlExtensionPlanKind.Dropped);
}

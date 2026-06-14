namespace AlvorKit.Script.Bindgen;

/// <summary>Enum typing rules for one native function.</summary>
public sealed class FunctionEnums
{
    /// <summary>Enum represented by the raw integer return value, when any.</summary>
    public string? Return { get; set; }

    /// <summary>
    /// Per-parameter candidate types. One entry creates one typed overload; several entries create one
    /// overload per candidate. The literals <c>bool</c> and <c>int</c> are allowed beside enum names.
    /// </summary>
    public Dictionary<string, string[]> Params { get; set; } = [];
}

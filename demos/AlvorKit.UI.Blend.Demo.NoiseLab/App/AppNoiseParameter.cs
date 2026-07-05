namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>How a FastNoise2 metadata member maps onto a Blend form control.</summary>
public enum AppNoiseParameterKind
{
    /// <summary>Float variable; bounded metadata ranges render as sliders, unbounded as number fields.</summary>
    Float,

    /// <summary>Integer variable; renders as an int drag field.</summary>
    Int,

    /// <summary>Enum variable; renders as a dropdown over the metadata enum names.</summary>
    Enum,

    /// <summary>Hybrid float member (float or node input); renders as a number field driving the float side.</summary>
    Hybrid,
}

/// <summary>
/// One editable member of a FastNoise2 node, captured from runtime metadata. FastNoise2 exposes no value
/// getters, so the current value lives here and every write goes through the owning field to the node.
/// </summary>
public sealed class AppNoiseParameter
{
    /// <summary>Gets the control kind this member maps to.</summary>
    public required AppNoiseParameterKind Kind { get; init; }

    /// <summary>Gets the metadata member name, used as the field label.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the metadata description, shown as the field tooltip.</summary>
    public required string Tooltip { get; init; }

    /// <summary>Gets the variable index (or hybrid index for hybrids) used when writing the node.</summary>
    public required int Index { get; init; }

    /// <summary>Gets the suggested minimum from metadata; equals <see cref="Max"/> or worse when unbounded.</summary>
    public float Min { get; init; } = float.NegativeInfinity;

    /// <summary>Gets the suggested maximum from metadata.</summary>
    public float Max { get; init; } = float.PositiveInfinity;

    /// <summary>Gets the enum member names for <see cref="AppNoiseParameterKind.Enum"/> members.</summary>
    public IReadOnlyList<BlendDropdownItem> EnumItems { get; init; } = [];

    /// <summary>Gets or sets the current value; int and enum members round it on write.</summary>
    public float Value { get; set; }

    /// <summary>Gets whether the metadata declared a usable finite range.</summary>
    public bool HasRange => float.IsFinite(Min) && float.IsFinite(Max) && Min < Max;
}

namespace AlvorKit.UI.Blend;

/// <summary>UI metadata components contributed by the Blend style package.</summary>
[Components(SkipBuilder = true)]
public interface IBlendUiComponents
{
    /// <summary>Tooltip text displayed when hovering this node.</summary>
    UiText TooltipFV { get; set; }

    /// <summary>Swatch color shown before the tooltip title; transparent hides the swatch.</summary>
    UiProp<Vec4> TooltipColorFV { get; set; }
}

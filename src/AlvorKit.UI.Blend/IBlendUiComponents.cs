namespace AlvorKit.UI.Blend;

/// <summary>UI metadata components contributed by the Blend style package.</summary>
[Components(SkipBuilder = true)]
public interface IBlendUiComponents
{
    /// <summary>Tooltip text displayed when hovering this node.</summary>
    UiText TooltipFV { get; set; }
}

namespace AlvorKit.UI;

[Components(SkipBuilder = true)]
public interface IUiSizeComponents
{
    /// <summary>Whether this node is excluded from layout flow.</summary>
    UiProp<bool> IsFloatingFV { get; set; }
    /// <summary>Whether this node is resized after siblings are laid out.</summary>
    UiProp<bool> IsPostSizedFV { get; set; }
    /// <summary>How child nodes are weighted for size distribution.</summary>
    UiProp<InnerSizing> InnerSizingFV { get; set; }
    /// <summary>Whether this node uses its own size or a weighted share.</summary>
    UiProp<SizeWeightType> SizeWeightTypeFV { get; set; }
    /// <summary>Spacing between child nodes in a list layout.</summary>
    UiProp<float> InnerSpacingFV { get; set; }
    /// <summary>Weight factor for weighted size distribution.</summary>
    UiProp<float?> SizeWeightFV { get; set; }
    /// <summary>Padding around the node's content area (left, top, right, bottom).</summary>
    UiProp<Vec4> PaddingFV { get; set; }
    /// <summary>Margin around the node (left, top, right, bottom).</summary>
    UiProp<Vec4> MarginFV { get; set; }
    /// <summary>Fixed size of this node.</summary>
    UiProp<Vec2> SizeFV { get; set; }
    /// <summary>Size as a fraction of the parent's available space.</summary>
    UiProp<Vec2?> SizeRelativeFV { get; set; }
    /// <summary>Size as a fraction of the largest child's size.</summary>
    UiProp<Vec2> SizeInnerMaxRelativeFV { get; set; }
    /// <summary>Size as a fraction of the sum of children's sizes.</summary>
    UiProp<Vec2> SizeInnerSumRelativeFV { get; set; }
    /// <summary>Size as a fraction of the text's measured size.</summary>
    UiProp<Vec2> SizeTextRelativeFV { get; set; }
    /// <summary>Font used for text measurement and rendering.</summary>
    UiProp<Font?> FontFV { get; set; }
    /// <summary>Font size in points.</summary>
    UiProp<int> FontSizeFV { get; set; }
    /// <summary>Padding around the font area.</summary>
    UiProp<Vec4> FontPaddingFV { get; set; }
    /// <summary>Text content of the node.</summary>
    [ComponentToString] UiText TextFV { get; set; }
    /// <summary>Padding around the text area.</summary>
    UiProp<Vec4> TextPaddingFV { get; set; }

    /// <summary>Resolved size of this node.</summary>
    Vec2 SizeR { get; internal set; }
    /// <summary>Resolved sum of children's sizes plus spacing.</summary>
    Vec2 SizeInnerSumR { get; internal set; }
    /// <summary>Resolved padding of this node.</summary>
    Vec4 PaddingR { get; internal set; }
    /// <summary>Resolved margin of this node.</summary>
    Vec4 MarginR { get; internal set; }
    /// <summary>Resolved horizontal size from weighted distribution.</summary>
    float? HorizontalWeightSizeR { get; internal set; }
    /// <summary>Resolved vertical size from weighted distribution.</summary>
    float? VerticalWeightSizeR { get; internal set; }
}

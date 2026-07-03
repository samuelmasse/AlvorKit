namespace AlvorKit.UI;

[Components(SkipBuilder = true)]
public interface IUiPositionComponents
{
    /// <summary>Layout direction for child nodes.</summary>
    UiProp<InnerLayout> InnerLayoutFV { get; set; }
    /// <summary>Alignment of this node within its parent.</summary>
    UiProp<Alignment> AlignmentFV { get; set; }
    /// <summary>Position offset of this node.</summary>
    UiProp<Vec2> OffsetFV { get; set; }
    /// <summary>Scroll offset applied to child nodes.</summary>
    UiProp<Vec2> InnerScrollOffsetFV { get; set; }
    /// <summary>Position offset relative to text size.</summary>
    UiProp<Vec2> OffsetTextRelativeFV { get; set; }
    /// <summary>Snaps alignment to the nearest multiple of this value. Inherited from parent's InnerAlignmentSnap if null.</summary>
    UiProp<float?> AlignmentSnapFV { get; set; }
    /// <summary>Default alignment snap applied to child nodes.</summary>
    UiProp<float?> InnerAlignmentSnapFV { get; set; }
    /// <summary>Rounds this node's resolved size up to the nearest multiple of this value</summary>
    UiProp<float> SizeAlignmentSnapFV { get; set; }

    /// <summary>Resolved position offset of this node.</summary>
    Vec2 OffsetR { get; internal set; }
    /// <summary>Resolved absolute position of this node relative to root.</summary>
    Vec2 PositionR { get; internal set; }
}

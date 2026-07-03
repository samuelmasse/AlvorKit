namespace AlvorKit.UI;

[Components(SkipBuilder = true)]
public interface IUiDrawComponents
{
    /// <summary>Background color of the node.</summary>
    UiProp<Vec4> ColorFV { get; set; }
    /// <summary>Texture to draw on the node.</summary>
    UiProp<Texture?> TextureFV { get; set; }
    /// <summary>Sub-region position within the texture.</summary>
    UiProp<Vec2?> TextureSubPositionFV { get; set; }
    /// <summary>Sub-region size within the texture.</summary>
    UiProp<Vec2?> TextureSubSizeFV { get; set; }
    /// <summary>Sub-region size relative to the node's rendered size.</summary>
    UiProp<Vec2?> TextureSubSizeRelativeFV { get; set; }
    /// <summary>Color tint applied to the texture.</summary>
    UiProp<Vec4?> TextureColorFV { get; set; }
    /// <summary>Rotation applied to the texture.</summary>
    UiProp<SpriteBatchRotation?> TextureRotationFV { get; set; }
    /// <summary>Flip applied to the texture.</summary>
    UiProp<SpriteBatchFlip?> TextureFlipFV { get; set; }
    /// <summary>Margin around the texture (left, top, right, bottom).</summary>
    UiProp<Vec4> TextureMarginFV { get; set; }
    /// <summary>Origin point for tiling relative to the sub size (0,0 = top-left, 1,1 = bottom-right).</summary>
    UiProp<Vec2?> TextureOriginRelativeFV { get; set; }
    /// <summary>Snaps texture sub-position to the nearest multiple of this value in texture pixels.</summary>
    UiProp<float> TextureAlignmentSnapFV { get; set; }
    /// <summary>Text alignment within the node.</summary>
    UiProp<Alignment?> TextAlignmentFV { get; set; }
    /// <summary>Additional offset applied to the text rendering within the node.</summary>
    UiProp<Vec2> TextOffsetFV { get; set; }
    /// <summary>Snaps the text block's starting position to the nearest multiple of this value (in UI units).</summary>
    UiProp<float> TextAlignmentSnapFV { get; set; }
    /// <summary>Snaps each individual glyph's position to the nearest multiple of this value (in UI units).</summary>
    UiProp<float> TextGlyphAlignmentSnapFV { get; set; }
    /// <summary>Color of the text.</summary>
    UiProp<Vec4> TextColorFV { get; set; }
    /// <summary>Offset of the text shadow.</summary>
    UiProp<Vec2?> TextShadowOffsetFV { get; set; }
    /// <summary>Absolute color of the text shadow.</summary>
    UiProp<Vec4?> TextShadowColorFV { get; set; }
    /// <summary>Shadow color as a multiplier of the text color.</summary>
    UiProp<Vec4?> TextShadowColorRelativeFV { get; set; }
    /// <summary>Callback invoked during draw with the node's screen offset.</summary>
    UiCallback<Action?> OnDrawFV { get; set; }
    /// <summary>Callback invoked every frame.</summary>
    UiCallback<Action?> OnFrameFV { get; set; }
}

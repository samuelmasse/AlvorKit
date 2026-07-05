namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearColor()"/>.</remarks>
    public override void ClearColor(float red, float green, float blue, float alpha)
    {
        state.clearColor.Set(nameof(ClearColor), (red, green, blue, alpha));
        base.ClearColor(red, green, blue, alpha);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClampColor()"/>.</remarks>
    public override void ClampColor(GlClampColorTarget target, GlClampColorMode clamp)
    {
        state.clampColor.Set(nameof(ClampColor), clamp);
        base.ClampColor(target, clamp);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetColorMask()"/>. Cannot be combined with <c>glColorMaski</c>.</remarks>
    public override void ColorMask(bool red, bool green, bool blue, bool alpha)
    {
        if (state.colorMaskMap.HasAny) throw new GlConflictException(nameof(ColorMask), nameof(ColorMaski));
        state.colorMask.Set(nameof(ColorMask), (red, green, blue, alpha));
        base.ColorMask(red, green, blue, alpha);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetColorMask(uint)"/> for the same buffer.
    /// Cannot be combined with <c>glColorMask</c>.
    /// </remarks>
    public override void ColorMaski(uint index, bool r, bool g, bool b, bool a)
    {
        if (state.colorMask.IsSet) throw new GlConflictException(nameof(ColorMaski), nameof(ColorMask));
        state.colorMaskMap.Set(nameof(ColorMaski), index, (r, g, b, a));
        base.ColorMaski(index, r, g, b, a);
    }
}

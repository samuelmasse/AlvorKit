namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendFunc()"/>.
    /// Cannot be combined with <c>glBlendFunci</c>, <c>glBlendFuncSeparate</c>, or <c>glBlendFuncSeparatei</c>.
    /// </remarks>
    public override void BlendFunc(GlBlendingFactor sfactor, GlBlendingFactor dfactor)
    {
        if (blendFuncSeparate.IsSet) throw new GlConflictException(nameof(BlendFunc), nameof(BlendFuncSeparate));
        if (blendFuncMap.HasAny) throw new GlConflictException(nameof(BlendFunc), nameof(BlendFunci));
        if (blendFuncSeparateMap.HasAny) throw new GlConflictException(nameof(BlendFunc), nameof(BlendFuncSeparatei));
        blendFunc.Set(nameof(BlendFunc), (sfactor, dfactor));
        base.BlendFunc(sfactor, dfactor);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendFunc(uint)"/> for the same buffer.
    /// Cannot be combined with <c>glBlendFunc</c>, <c>glBlendFuncSeparate</c>, or <c>glBlendFuncSeparatei</c>.
    /// </remarks>
    public override void BlendFunci(uint buf, GlBlendingFactor src, GlBlendingFactor dst)
    {
        if (blendFunc.IsSet) throw new GlConflictException(nameof(BlendFunci), nameof(BlendFunc));
        if (blendFuncSeparate.IsSet) throw new GlConflictException(nameof(BlendFunci), nameof(BlendFuncSeparate));
        if (blendFuncSeparateMap.HasAny) throw new GlConflictException(nameof(BlendFunci), nameof(BlendFuncSeparatei));
        blendFuncMap.Set(nameof(BlendFunci), buf, (src, dst));
        base.BlendFunci(buf, src, dst);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendFuncSeparate()"/>.
    /// Cannot be combined with <c>glBlendFunc</c>, <c>glBlendFunci</c>, or <c>glBlendFuncSeparatei</c>.
    /// </remarks>
    public override void BlendFuncSeparate(GlBlendingFactor sfactorRGB, GlBlendingFactor dfactorRGB, GlBlendingFactor sfactorAlpha, GlBlendingFactor dfactorAlpha)
    {
        if (blendFunc.IsSet) throw new GlConflictException(nameof(BlendFuncSeparate), nameof(BlendFunc));
        if (blendFuncMap.HasAny) throw new GlConflictException(nameof(BlendFuncSeparate), nameof(BlendFunci));
        if (blendFuncSeparateMap.HasAny) throw new GlConflictException(nameof(BlendFuncSeparate), nameof(BlendFuncSeparatei));
        blendFuncSeparate.Set(nameof(BlendFuncSeparate), (sfactorRGB, dfactorRGB, sfactorAlpha, dfactorAlpha));
        base.BlendFuncSeparate(sfactorRGB, dfactorRGB, sfactorAlpha, dfactorAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendFuncSeparate(uint)"/> for the same buffer.
    /// Cannot be combined with <c>glBlendFunc</c>, <c>glBlendFunci</c>, or <c>glBlendFuncSeparate</c>.
    /// </remarks>
    public override void BlendFuncSeparatei(uint buf, GlBlendingFactor srcRGB, GlBlendingFactor dstRGB, GlBlendingFactor srcAlpha, GlBlendingFactor dstAlpha)
    {
        if (blendFunc.IsSet) throw new GlConflictException(nameof(BlendFuncSeparatei), nameof(BlendFunc));
        if (blendFuncSeparate.IsSet) throw new GlConflictException(nameof(BlendFuncSeparatei), nameof(BlendFuncSeparate));
        if (blendFuncMap.HasAny) throw new GlConflictException(nameof(BlendFuncSeparatei), nameof(BlendFunci));
        blendFuncSeparateMap.Set(nameof(BlendFuncSeparatei), buf, (srcRGB, dstRGB, srcAlpha, dstAlpha));
        base.BlendFuncSeparatei(buf, srcRGB, dstRGB, srcAlpha, dstAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendEquation()"/>.
    /// Cannot be combined with <c>glBlendEquationSeparate</c> or <c>glBlendEquationSeparatei</c>.
    /// </remarks>
    public override void BlendEquation(GlBlendEquationModeEXT mode)
    {
        if (blendEquationSeparate.IsSet) throw new GlConflictException(nameof(BlendEquation), nameof(BlendEquationSeparate));
        if (blendEquationSeparateMap.HasAny) throw new GlConflictException(nameof(BlendEquation), nameof(BlendEquationSeparatei));
        blendEquation.Set(nameof(BlendEquation), mode);
        base.BlendEquation(mode);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendEquationSeparate()"/>.
    /// Cannot be combined with <c>glBlendEquation</c> or <c>glBlendEquationSeparatei</c>.
    /// </remarks>
    public override void BlendEquationSeparate(GlBlendEquationModeEXT modeRGB, GlBlendEquationModeEXT modeAlpha)
    {
        if (blendEquation.IsSet) throw new GlConflictException(nameof(BlendEquationSeparate), nameof(BlendEquation));
        if (blendEquationSeparateMap.HasAny) throw new GlConflictException(nameof(BlendEquationSeparate), nameof(BlendEquationSeparatei));
        blendEquationSeparate.Set(nameof(BlendEquationSeparate), (modeRGB, modeAlpha));
        base.BlendEquationSeparate(modeRGB, modeAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendEquationSeparate(uint)"/> for the same buffer.
    /// Cannot be combined with <c>glBlendEquation</c> or <c>glBlendEquationSeparate</c>.
    /// </remarks>
    public override void BlendEquationSeparatei(uint buf, GlBlendEquationModeEXT modeRGB, GlBlendEquationModeEXT modeAlpha)
    {
        if (blendEquation.IsSet) throw new GlConflictException(nameof(BlendEquationSeparatei), nameof(BlendEquation));
        if (blendEquationSeparate.IsSet) throw new GlConflictException(nameof(BlendEquationSeparatei), nameof(BlendEquationSeparate));
        blendEquationSeparateMap.Set(nameof(BlendEquationSeparatei), buf, (modeRGB, modeAlpha));
        base.BlendEquationSeparatei(buf, modeRGB, modeAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetBlendColor()"/>.</remarks>
    public override void BlendColor(float red, float green, float blue, float alpha)
    {
        blendColor.Set(nameof(BlendColor), (red, green, blue, alpha));
        base.BlendColor(red, green, blue, alpha);
    }
}

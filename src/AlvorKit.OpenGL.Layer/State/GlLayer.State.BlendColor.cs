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
        if (state.blendFuncSeparate.IsSet) throw new GlConflictException(nameof(BlendFunc), nameof(BlendFuncSeparate));
        if (state.blendFuncMap.HasAny) throw new GlConflictException(nameof(BlendFunc), nameof(BlendFunci));
        if (state.blendFuncSeparateMap.HasAny) throw new GlConflictException(nameof(BlendFunc), nameof(BlendFuncSeparatei));
        state.blendFunc.Set(nameof(BlendFunc), (sfactor, dfactor));
        base.BlendFunc(sfactor, dfactor);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendFunc(uint)"/> for the same buffer.
    /// Cannot be combined with <c>glBlendFunc</c>, <c>glBlendFuncSeparate</c>, or <c>glBlendFuncSeparatei</c>.
    /// </remarks>
    public override void BlendFunci(uint buf, GlBlendingFactor src, GlBlendingFactor dst)
    {
        if (state.blendFunc.IsSet) throw new GlConflictException(nameof(BlendFunci), nameof(BlendFunc));
        if (state.blendFuncSeparate.IsSet) throw new GlConflictException(nameof(BlendFunci), nameof(BlendFuncSeparate));
        if (state.blendFuncSeparateMap.HasAny) throw new GlConflictException(nameof(BlendFunci), nameof(BlendFuncSeparatei));
        state.blendFuncMap.Set(nameof(BlendFunci), buf, (src, dst));
        base.BlendFunci(buf, src, dst);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendFuncSeparate()"/>.
    /// Cannot be combined with <c>glBlendFunc</c>, <c>glBlendFunci</c>, or <c>glBlendFuncSeparatei</c>.
    /// </remarks>
    public override void BlendFuncSeparate(GlBlendingFactor sfactorRGB, GlBlendingFactor dfactorRGB, GlBlendingFactor sfactorAlpha, GlBlendingFactor dfactorAlpha)
    {
        if (state.blendFunc.IsSet) throw new GlConflictException(nameof(BlendFuncSeparate), nameof(BlendFunc));
        if (state.blendFuncMap.HasAny) throw new GlConflictException(nameof(BlendFuncSeparate), nameof(BlendFunci));
        if (state.blendFuncSeparateMap.HasAny) throw new GlConflictException(nameof(BlendFuncSeparate), nameof(BlendFuncSeparatei));
        state.blendFuncSeparate.Set(nameof(BlendFuncSeparate), (sfactorRGB, dfactorRGB, sfactorAlpha, dfactorAlpha));
        base.BlendFuncSeparate(sfactorRGB, dfactorRGB, sfactorAlpha, dfactorAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendFuncSeparate(uint)"/> for the same buffer.
    /// Cannot be combined with <c>glBlendFunc</c>, <c>glBlendFunci</c>, or <c>glBlendFuncSeparate</c>.
    /// </remarks>
    public override void BlendFuncSeparatei(uint buf, GlBlendingFactor srcRGB, GlBlendingFactor dstRGB, GlBlendingFactor srcAlpha, GlBlendingFactor dstAlpha)
    {
        if (state.blendFunc.IsSet) throw new GlConflictException(nameof(BlendFuncSeparatei), nameof(BlendFunc));
        if (state.blendFuncSeparate.IsSet) throw new GlConflictException(nameof(BlendFuncSeparatei), nameof(BlendFuncSeparate));
        if (state.blendFuncMap.HasAny) throw new GlConflictException(nameof(BlendFuncSeparatei), nameof(BlendFunci));
        state.blendFuncSeparateMap.Set(nameof(BlendFuncSeparatei), buf, (srcRGB, dstRGB, srcAlpha, dstAlpha));
        base.BlendFuncSeparatei(buf, srcRGB, dstRGB, srcAlpha, dstAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendEquation()"/>.
    /// Cannot be combined with <c>glBlendEquationSeparate</c> or <c>glBlendEquationSeparatei</c>.
    /// </remarks>
    public override void BlendEquation(GlBlendEquationModeEXT mode)
    {
        if (state.blendEquationSeparate.IsSet) throw new GlConflictException(nameof(BlendEquation), nameof(BlendEquationSeparate));
        if (state.blendEquationSeparateMap.HasAny) throw new GlConflictException(nameof(BlendEquation), nameof(BlendEquationSeparatei));
        state.blendEquation.Set(nameof(BlendEquation), mode);
        base.BlendEquation(mode);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendEquationSeparate()"/>.
    /// Cannot be combined with <c>glBlendEquation</c> or <c>glBlendEquationSeparatei</c>.
    /// </remarks>
    public override void BlendEquationSeparate(GlBlendEquationModeEXT modeRGB, GlBlendEquationModeEXT modeAlpha)
    {
        if (state.blendEquation.IsSet) throw new GlConflictException(nameof(BlendEquationSeparate), nameof(BlendEquation));
        if (state.blendEquationSeparateMap.HasAny) throw new GlConflictException(nameof(BlendEquationSeparate), nameof(BlendEquationSeparatei));
        state.blendEquationSeparate.Set(nameof(BlendEquationSeparate), (modeRGB, modeAlpha));
        base.BlendEquationSeparate(modeRGB, modeAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetBlendEquationSeparate(uint)"/> for the same buffer.
    /// Cannot be combined with <c>glBlendEquation</c> or <c>glBlendEquationSeparate</c>.
    /// </remarks>
    public override void BlendEquationSeparatei(uint buf, GlBlendEquationModeEXT modeRGB, GlBlendEquationModeEXT modeAlpha)
    {
        if (state.blendEquation.IsSet) throw new GlConflictException(nameof(BlendEquationSeparatei), nameof(BlendEquation));
        if (state.blendEquationSeparate.IsSet) throw new GlConflictException(nameof(BlendEquationSeparatei), nameof(BlendEquationSeparate));
        state.blendEquationSeparateMap.Set(nameof(BlendEquationSeparatei), buf, (modeRGB, modeAlpha));
        base.BlendEquationSeparatei(buf, modeRGB, modeAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetBlendColor()"/>.</remarks>
    public override void BlendColor(float red, float green, float blue, float alpha)
    {
        state.blendColor.Set(nameof(BlendColor), (red, green, blue, alpha));
        base.BlendColor(red, green, blue, alpha);
    }
}

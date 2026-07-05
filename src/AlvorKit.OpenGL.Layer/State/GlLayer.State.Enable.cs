namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="Disable(GlEnableCap)"/> for the same capability.</remarks>
    public override void Enable(GlEnableCap cap) { state.enableMap.Set(nameof(Enable), cap, true); base.Enable(cap); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <see cref="Enable(GlEnableCap)"/> for the same capability.</remarks>
    public override void Disable(GlEnableCap cap) { state.enableMap.Reset(nameof(Disable), cap); base.Disable(cap); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="Disablei(GlEnableCap, uint)"/> for the same capability and index.</remarks>
    public override void Enablei(GlEnableCap target, uint index) { state.indexedEnableMap.Set(nameof(Enablei), (target, index), true); base.Enablei(target, index); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <see cref="Enablei(GlEnableCap, uint)"/> for the same capability and index.</remarks>
    public override void Disablei(GlEnableCap target, uint index) { state.indexedEnableMap.Reset(nameof(Disablei), (target, index)); base.Disablei(target, index); }
}

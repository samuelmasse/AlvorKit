namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>
    /// Layer: Restores <c>glBlendFunc</c> to <see cref="DefaultBlendFunc"/>.
    /// Must be paired with exactly one earlier call to <c>glBlendFunc</c>.
    /// </summary>
    public void ResetBlendFunc()
    {
        state.blendFunc.Reset(nameof(BlendFunc));
        base.BlendFunc(DefaultBlendFunc.Source, DefaultBlendFunc.Destination);
    }

    /// <summary>
    /// Layer: Restores <c>glBlendFunci</c> for buffer <paramref name="buf"/>.
    /// Must be paired with exactly one earlier call to <c>glBlendFunci</c> for the same buffer.
    /// </summary>
    public void ResetBlendFunc(uint buf)
    {
        state.blendFuncMap.Reset(nameof(BlendFunci), buf);
        base.BlendFunci(buf, DefaultBlendFunc.Source, DefaultBlendFunc.Destination);
    }

    /// <summary>
    /// Layer: Restores <c>glBlendFuncSeparate</c> to <see cref="DefaultBlendFuncSeparate"/>.
    /// Must be paired with exactly one earlier call to <c>glBlendFuncSeparate</c>.
    /// </summary>
    public void ResetBlendFuncSeparate()
    {
        state.blendFuncSeparate.Reset(nameof(BlendFuncSeparate));
        base.BlendFuncSeparate(
            DefaultBlendFuncSeparate.SourceRgb,
            DefaultBlendFuncSeparate.DestinationRgb,
            DefaultBlendFuncSeparate.SourceAlpha,
            DefaultBlendFuncSeparate.DestinationAlpha);
    }

    /// <summary>
    /// Layer: Restores <c>glBlendFuncSeparatei</c> for buffer <paramref name="buf"/>.
    /// Must be paired with exactly one earlier call to <c>glBlendFuncSeparatei</c> for the same buffer.
    /// </summary>
    public void ResetBlendFuncSeparate(uint buf)
    {
        state.blendFuncSeparateMap.Reset(nameof(BlendFuncSeparatei), buf);
        base.BlendFuncSeparatei(
            buf,
            DefaultBlendFuncSeparate.SourceRgb,
            DefaultBlendFuncSeparate.DestinationRgb,
            DefaultBlendFuncSeparate.SourceAlpha,
            DefaultBlendFuncSeparate.DestinationAlpha);
    }

    /// <summary>
    /// Layer: Restores <c>glBlendEquation</c> to <see cref="DefaultBlendEquation"/>.
    /// Must be paired with exactly one earlier call to <c>glBlendEquation</c>.
    /// </summary>
    public void ResetBlendEquation()
    {
        state.blendEquation.Reset(nameof(BlendEquation));
        base.BlendEquation(DefaultBlendEquation);
    }

    /// <summary>
    /// Layer: Restores <c>glBlendEquationSeparate</c> to <see cref="DefaultBlendEquationSeparate"/>.
    /// Must be paired with exactly one earlier call to <c>glBlendEquationSeparate</c>.
    /// </summary>
    public void ResetBlendEquationSeparate()
    {
        state.blendEquationSeparate.Reset(nameof(BlendEquationSeparate));
        base.BlendEquationSeparate(DefaultBlendEquationSeparate.Rgb, DefaultBlendEquationSeparate.Alpha);
    }

    /// <summary>
    /// Layer: Restores <c>glBlendEquationSeparatei</c> for buffer <paramref name="buf"/>.
    /// Must be paired with exactly one earlier call to <c>glBlendEquationSeparatei</c> for the same buffer.
    /// </summary>
    public void ResetBlendEquationSeparate(uint buf)
    {
        state.blendEquationSeparateMap.Reset(nameof(BlendEquationSeparatei), buf);
        base.BlendEquationSeparatei(buf, DefaultBlendEquationSeparate.Rgb, DefaultBlendEquationSeparate.Alpha);
    }

    /// <summary>
    /// Layer: Restores <c>glBlendColor</c> to <see cref="DefaultBlendColor"/>.
    /// Must be paired with exactly one earlier call to <c>glBlendColor</c>.
    /// </summary>
    public void ResetBlendColor()
    {
        state.blendColor.Reset(nameof(BlendColor));
        base.BlendColor(DefaultBlendColor.Red, DefaultBlendColor.Green, DefaultBlendColor.Blue, DefaultBlendColor.Alpha);
    }

    /// <summary>
    /// Layer: Restores <c>glClearColor</c> to <see cref="DefaultClearColor"/>.
    /// Must be paired with exactly one earlier call to <c>glClearColor</c>.
    /// </summary>
    public void ResetClearColor()
    {
        state.clearColor.Reset(nameof(ClearColor));
        base.ClearColor(DefaultClearColor.Red, DefaultClearColor.Green, DefaultClearColor.Blue, DefaultClearColor.Alpha);
    }

    /// <summary>
    /// Layer: Restores <c>glClampColor</c> to <see cref="DefaultClampColor"/>.
    /// Must be paired with exactly one earlier call to <c>glClampColor</c>.
    /// </summary>
    public void ResetClampColor()
    {
        state.clampColor.Reset(nameof(ClampColor));
        base.ClampColor(DefaultClampColor.Target, DefaultClampColor.Clamp);
    }

    /// <summary>
    /// Layer: Restores <c>glColorMask</c> to <see cref="DefaultColorMask"/>.
    /// Must be paired with exactly one earlier call to <c>glColorMask</c>.
    /// </summary>
    public void ResetColorMask()
    {
        state.colorMask.Reset(nameof(ColorMask));
        base.ColorMask(DefaultColorMask.Red, DefaultColorMask.Green, DefaultColorMask.Blue, DefaultColorMask.Alpha);
    }

    /// <summary>
    /// Layer: Restores <c>glColorMaski</c> for buffer <paramref name="buf"/>.
    /// Must be paired with exactly one earlier call to <c>glColorMaski</c> for the same buffer.
    /// </summary>
    public void ResetColorMask(uint buf)
    {
        state.colorMaskMap.Reset(nameof(ColorMaski), buf);
        base.ColorMaski(buf, DefaultColorMask.Red, DefaultColorMask.Green, DefaultColorMask.Blue, DefaultColorMask.Alpha);
    }
}

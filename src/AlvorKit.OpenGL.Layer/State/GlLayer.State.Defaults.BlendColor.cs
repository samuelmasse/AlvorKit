namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>Layer: The default value <see cref="ResetBlendFunc()"/> restores.</summary>
    public virtual (GlBlendingFactor Source, GlBlendingFactor Destination) DefaultBlendFunc => (GlBlendingFactor.One, GlBlendingFactor.Zero);

    /// <summary>Layer: The default value <see cref="ResetBlendFuncSeparate()"/> restores.</summary>
    public virtual (
        GlBlendingFactor SourceRgb,
        GlBlendingFactor DestinationRgb,
        GlBlendingFactor SourceAlpha,
        GlBlendingFactor DestinationAlpha) DefaultBlendFuncSeparate =>
            (GlBlendingFactor.One, GlBlendingFactor.Zero, GlBlendingFactor.One, GlBlendingFactor.Zero);

    /// <summary>Layer: The default value <see cref="ResetBlendEquation()"/> restores.</summary>
    public virtual GlBlendEquationModeEXT DefaultBlendEquation => GlBlendEquationModeEXT.FuncAdd;

    /// <summary>Layer: The default value <see cref="ResetBlendEquationSeparate()"/> restores.</summary>
    public virtual (GlBlendEquationModeEXT Rgb, GlBlendEquationModeEXT Alpha) DefaultBlendEquationSeparate =>
        (GlBlendEquationModeEXT.FuncAdd, GlBlendEquationModeEXT.FuncAdd);

    /// <summary>Layer: The default value <see cref="ResetBlendColor()"/> restores.</summary>
    public virtual (float Red, float Green, float Blue, float Alpha) DefaultBlendColor => (0f, 0f, 0f, 0f);

    /// <summary>Layer: The default value <see cref="ResetClearColor()"/> restores.</summary>
    public virtual (float Red, float Green, float Blue, float Alpha) DefaultClearColor => (0f, 0f, 0f, 0f);

    /// <summary>Layer: The default value <see cref="ResetClampColor()"/> restores.</summary>
    public virtual (GlClampColorTarget Target, GlClampColorMode Clamp) DefaultClampColor => (GlClampColorTarget.ClampReadColor, GlClampColorMode.FixedOnly);

    /// <summary>Layer: The default value <see cref="ResetColorMask()"/> restores.</summary>
    public virtual (bool Red, bool Green, bool Blue, bool Alpha) DefaultColorMask => (true, true, true, true);
}

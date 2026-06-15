namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>Layer: The default value <see cref="ResetActiveTexture()"/> restores.</summary>
    public virtual GlTextureUnit DefaultActiveTexture => GlTextureUnit.Texture0;

    /// <summary>Layer: The default value <see cref="ResetClipControl()"/> restores.</summary>
    public virtual (GlClipControlOrigin Origin, GlClipControlDepth Depth) DefaultClipControl => (GlClipControlOrigin.LowerLeft, GlClipControlDepth.NegativeOneToOne);

    /// <summary>Layer: The default value <see cref="ResetHint(GlHintTarget)"/> restores.</summary>
    public virtual GlHintMode DefaultHint => GlHintMode.DontCare;

    /// <summary>Layer: The default value <see cref="ResetMinSampleShading()"/> restores.</summary>
    public virtual float DefaultMinSampleShading => 0f;

    /// <summary>Layer: The default value <see cref="ResetSampleCoverage()"/> restores.</summary>
    public virtual (float Value, bool Invert) DefaultSampleCoverage => (1f, false);

    /// <summary>Layer: The default value <see cref="ResetSampleMask(uint)"/> restores.</summary>
    public virtual uint DefaultSampleMask => uint.MaxValue;

    /// <summary>Layer: The default value <see cref="ResetReadBuffer()"/> restores.</summary>
    public virtual GlReadBufferMode DefaultReadBuffer => GlReadBufferMode.ColorAttachment0;

    /// <summary>Layer: The default value <see cref="ResetPixelStore(GlPixelStoreParameter)"/> restores for <paramref name="pname"/>.</summary>
    public virtual int DefaultPixelStore(GlPixelStoreParameter pname) => pname is GlPixelStoreParameter.PackAlignment or GlPixelStoreParameter.UnpackAlignment ? 4 : 0;

    /// <summary>Layer: The default value <see cref="ResetPatchParameter(GlPatchParameterName)"/> restores for <paramref name="pname"/>.</summary>
    public virtual int DefaultPatchParameter(GlPatchParameterName pname) => pname == GlPatchParameterName.PatchVertices ? 3 : 0;

    /// <summary>Layer: The default value <see cref="ResetPointParameter(GlPointParameterName)"/> restores for <paramref name="pname"/>.</summary>
    public virtual float DefaultPointParameter(GlPointParameterName pname) => pname == GlPointParameterName.PointFadeThresholdSize ? 1f : 0f;
}

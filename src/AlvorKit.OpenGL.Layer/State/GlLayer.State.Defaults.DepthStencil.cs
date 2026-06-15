namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>Layer: The default value <see cref="ResetDepthFunc()"/> restores.</summary>
    public virtual GlDepthFunction DefaultDepthFunc => GlDepthFunction.Less;

    /// <summary>Layer: The default value <see cref="ResetDepthMask()"/> restores.</summary>
    public virtual bool DefaultDepthMask => true;

    /// <summary>Layer: The default value <see cref="ResetClearDepth()"/> restores.</summary>
    public virtual double DefaultClearDepth => 1.0;

    /// <summary>Layer: The default value <see cref="ResetDepthRange()"/> restores.</summary>
    public virtual (double Near, double Far) DefaultDepthRange => (0.0, 1.0);

    /// <summary>Layer: The default value <see cref="ResetClearStencil()"/> restores.</summary>
    public virtual int DefaultClearStencil => 0;

    /// <summary>Layer: The default value <see cref="ResetStencilFunc()"/> restores.</summary>
    public virtual (GlStencilFunction Func, int Ref, uint Mask) DefaultStencilFunc => (GlStencilFunction.Always, 0, uint.MaxValue);

    /// <summary>Layer: The default value <see cref="ResetStencilMask()"/> restores.</summary>
    public virtual uint DefaultStencilMask => uint.MaxValue;

    /// <summary>Layer: The default value <see cref="ResetStencilOp()"/> restores.</summary>
    public virtual (GlStencilOp Fail, GlStencilOp ZFail, GlStencilOp ZPass) DefaultStencilOp => (GlStencilOp.Keep, GlStencilOp.Keep, GlStencilOp.Keep);
}

namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerStateTest
{
    [TestMethod]
    public void BlendFunc_ThenBlendFuncSeparate_Conflicts()
    {
        gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha);
        Assert.Throws<GlConflictException>(() =>
            gl.BlendFuncSeparate(GlBlendingFactor.One, GlBlendingFactor.Zero, GlBlendingFactor.One, GlBlendingFactor.Zero));
    }

    [TestMethod]
    public void BlendFunc_ThenBlendFunci_Conflicts()
    {
        gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha);
        Assert.Throws<GlConflictException>(() => gl.BlendFunci(0, GlBlendingFactor.One, GlBlendingFactor.Zero));
    }

    [TestMethod]
    public void BlendFunc_AfterResettingSeparate_Succeeds()
    {
        gl.BlendFuncSeparate(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha, GlBlendingFactor.One, GlBlendingFactor.Zero);
        gl.ResetBlendFuncSeparate();
        gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha);
    }

    [TestMethod]
    public void BlendFunci_PerBuffer_ThenReset()
    {
        gl.BlendFunci(0, GlBlendingFactor.One, GlBlendingFactor.Zero);
        gl.BlendFunci(1, GlBlendingFactor.One, GlBlendingFactor.Zero);
        gl.ResetBlendFunc(0);
        Assert.Throws<GlAlreadyUnsetException>(() => gl.ResetBlendFunc(0));
    }

    [TestMethod]
    public void Viewport_ThenViewportIndexed_Conflicts()
    {
        gl.Viewport(0, 0, 800, 600);
        Assert.Throws<GlConflictException>(() => gl.ViewportIndexedf(0, 0, 0, 800, 600));
    }

    [TestMethod]
    public void ColorMask_ThenColorMaski_Conflicts()
    {
        gl.ColorMask(false, false, false, false);
        Assert.Throws<GlConflictException>(() => gl.ColorMaski(0, true, true, true, true));
    }

    [TestMethod]
    public void StencilFunc_ThenStencilFuncSeparate_Conflicts()
    {
        gl.StencilFunc(GlStencilFunction.Always, 1, 0xFF);
        Assert.Throws<GlConflictException>(() => gl.StencilFuncSeparate(GlTriangleFace.Front, GlStencilFunction.Always, 1, 0xFF));
    }

    [TestMethod]
    public void PolygonOffset_ThenClamp_Conflicts()
    {
        gl.PolygonOffset(1f, 1f);
        Assert.Throws<GlConflictException>(() => gl.PolygonOffsetClamp(1f, 1f, 0f));
    }
}

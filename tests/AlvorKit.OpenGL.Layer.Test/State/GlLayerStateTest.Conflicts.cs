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

    /// <summary>Global blend func conflicts with per-buffer separate blend func state.</summary>
    [TestMethod]
    public void BlendFuncSeparatei_ThenBlendFunc_Conflicts()
    {
        gl.BlendFuncSeparatei(0, GlBlendingFactor.One, GlBlendingFactor.Zero, GlBlendingFactor.One, GlBlendingFactor.Zero);

        Assert.Throws<GlConflictException>(() => gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha));
    }

    /// <summary>Per-buffer blend func conflicts with global separate blend func state.</summary>
    [TestMethod]
    public void BlendFuncSeparate_ThenBlendFunci_Conflicts()
    {
        gl.BlendFuncSeparate(GlBlendingFactor.One, GlBlendingFactor.Zero, GlBlendingFactor.One, GlBlendingFactor.Zero);

        Assert.Throws<GlConflictException>(() => gl.BlendFunci(0, GlBlendingFactor.One, GlBlendingFactor.Zero));
    }

    /// <summary>Global separate blend func conflicts with per-buffer blend func state.</summary>
    [TestMethod]
    public void BlendFunci_ThenBlendFuncSeparate_Conflicts()
    {
        gl.BlendFunci(0, GlBlendingFactor.One, GlBlendingFactor.Zero);

        Assert.Throws<GlConflictException>(() =>
            gl.BlendFuncSeparate(GlBlendingFactor.One, GlBlendingFactor.Zero, GlBlendingFactor.One, GlBlendingFactor.Zero));
    }

    /// <summary>Per-buffer separate blend func conflicts with global blend func state.</summary>
    [TestMethod]
    public void BlendFunc_ThenBlendFuncSeparatei_Conflicts()
    {
        gl.BlendFunc(GlBlendingFactor.SrcAlpha, GlBlendingFactor.OneMinusSrcAlpha);

        Assert.Throws<GlConflictException>(() =>
            gl.BlendFuncSeparatei(0, GlBlendingFactor.One, GlBlendingFactor.Zero, GlBlendingFactor.One, GlBlendingFactor.Zero));
    }

    /// <summary>Per-buffer separate blend func conflicts with global separate blend func state.</summary>
    [TestMethod]
    public void BlendFuncSeparate_ThenBlendFuncSeparatei_Conflicts()
    {
        gl.BlendFuncSeparate(GlBlendingFactor.One, GlBlendingFactor.Zero, GlBlendingFactor.One, GlBlendingFactor.Zero);

        Assert.Throws<GlConflictException>(() =>
            gl.BlendFuncSeparatei(0, GlBlendingFactor.One, GlBlendingFactor.Zero, GlBlendingFactor.One, GlBlendingFactor.Zero));
    }

    /// <summary>Per-buffer blend func conflicts with per-buffer separate blend func state.</summary>
    [TestMethod]
    public void BlendFunci_ThenBlendFuncSeparatei_Conflicts()
    {
        gl.BlendFunci(0, GlBlendingFactor.One, GlBlendingFactor.Zero);

        Assert.Throws<GlConflictException>(() =>
            gl.BlendFuncSeparatei(1, GlBlendingFactor.One, GlBlendingFactor.Zero, GlBlendingFactor.One, GlBlendingFactor.Zero));
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

    /// <summary>Blend equation conflicts are enforced between global and per-buffer separate state.</summary>
    [TestMethod]
    public void BlendEquationSeparatei_ThenBlendEquation_Conflicts()
    {
        gl.BlendEquationSeparatei(0, GlBlendEquationModeEXT.FuncAdd, GlBlendEquationModeEXT.FuncReverseSubtract);

        Assert.Throws<GlConflictException>(() => gl.BlendEquation(GlBlendEquationModeEXT.FuncSubtract));
    }

    /// <summary>Per-buffer separate blend equation conflicts with global blend equation state.</summary>
    [TestMethod]
    public void BlendEquation_ThenBlendEquationSeparatei_Conflicts()
    {
        gl.BlendEquation(GlBlendEquationModeEXT.FuncAdd);

        Assert.Throws<GlConflictException>(() =>
            gl.BlendEquationSeparatei(0, GlBlendEquationModeEXT.FuncAdd, GlBlendEquationModeEXT.FuncReverseSubtract));
    }

    /// <summary>Per-buffer separate blend equation conflicts with global separate blend equation state.</summary>
    [TestMethod]
    public void BlendEquationSeparate_ThenBlendEquationSeparatei_Conflicts()
    {
        gl.BlendEquationSeparate(GlBlendEquationModeEXT.FuncAdd, GlBlendEquationModeEXT.FuncReverseSubtract);

        Assert.Throws<GlConflictException>(() =>
            gl.BlendEquationSeparatei(0, GlBlendEquationModeEXT.FuncAdd, GlBlendEquationModeEXT.FuncReverseSubtract));
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

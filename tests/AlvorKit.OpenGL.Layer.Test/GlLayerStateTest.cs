namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class GlLayerStateTest
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup() => gl = new GlLayer(new GlNoop());

    [TestMethod]
    public void Enable_ThenDisable_Succeeds()
    {
        gl.Enable(GlEnableCap.Blend);
        gl.Disable(GlEnableCap.Blend);
    }

    [TestMethod]
    public void Enable_WhenAlreadyEnabled_Throws()
    {
        gl.Enable(GlEnableCap.Blend);
        Assert.Throws<GlAlreadySetException>(() => gl.Enable(GlEnableCap.Blend));
    }

    [TestMethod]
    public void Disable_WhenNotEnabled_Throws()
    {
        Assert.Throws<GlAlreadyUnsetException>(() => gl.Disable(GlEnableCap.Blend));
    }

    [TestMethod]
    public void Enable_DifferentCaps_DoNotConflict()
    {
        gl.Enable(GlEnableCap.Blend);
        gl.Enable(GlEnableCap.DepthTest);
    }

    [TestMethod]
    public void IndexedEnable_PairsIndependently()
    {
        gl.Enablei(GlEnableCap.Blend, 0);
        gl.Enablei(GlEnableCap.Blend, 1);
        gl.Disablei(GlEnableCap.Blend, 0);
        Assert.Throws<GlAlreadyUnsetException>(() => gl.Disablei(GlEnableCap.Blend, 0));
    }

    [TestMethod]
    public void ValueSetter_SetThenReset_Succeeds()
    {
        gl.ClearColor(0.1f, 0.2f, 0.3f, 1f);
        gl.ResetClearColor();
    }

    [TestMethod]
    public void ValueSetter_SetTwice_Throws()
    {
        gl.ClearColor(0.1f, 0.2f, 0.3f, 1f);
        Assert.Throws<GlAlreadySetException>(() => gl.ClearColor(0.4f, 0.5f, 0.6f, 1f));
    }

    [TestMethod]
    public void ValueSetter_SetAfterReset_Succeeds()
    {
        gl.ClearColor(0.1f, 0.2f, 0.3f, 1f);
        gl.ResetClearColor();
        gl.ClearColor(0.4f, 0.5f, 0.6f, 1f);
    }

    [TestMethod]
    public void Reset_WhenNotSet_Throws()
    {
        Assert.Throws<GlAlreadyUnsetException>(() => gl.ResetClearColor());
    }

    [TestMethod]
    public void Viewport_SetResetSet_Succeeds()
    {
        gl.Viewport(0, 0, 800, 600);
        gl.ResetViewport();
        gl.Viewport(0, 0, 1024, 768);
    }

    [TestMethod]
    public void DepthMask_SetResetSet()
    {
        gl.DepthMask(false);
        gl.ResetDepthMask();
        gl.DepthMask(true);
    }

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

    [TestMethod]
    public void Hint_PerTarget_SetThenReset()
    {
        gl.Hint(GlHintTarget.LineSmoothHint, GlHintMode.Nicest);
        gl.ResetHint(GlHintTarget.LineSmoothHint);
        Assert.Throws<GlAlreadyUnsetException>(() => gl.ResetHint(GlHintTarget.LineSmoothHint));
    }

    [TestMethod]
    public void PixelStore_SetThenReset()
    {
        gl.PixelStorei(GlPixelStoreParameter.UnpackAlignment, 1);
        Assert.Throws<GlAlreadySetException>(() => gl.PixelStorei(GlPixelStoreParameter.UnpackAlignment, 2));
        gl.ResetPixelStore(GlPixelStoreParameter.UnpackAlignment);
        gl.PixelStorei(GlPixelStoreParameter.UnpackAlignment, 4);
    }

    [TestMethod]
    public void SampleMask_PerWord_SetThenReset()
    {
        gl.SampleMaski(0, 0x0F);
        gl.ResetSampleMask(0);
        gl.SampleMaski(0, uint.MaxValue);
    }
}

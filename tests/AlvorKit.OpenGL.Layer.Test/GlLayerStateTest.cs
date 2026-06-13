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
        gl.Enable(EnableCap.Blend);
        gl.Disable(EnableCap.Blend);
    }

    [TestMethod]
    public void Enable_WhenAlreadyEnabled_Throws()
    {
        gl.Enable(EnableCap.Blend);
        Assert.Throws<GlAlreadySetException>(() => gl.Enable(EnableCap.Blend));
    }

    [TestMethod]
    public void Disable_WhenNotEnabled_Throws()
    {
        Assert.Throws<GlAlreadyUnsetException>(() => gl.Disable(EnableCap.Blend));
    }

    [TestMethod]
    public void Enable_DifferentCaps_DoNotConflict()
    {
        gl.Enable(EnableCap.Blend);
        gl.Enable(EnableCap.DepthTest);
    }

    [TestMethod]
    public void IndexedEnable_PairsIndependently()
    {
        gl.Enablei(EnableCap.Blend, 0);
        gl.Enablei(EnableCap.Blend, 1);
        gl.Disablei(EnableCap.Blend, 0);
        Assert.Throws<GlAlreadyUnsetException>(() => gl.Disablei(EnableCap.Blend, 0));
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
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Assert.Throws<GlConflictException>(() =>
            gl.BlendFuncSeparate(BlendingFactor.One, BlendingFactor.Zero, BlendingFactor.One, BlendingFactor.Zero));
    }

    [TestMethod]
    public void BlendFunc_ThenBlendFunci_Conflicts()
    {
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Assert.Throws<GlConflictException>(() => gl.BlendFunci(0, BlendingFactor.One, BlendingFactor.Zero));
    }

    [TestMethod]
    public void BlendFunc_AfterResettingSeparate_Succeeds()
    {
        gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.One, BlendingFactor.Zero);
        gl.ResetBlendFuncSeparate();
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    [TestMethod]
    public void BlendFunci_PerBuffer_ThenReset()
    {
        gl.BlendFunci(0, BlendingFactor.One, BlendingFactor.Zero);
        gl.BlendFunci(1, BlendingFactor.One, BlendingFactor.Zero);
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
        gl.StencilFunc(StencilFunction.Always, 1, 0xFF);
        Assert.Throws<GlConflictException>(() => gl.StencilFuncSeparate(TriangleFace.Front, StencilFunction.Always, 1, 0xFF));
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
        gl.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
        gl.ResetHint(HintTarget.LineSmoothHint);
        Assert.Throws<GlAlreadyUnsetException>(() => gl.ResetHint(HintTarget.LineSmoothHint));
    }

    [TestMethod]
    public void PixelStore_SetThenReset()
    {
        gl.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);
        Assert.Throws<GlAlreadySetException>(() => gl.PixelStorei(PixelStoreParameter.UnpackAlignment, 2));
        gl.ResetPixelStore(PixelStoreParameter.UnpackAlignment);
        gl.PixelStorei(PixelStoreParameter.UnpackAlignment, 4);
    }

    [TestMethod]
    public void SampleMask_PerWord_SetThenReset()
    {
        gl.SampleMaski(0, 0x0F);
        gl.ResetSampleMask(0);
        gl.SampleMaski(0, uint.MaxValue);
    }
}

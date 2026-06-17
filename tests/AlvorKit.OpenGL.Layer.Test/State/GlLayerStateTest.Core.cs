namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerStateTest
{
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

    /// <summary>A failed viewport-array reset leaves earlier viewport indices recorded.</summary>
    [TestMethod]
    public void ResetViewportArray_WhenLaterIndexUnset_DoesNotResetEarlierIndex()
    {
        gl.ViewportIndexedf(0, 0, 0, 800, 600);

        Assert.Throws<GlAlreadyUnsetException>(() => gl.ResetViewportArray(0, 2));

        Assert.Throws<GlAlreadySetException>(() => gl.ViewportIndexedf(0, 0, 0, 1024, 768));
    }
}

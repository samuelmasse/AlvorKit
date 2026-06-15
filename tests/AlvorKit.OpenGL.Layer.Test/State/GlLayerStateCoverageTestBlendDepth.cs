namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Exercises blend, color, and depth state families not covered by targeted invariant tests.
/// </summary>
[TestClass]
public unsafe class GlLayerStateCoverageTestBlendDepth
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup() => gl = new GlLayer(new GlNoop());

    [TestMethod]
    public void BlendAndColorState_SetThenReset()
    {
        gl.BlendFuncSeparatei(0, GlBlendingFactor.One, GlBlendingFactor.Zero, GlBlendingFactor.One, GlBlendingFactor.Zero);
        gl.ResetBlendFuncSeparate(0);
        gl.BlendFunc(GlBlendingFactor.One, GlBlendingFactor.Zero);
        gl.ResetBlendFunc();
        gl.BlendEquation(GlBlendEquationModeEXT.FuncSubtract);
        gl.ResetBlendEquation();
        gl.BlendEquationSeparate(GlBlendEquationModeEXT.FuncAdd, GlBlendEquationModeEXT.FuncReverseSubtract);
        gl.ResetBlendEquationSeparate();
        gl.BlendEquationSeparatei(0, GlBlendEquationModeEXT.FuncAdd, GlBlendEquationModeEXT.FuncReverseSubtract);
        gl.ResetBlendEquationSeparate(0);
        gl.BlendColor(0.1f, 0.2f, 0.3f, 0.4f);
        gl.ResetBlendColor();
        gl.ClampColor(GlClampColorTarget.ClampReadColor, GlClampColorMode.True);
        gl.ResetClampColor();
        gl.ColorMaski(0, true, false, true, false);
        gl.ResetColorMask(0);
        gl.ColorMask(false, true, false, true);
        gl.ResetColorMask();
    }

    [TestMethod]
    public void DepthState_SetThenReset()
    {
        gl.DepthFunc(GlDepthFunction.Always);
        gl.ResetDepthFunc();
        gl.ClearDepth(0.5);
        gl.ResetClearDepth();
        gl.ClearDepthf(0.25f);
        gl.ResetClearDepth();
        gl.ClearStencil(1);
        gl.ResetClearStencil();
        gl.DepthRange(0.1, 0.9);
        gl.ResetDepthRange();
        gl.DepthRangef(0.2f, 0.8f);
        gl.ResetDepthRange();
        gl.DepthRangeIndexed(0, 0.2, 0.8);
        gl.ResetDepthRangeIndexed(0);

        double* ranges = stackalloc double[] { 0.0, 1.0, 0.1, 0.9 };
        gl.DepthRangeArrayv(0, 2, (nint)ranges);
        gl.ResetDepthRangeArray(0, 2);
    }

    [TestMethod]
    public void Clear_StencilAndIndexedScissorPrerequisites_Throw()
    {
        Assert.Throws<GlMissingPrerequisiteException>(() => gl.Clear(GlClearBufferMask.StencilBufferBit));

        gl.ClearColor(0, 0, 0, 1);
        gl.Enablei(GlEnableCap.ScissorTest, 0);

        Assert.Throws<GlMissingPrerequisiteException>(() => gl.Clear(GlClearBufferMask.ColorBufferBit));
    }
}

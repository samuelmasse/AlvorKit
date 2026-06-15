namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerStateTest
{
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

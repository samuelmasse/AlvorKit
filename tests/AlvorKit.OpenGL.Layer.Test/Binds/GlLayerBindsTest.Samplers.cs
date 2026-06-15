namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerBindsTest
{
    [TestMethod]
    public void BindSampler_ThenUnbind_Succeeds()
    {
        gl.BindSampler(0, Sampler(1));
        gl.UnbindSampler(0);
    }

    [TestMethod]
    public void BindSamplers_ThenUnbind_Succeeds()
    {
        gl.BindSamplers(0, [Sampler(1), Sampler(2), Sampler(3)]);
        gl.UnbindSamplers(0, 3);
    }

    [TestMethod]
    public void BindSamplers_OverLiveSingularBind_Throws()
    {
        gl.BindSampler(1, Sampler(7));
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindSamplers(0, [Sampler(4), Sampler(5), Sampler(6)]));
    }

    [TestMethod]
    public void UnbindSamplers_WhenNothingBound_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnbindSamplers(0, 2));
    }
}

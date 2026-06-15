namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerBindsTest
{
    [TestMethod]
    public void UseProgram_ThenUnuse_Succeeds()
    {
        gl.UseProgram(Program(1));
        gl.UnuseProgram();
    }

    [TestMethod]
    public void UseProgram_OverLiveProgram_Throws()
    {
        gl.UseProgram(Program(1));
        Assert.Throws<GlAlreadyBoundException>(() => gl.UseProgram(Program(2)));
    }

    [TestMethod]
    public void UnuseProgram_WhenNothingUsed_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnuseProgram());
    }

    [TestMethod]
    public void Query_BeginThenEnd_Succeeds()
    {
        gl.BeginQuery(GlQueryTarget.SamplesPassed, Query(1));
        gl.EndQuery(GlQueryTarget.SamplesPassed);
    }

    [TestMethod]
    public void Query_BeginWhileActive_Throws()
    {
        gl.BeginQuery(GlQueryTarget.SamplesPassed, Query(1));
        Assert.Throws<GlAlreadyBoundException>(() => gl.BeginQuery(GlQueryTarget.SamplesPassed, Query(2)));
    }

    [TestMethod]
    public void Query_EndWithoutBegin_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.EndQuery(GlQueryTarget.SamplesPassed));
    }

    [TestMethod]
    public void ConditionalRender_BeginThenEnd_Succeeds()
    {
        gl.BeginConditionalRender(1, GlConditionalRenderMode.QueryWait);
        gl.EndConditionalRender();
    }

    [TestMethod]
    public void ConditionalRender_EndWithoutBegin_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.EndConditionalRender());
    }
}

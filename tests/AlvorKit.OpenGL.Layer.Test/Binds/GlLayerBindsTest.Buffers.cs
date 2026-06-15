namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerBindsTest
{
    [TestMethod]
    public void BindBuffer_ThenUnbind_Succeeds()
    {
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, Buffer(1));
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
    }

    [TestMethod]
    public void BindBuffer_OverLiveBinding_Throws()
    {
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, Buffer(1));
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBuffer(GlBufferTarget.ArrayBuffer, Buffer(2)));
    }

    [TestMethod]
    public void BindZero_OverLiveBinding_Throws()
    {
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, Buffer(1));
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBuffer(GlBufferTarget.ArrayBuffer, Buffer(0)));
    }

    [TestMethod]
    public void UnbindBuffer_WhenNothingBound_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnbindBuffer(GlBufferTarget.ArrayBuffer));
    }

    [TestMethod]
    public void BindBuffer_DifferentTargets_DoNotConflict()
    {
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, Buffer(1));
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, Buffer(2));
    }

    [TestMethod]
    public void BindBuffersBase_ThenUnbind_Succeeds()
    {
        gl.BindBuffersBase(GlBufferTarget.UniformBuffer, 0, [Buffer(1), Buffer(2)]);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.UnbindBuffersBase(GlBufferTarget.UniformBuffer, 0, 2);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
    }

    [TestMethod]
    public void BindBufferBase_TwoIndicesWithoutGenericUnbind_Throws()
    {
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 0, Buffer(1));
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBufferBase(GlBufferTarget.UniformBuffer, 1, Buffer(2)));
    }

    [TestMethod]
    public void BindBufferBase_TwoIndicesWithGenericUnbind_Succeeds()
    {
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 0, Buffer(1));
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 1, Buffer(2));
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.UnbindBufferBase(GlBufferTarget.UniformBuffer, 0);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.UnbindBufferBase(GlBufferTarget.UniformBuffer, 1);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
    }

    [TestMethod]
    public void BindBuffersBase_OverLiveSingularBind_Throws()
    {
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 0, Buffer(9));
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBuffersBase(GlBufferTarget.UniformBuffer, 0, [Buffer(1), Buffer(2)]));
    }
}

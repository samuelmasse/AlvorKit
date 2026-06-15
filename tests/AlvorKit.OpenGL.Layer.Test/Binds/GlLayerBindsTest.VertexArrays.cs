namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerBindsTest
{
    [TestMethod]
    public void BindVertexArray_WhenVboBound_Throws()
    {
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, Buffer(2));
        Assert.Throws<GlBindConflictException>(() => gl.BindVertexArray(VertexArray(1)));
    }

    [TestMethod]
    public void BindVertexArray_WhenEboBound_Throws()
    {
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, Buffer(2));
        Assert.Throws<GlBindConflictException>(() => gl.BindVertexArray(VertexArray(1)));
    }

    [TestMethod]
    public void BindVertexArray_WhenVertexBufferBound_Throws()
    {
        gl.BindVertexBuffer(0, Buffer(2), 0, 16);
        Assert.Throws<GlBindConflictException>(() => gl.BindVertexArray(VertexArray(1)));
    }

    [TestMethod]
    public void BindVertexBuffer_ThenUnbind_Succeeds()
    {
        gl.BindVertexBuffer(0, Buffer(2), 0, 16);
        gl.UnbindVertexBuffer(0);
    }

    [TestMethod]
    public void BindVertexArray_ThenUnbind_Succeeds()
    {
        gl.BindVertexArray(VertexArray(1));
        gl.UnbindVertexArray();
    }

    [TestMethod]
    public void UnbindVertexArray_WhenArrayBufferBound_Throws()
    {
        gl.BindVertexArray(VertexArray(1));
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, Buffer(2));
        Assert.Throws<GlBindConflictException>(() => gl.UnbindVertexArray());
    }

    [TestMethod]
    public void UnbindVertexArray_ClearsVaoOwnedBufferBindings()
    {
        gl.BindVertexArray(VertexArray(1));
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, Buffer(2));
        gl.BindVertexBuffer(0, Buffer(3), 0, 16);

        gl.UnbindVertexArray();

        Assert.Throws<GlNotBoundException>(() => gl.UnbindBuffer(GlBufferTarget.ElementArrayBuffer));
        Assert.Throws<GlNotBoundException>(() => gl.UnbindVertexBuffer(0));
    }

    [TestMethod]
    public void UnbindElementArrayBuffer_WhenVaoBound_Throws()
    {
        gl.BindVertexArray(VertexArray(1));
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, Buffer(2));

        Assert.Throws<GlBindConflictException>(() => gl.UnbindBuffer(GlBufferTarget.ElementArrayBuffer));
    }
}

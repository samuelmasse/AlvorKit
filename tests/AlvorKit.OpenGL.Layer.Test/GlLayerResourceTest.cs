namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class GlLayerResourceTest
{
    [TestMethod]
    public void Dispose_DeletesEveryAllocatedObject()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);

        var buffer = gl.GenBuffer();
        var texture = gl.GenTexture();
        var vao = gl.GenVertexArray();
        var shader = gl.CreateShader(GlShaderType.VertexShader);
        var program = gl.CreateProgram();

        gl.Dispose();

        CollectionAssert.AreEquivalent(new[] { (uint)buffer, (uint)texture, (uint)vao, (uint)shader, (uint)program }, inner.Deleted);
    }

    [TestMethod]
    public void Dispose_AfterManualDelete_DoesNotDeleteAgain()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);

        var buffer = gl.GenBuffer();
        gl.DeleteBuffer(buffer);
        gl.Dispose();

        Assert.AreEqual(1, inner.Deleted.Count(id => id == (uint)buffer));
    }

    [TestMethod]
    public void GenAndDelete_ReflectedInLiveSet()
    {
        var gl = new GlLayer(new RecordingGl());

        var buffer = gl.GenBuffer();
        Assert.IsTrue(gl.Buffers.Contains(buffer));

        gl.DeleteBuffer(buffer);
        Assert.IsFalse(gl.Buffers.Contains(buffer));
    }

    [TestMethod]
    public void DeleteUntrackedBuffer_Throws()
    {
        var gl = new GlLayer(new RecordingGl());
        Assert.Throws<GlResourceNotTrackedException<GlBufferHandle>>(() => gl.DeleteBuffer((GlBufferHandle)123u));
    }

    [TestMethod]
    public void DeleteUntrackedTexture_Throws()
    {
        var gl = new GlLayer(new RecordingGl());
        Assert.Throws<GlResourceNotTrackedException<GlTextureHandle>>(() => gl.DeleteTexture((GlTextureHandle)123u));
    }

    [TestMethod]
    public void Dispose_WhenNothingAllocated_DeletesNothing()
    {
        var inner = new RecordingGl();
        new GlLayer(inner).Dispose();
        Assert.AreEqual(0, inner.Deleted.Count);
    }
}

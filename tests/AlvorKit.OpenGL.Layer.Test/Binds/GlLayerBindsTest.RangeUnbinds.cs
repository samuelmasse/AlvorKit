namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerBindsTest
{
    /// <summary>Range buffer-base unbinds forward zero object ids to the backend.</summary>
    [TestMethod]
    public void UnbindBuffersBase_ForwardsZeroIds()
    {
        var inner = new RecordingGl();
        var layer = new GlLayer(inner);

        layer.BindBuffersBase(GlBufferTarget.UniformBuffer, 0, [Buffer(1), Buffer(2)]);
        layer.UnbindBuffer(GlBufferTarget.UniformBuffer);
        layer.UnbindBuffersBase(GlBufferTarget.UniformBuffer, 0, 2);

        CollectionAssert.AreEqual(new uint[] { 0, 0 }, inner.LastBindBuffersBaseBuffers);
    }

    /// <summary>Range buffer unbinds forward zero object ids, offsets, and sizes to the backend.</summary>
    [TestMethod]
    public void UnbindBuffersRange_ForwardsZeroRanges()
    {
        var inner = new RecordingGl();
        var layer = new GlLayer(inner);

        layer.BindBuffersRange(GlBufferTarget.UniformBuffer, 0, [Buffer(1), Buffer(2)], [4, 8], [16, 32]);
        layer.UnbindBuffer(GlBufferTarget.UniformBuffer);
        layer.UnbindBuffersRange(GlBufferTarget.UniformBuffer, 0, 2);

        CollectionAssert.AreEqual(new uint[] { 0, 0 }, inner.LastBindBuffersRangeBuffers);
        CollectionAssert.AreEqual(new nint[] { 0, 0 }, inner.LastBindBuffersRangeOffsets);
        CollectionAssert.AreEqual(new nint[] { 0, 0 }, inner.LastBindBuffersRangeSizes);
    }

    /// <summary>Range vertex-buffer unbinds forward zero object ids, offsets, and strides to the backend.</summary>
    [TestMethod]
    public void UnbindVertexBuffers_ForwardsZeroRanges()
    {
        var inner = new RecordingGl();
        var layer = new GlLayer(inner);

        layer.BindVertexBuffers(0, [Buffer(1), Buffer(2)], [4, 8], [16, 32]);
        layer.UnbindVertexBuffers(0, 2);

        CollectionAssert.AreEqual(new uint[] { 0, 0 }, inner.LastBindVertexBuffersBuffers);
        CollectionAssert.AreEqual(new nint[] { 0, 0 }, inner.LastBindVertexBuffersOffsets);
        CollectionAssert.AreEqual(new int[] { 0, 0 }, inner.LastBindVertexBuffersStrides);
    }

    /// <summary>Range sampler unbinds forward zero object ids to the backend.</summary>
    [TestMethod]
    public void UnbindSamplers_ForwardsZeroIds()
    {
        var inner = new RecordingGl();
        var layer = new GlLayer(inner);

        layer.BindSamplers(0, [Sampler(1), Sampler(2)]);
        layer.UnbindSamplers(0, 2);

        CollectionAssert.AreEqual(new uint[] { 0, 0 }, inner.LastBindSamplers);
    }

    /// <summary>Range image-texture unbinds forward zero object ids to the backend.</summary>
    [TestMethod]
    public void UnbindImageTextures_ForwardsZeroIds()
    {
        var inner = new RecordingGl();
        var layer = new GlLayer(inner);

        layer.BindImageTextures(0, [Texture(1), Texture(2)]);
        layer.UnbindImageTextures(0, 2);

        CollectionAssert.AreEqual(new uint[] { 0, 0 }, inner.LastBindImageTextures);
    }

    /// <summary>Range texture unbinds forward zero object ids to the backend.</summary>
    [TestMethod]
    public void UnbindTextures_ForwardsZeroIds()
    {
        var inner = new RecordingGl();
        var layer = new GlLayer(inner);

        TrackTextureTarget(layer, Texture(1), GlTextureTarget.Texture2D);
        TrackTextureTarget(layer, Texture(2), GlTextureTarget.Texture2D);
        layer.BindTextures(0, [Texture(1), Texture(2)]);
        layer.UnbindTextures(0, 2);

        CollectionAssert.AreEqual(new uint[] { 0, 0 }, inner.LastBindTextures);
    }

    private static void TrackTextureTarget(GlLayer layer, GlTextureHandle texture, GlTextureTarget target)
    {
        layer.ActiveTexture(GlTextureUnit.Texture0);
        layer.BindTexture(target, texture);
        layer.UnbindTexture(target);
        layer.ResetActiveTexture();
    }
}

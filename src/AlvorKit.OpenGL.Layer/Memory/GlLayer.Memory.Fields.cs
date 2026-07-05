namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <summary>Layer: total bytes of buffer storage allocated and not yet deleted.</summary>
    public long BufferUsage => state.bufferUsage;

    /// <summary>Layer: total bytes of texture storage allocated and not yet deleted.</summary>
    public long TextureUsage => state.textureUsage;

    /// <summary>Layer: total bytes of renderbuffer storage allocated and not yet deleted.</summary>
    public long RenderbufferUsage => state.renderbufferUsage;

    /// <summary>Layer: the last recorded byte size of each live buffer.</summary>
    public IReadOnlyDictionary<GlBufferHandle, long> BufferSizes => state.bufferSizes;

    /// <summary>Layer: the aggregate recorded shape of each live texture.</summary>
    public IReadOnlyDictionary<GlTextureHandle, GlTextureInfo> TextureSizes => state.textureSizes;

    /// <summary>Layer: the recorded storage shape of each live texture level.</summary>
    public IReadOnlyDictionary<(GlTextureHandle Texture, int Level), GlTextureInfo> TextureLevelSizes => state.textureLevelSizes;

    /// <summary>Layer: the last recorded shape of each live renderbuffer.</summary>
    public IReadOnlyDictionary<GlRenderbufferHandle, GlRenderbufferInfo> RenderbufferSizes => state.renderbufferSizes;
}

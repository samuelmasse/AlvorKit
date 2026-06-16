namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <summary>Tracks the last recorded byte size for each live buffer.</summary>
    private readonly Dictionary<GlBufferHandle, long> bufferSizes = [];

    /// <summary>Stores the total tracked bytes allocated to live buffers.</summary>
    private long bufferUsage;

    /// <summary>Tracks the aggregate recorded texture shape for each live texture.</summary>
    private readonly Dictionary<GlTextureHandle, GlTextureInfo> textureSizes = [];

    /// <summary>Tracks recorded texture storage by texture handle and mip level.</summary>
    private readonly Dictionary<(GlTextureHandle Texture, int Level), GlTextureInfo> textureLevelSizes = [];

    /// <summary>Stores the total tracked bytes allocated to live textures.</summary>
    private long textureUsage;

    /// <summary>Tracks the last recorded renderbuffer shape for each live renderbuffer.</summary>
    private readonly Dictionary<GlRenderbufferHandle, GlRenderbufferInfo> renderbufferSizes = [];

    /// <summary>Stores the total tracked bytes allocated to live renderbuffers.</summary>
    private long renderbufferUsage;

    /// <summary>Layer: total bytes of buffer storage allocated and not yet deleted.</summary>
    public long BufferUsage => bufferUsage;

    /// <summary>Layer: total bytes of texture storage allocated and not yet deleted.</summary>
    public long TextureUsage => textureUsage;

    /// <summary>Layer: total bytes of renderbuffer storage allocated and not yet deleted.</summary>
    public long RenderbufferUsage => renderbufferUsage;

    /// <summary>Layer: the last recorded byte size of each live buffer.</summary>
    public IReadOnlyDictionary<GlBufferHandle, long> BufferSizes => bufferSizes;

    /// <summary>Layer: the aggregate recorded shape of each live texture.</summary>
    public IReadOnlyDictionary<GlTextureHandle, GlTextureInfo> TextureSizes => textureSizes;

    /// <summary>Layer: the recorded storage shape of each live texture level.</summary>
    public IReadOnlyDictionary<(GlTextureHandle Texture, int Level), GlTextureInfo> TextureLevelSizes => textureLevelSizes;

    /// <summary>Layer: the last recorded shape of each live renderbuffer.</summary>
    public IReadOnlyDictionary<GlRenderbufferHandle, GlRenderbufferInfo> RenderbufferSizes => renderbufferSizes;
}

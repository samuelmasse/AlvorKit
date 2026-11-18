namespace AlvorKit;

/// <summary>Loads and caches textures owned by the supplied GL layer.</summary>
[Root]
public class RootTextures
{
    private readonly RootPngs pngs;
    private readonly RootFiles files;
    private readonly GlLayer gl;
    private readonly Dictionary<string, Texture2D> textures = [];

    public Texture2D this[string path]
    {
        get
        {
            if (textures.TryGetValue(path, out var texture))
                return texture;

            using var stream = files.OpenRead(path);
            var image = pngs.Read(stream);
            texture = new Texture2D(gl, (Vec2u)image.Size)
            {
                Pixels = image.Pixels.Span,
                MagFilter = GlTextureMagFilter.Nearest,
                MinFilter = GlTextureMinFilter.Nearest,
                WrapS = GlTextureWrapMode.ClampToEdge,
                WrapT = GlTextureWrapMode.ClampToEdge,
            };
            textures.Add(path, texture);
            return texture;
        }
    }

    public RootTextures(RootPngs pngs, RootFiles files, RootGl gl) : this(pngs, files, (GlLayer)gl) { }

    protected RootTextures(RootPngs pngs, RootFiles files, GlLayer gl)
    {
        this.pngs = pngs;
        this.files = files;
        this.gl = gl;
    }
}

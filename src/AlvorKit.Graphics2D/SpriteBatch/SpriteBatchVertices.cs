namespace AlvorKit.Graphics2D;

/// <summary>Collects sprite vertices and splits them into texture-slot-compatible sections.</summary>
internal class SpriteBatchVertices(int textureSlots)
{
    /// <summary>The pending draw sections.</summary>
    private readonly List<SpriteBatchSection> sections = [];

    /// <summary>The pending interleaved vertices.</summary>
    private readonly List<SpriteBatchVertex> vertices = [];

    /// <summary>The texture-to-slot map for the current section.</summary>
    private readonly Dictionary<Texture, int> slotMap = [];

    /// <summary>The textures referenced by all pending sections.</summary>
    private readonly List<Texture> textures = [];

    /// <summary>Gets a span over the pending vertices.</summary>
    internal ReadOnlySpan<SpriteBatchVertex> Vertices => CollectionsMarshal.AsSpan(vertices);

    /// <summary>Gets a span over the pending sections.</summary>
    internal ReadOnlySpan<SpriteBatchSection> Sections => CollectionsMarshal.AsSpan(sections);

    /// <summary>Gets the number of pending vertices.</summary>
    internal int VertexCount => vertices.Count;

    /// <summary>Gets the current section by reference.</summary>
    private ref SpriteBatchSection Section => ref CollectionsMarshal.AsSpan(sections)[^1];

    /// <summary>Adds one vertex for the supplied texture, assigning a texture slot for its section.</summary>
    internal void Add(Texture texture, SpriteBatchVertex vertex)
    {
        if (sections.Count == 0)
            StartNewSection();

        if (!slotMap.TryGetValue(texture, out var slot))
        {
            if (Section.TextureCount >= textureSlots)
                StartNewSection();

            slot = Section.TextureCount++;
            textures.Add(texture);
            slotMap[texture] = slot;
        }

        vertex.TexIndex = slot;
        vertices.Add(vertex);
        Section.Count++;
    }

    /// <summary>Gets the textures used by one draw section.</summary>
    internal ReadOnlySpan<Texture> SectionTextures(int sectionIndex) =>
        CollectionsMarshal.AsSpan(textures).Slice(Sections[sectionIndex].TextureStart, Sections[sectionIndex].TextureCount);

    /// <summary>Starts a new draw section and clears the section-local texture slot map.</summary>
    private void StartNewSection()
    {
        slotMap.Clear();
        sections.Add(new SpriteBatchSection { TextureStart = textures.Count });
    }

    /// <summary>Clears all pending vertices, sections, textures, and texture slots.</summary>
    internal void Reset()
    {
        sections.Clear();
        vertices.Clear();
        textures.Clear();
        slotMap.Clear();
    }
}

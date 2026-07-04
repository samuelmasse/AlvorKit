namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Owns a parsed GLB 2.0 document and exposes small helpers for the demo loader.</summary>
internal sealed class GlbDocument : IDisposable
{
    /// <summary>The GLB magic number for the ASCII text <c>glTF</c>.</summary>
    private const uint GlbMagic = 0x46546C67;

    /// <summary>The chunk type for the GLB JSON chunk.</summary>
    private const uint JsonChunkType = 0x4E4F534A;

    /// <summary>The chunk type for the GLB binary chunk.</summary>
    private const uint BinaryChunkType = 0x004E4942;

    /// <summary>The complete GLB file bytes, retained so spans into the binary chunk stay valid.</summary>
    private readonly byte[] bytes;

    /// <summary>The parsed JSON chunk from the GLB file.</summary>
    private readonly JsonDocument json;

    /// <summary>The byte offset of the binary chunk inside <see cref="bytes"/>.</summary>
    private readonly int binaryOffset;

    /// <summary>The byte length of the binary chunk inside <see cref="bytes"/>.</summary>
    private readonly int binaryLength;

    /// <summary>Stores the parsed JSON document and binary chunk bounds for accessor reads.</summary>
    private GlbDocument(byte[] bytes, JsonDocument json, int binaryOffset, int binaryLength)
    {
        this.bytes = bytes;
        this.json = json;
        this.binaryOffset = binaryOffset;
        this.binaryLength = binaryLength;
    }

    /// <summary>Gets the root JSON element from the GLB JSON chunk.</summary>
    public JsonElement Root => json.RootElement;

    /// <summary>Gets the GLB binary chunk used by buffer views and accessors.</summary>
    public ReadOnlySpan<byte> Binary => bytes.AsSpan(binaryOffset, binaryLength);

    /// <summary>Opens a GLB 2.0 file and validates that it contains JSON and binary chunks.</summary>
    public static GlbDocument Open(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 20)
            throw new FormatException("The GLB file is too small to contain a valid header.");

        var magic = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(0, 4));
        var version = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4, 4));
        var declaredLength = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(8, 4));

        if (magic != GlbMagic || version != 2 || declaredLength != bytes.Length)
            throw new FormatException("The file is not a valid GLB 2.0 asset.");

        var offset = 12;
        string? jsonText = null;
        var binaryOffset = -1;
        var binaryLength = 0;

        while (offset + 8 <= bytes.Length)
        {
            var chunkLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4)));
            var chunkType = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset + 4, 4));
            var chunkOffset = offset + 8;

            if (chunkOffset + chunkLength > bytes.Length)
                throw new FormatException("The GLB chunk table extends past the end of the file.");

            if (chunkType == JsonChunkType)
                jsonText = Encoding.UTF8.GetString(bytes, chunkOffset, chunkLength).TrimEnd('\0', ' ');
            else if (chunkType == BinaryChunkType)
                (binaryOffset, binaryLength) = (chunkOffset, chunkLength);

            offset = chunkOffset + chunkLength;
        }

        if (jsonText is null || binaryOffset < 0)
            throw new FormatException("The GLB must contain both JSON and binary chunks.");

        return new GlbDocument(bytes, JsonDocument.Parse(jsonText), binaryOffset, binaryLength);
    }

    /// <summary>Reads a required JSON object property or throws with the missing property name.</summary>
    public static JsonElement RequiredProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
            return property;

        throw new FormatException($"The GLB JSON is missing required property '{propertyName}'.");
    }

    /// <summary>Reads a required JSON array element or throws with the source array name.</summary>
    public static JsonElement RequiredArrayElement(JsonElement array, int index, string arrayName)
    {
        if (array.ValueKind != JsonValueKind.Array || index < 0 || index >= array.GetArrayLength())
            throw new FormatException($"The GLB JSON does not contain {arrayName}[{index}].");

        return array[index];
    }

    /// <summary>Reads an optional integer JSON property and returns the supplied default when it is absent.</summary>
    public static int GetInt32OrDefault(JsonElement element, string propertyName, int defaultValue) =>
        element.TryGetProperty(propertyName, out var property) ? property.GetInt32() : defaultValue;

    /// <summary>Reads an optional string JSON property and returns the supplied default when it is absent.</summary>
    public static string GetStringOrDefault(JsonElement element, string propertyName, string defaultValue) =>
        element.TryGetProperty(propertyName, out var property) ? property.GetString() ?? defaultValue : defaultValue;

    /// <summary>Reads one accessor JSON element by index.</summary>
    public JsonElement Accessor(int index) => RequiredArrayElement(RequiredProperty(Root, "accessors"), index, "accessors");

    /// <summary>Reads one buffer view JSON element by index.</summary>
    public JsonElement BufferView(int index) => RequiredArrayElement(RequiredProperty(Root, "bufferViews"), index, "bufferViews");

    /// <summary>Reads a buffer view from the binary chunk as an immutable byte span.</summary>
    public ReadOnlySpan<byte> ReadBufferView(int bufferViewIndex)
    {
        var view = BufferView(bufferViewIndex);
        var viewOffset = GetInt32OrDefault(view, "byteOffset", 0);
        var viewLength = RequiredProperty(view, "byteLength").GetInt32();

        if (viewOffset < 0 || viewLength < 0 || viewOffset + viewLength > binaryLength)
            throw new FormatException($"Buffer view {bufferViewIndex} extends outside the GLB binary chunk.");

        return bytes.AsSpan(binaryOffset + viewOffset, viewLength);
    }

    /// <summary>Computes the byte offset for one accessor element inside the GLB binary chunk.</summary>
    public int AccessorElementOffset(JsonElement accessor, int elementIndex, int packedElementSize)
    {
        var viewIndex = RequiredProperty(accessor, "bufferView").GetInt32();
        var view = BufferView(viewIndex);
        var accessorOffset = GetInt32OrDefault(accessor, "byteOffset", 0);
        var viewOffset = GetInt32OrDefault(view, "byteOffset", 0);
        var stride = GetInt32OrDefault(view, "byteStride", packedElementSize);
        return viewOffset + accessorOffset + (elementIndex * stride);
    }

    /// <summary>Reads one little-endian 32-bit floating-point value from the GLB binary chunk.</summary>
    public float ReadSingle(int offset)
    {
        var value = BinaryPrimitives.ReadInt32LittleEndian(Binary.Slice(offset, 4));
        return BitConverter.Int32BitsToSingle(value);
    }

    /// <summary>Disposes the parsed JSON document once startup loading is complete.</summary>
    public void Dispose() => json.Dispose();
}

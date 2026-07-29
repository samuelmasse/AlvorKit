namespace AlvorKit.Script.LiveCode;

/// <summary>Stable JSON and length-prefixed pipe framing for Source Update coordinator traffic.</summary>
internal static class SourceUpdateCoordinatorJson
{
    internal static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    internal static T ReadFile<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), Options)
        ?? throw new InvalidOperationException($"Invalid Source Update JSON: {path}");

    internal static void WriteFile<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(value, Options), Encoding.UTF8);
        File.Move(temporary, path, overwrite: true);
    }

    internal static async Task Write<T>(
        Stream stream,
        T value,
        CancellationToken cancellationToken)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(value, Options);
        if (data.Length > 16 * 1024 * 1024)
            throw new InvalidDataException("Source Update coordinator message is too large.");
        var length = BitConverter.GetBytes(data.Length);
        await stream.WriteAsync(length, cancellationToken);
        await stream.WriteAsync(data, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    internal static async Task<T> Read<T>(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var lengthBytes = new byte[sizeof(int)];
        await stream.ReadExactlyAsync(lengthBytes, cancellationToken);
        var length = BitConverter.ToInt32(lengthBytes);
        if (length <= 0 || length > 16 * 1024 * 1024)
            throw new InvalidDataException("Source Update coordinator message length is invalid.");
        var data = new byte[length];
        await stream.ReadExactlyAsync(data, cancellationToken);
        return JsonSerializer.Deserialize<T>(data, Options)
            ?? throw new InvalidDataException("Source Update coordinator message is invalid.");
    }
}

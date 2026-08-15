namespace AlvorKit;

/// <summary>Reads and writes bounded length-prefixed JSON protocol messages.</summary>
internal sealed class LiveCodeWire
{
    private const int MaximumMessageBytes = 64 * 1024 * 1024;

    internal async Task Write<T>(Stream stream, T value, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(value, LiveCodeJson.Options);
        if (payload.Length > MaximumMessageBytes)
            throw new InvalidOperationException($"LiveCode message exceeds {MaximumMessageBytes} bytes.");

        var length = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(length, payload.Length);
        await stream.WriteAsync(length, cancellationToken);
        await stream.WriteAsync(payload, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    internal async Task<T> Read<T>(Stream stream, CancellationToken cancellationToken)
    {
        var lengthBytes = new byte[sizeof(int)];
        await stream.ReadExactlyAsync(lengthBytes, cancellationToken);
        var length = BinaryPrimitives.ReadInt32LittleEndian(lengthBytes);
        if (length <= 0 || length > MaximumMessageBytes)
            throw new InvalidDataException($"Invalid LiveCode message length '{length}'.");

        var payload = new byte[length];
        await stream.ReadExactlyAsync(payload, cancellationToken);
        return JsonSerializer.Deserialize<T>(payload, LiveCodeJson.Options)
            ?? throw new InvalidDataException("LiveCode message contained JSON null.");
    }
}

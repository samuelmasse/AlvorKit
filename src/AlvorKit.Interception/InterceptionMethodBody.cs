namespace AlvorKit;

/// <summary>A complete CLR method body beginning with a tiny or fat method header.</summary>
public sealed class InterceptionMethodBody
{
    private readonly byte[] bytes;

    private InterceptionMethodBody(byte[] bytes) => this.bytes = bytes;

    /// <summary>Gets the immutable complete method body bytes.</summary>
    public ReadOnlyMemory<byte> Bytes => bytes;

    /// <summary>Creates a reviewed raw method body after structural header validation.</summary>
    public static InterceptionMethodBody FromRaw(ReadOnlySpan<byte> body)
    {
        if (body.IsEmpty)
            throw new ArgumentException("A replacement method body cannot be empty.", nameof(body));

        var format = body[0] & 0x03;
        if (format == 0x02)
        {
            var codeSize = body[0] >> 2;
            if (body.Length != codeSize + 1)
                throw new ArgumentException("The tiny method header code size does not match the payload.", nameof(body));
        }
        else if (format == 0x03)
        {
            if (body.Length < 12)
                throw new ArgumentException("A fat method body must contain its twelve-byte header.", nameof(body));
            var headerSize = ((body[1] >> 4) & 0x0F) * 4;
            var codeSize = BinaryPrimitives.ReadInt32LittleEndian(body[4..8]);
            if (headerSize < 12 ||
                headerSize > body.Length ||
                codeSize < 0 ||
                codeSize > body.Length - headerSize)
            {
                throw new ArgumentException("The fat method header does not fit inside the payload.", nameof(body));
            }
        }
        else
        {
            throw new ArgumentException("The replacement does not begin with a CLR tiny or fat method header.", nameof(body));
        }

        return new(body.ToArray());
    }

}

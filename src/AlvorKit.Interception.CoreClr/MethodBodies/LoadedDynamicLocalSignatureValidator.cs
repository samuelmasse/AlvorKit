namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Allows only local signatures whose module tokens remain valid in the owner scope.</summary>
internal static class LoadedDynamicLocalSignatureValidator
{
    internal static void Validate(ReadOnlySpan<byte> signature)
    {
        int offset = 0;
        if (signature.IsEmpty || signature[offset++] != 0x07 ||
            !ReadCompressed(signature, ref offset, out uint count))
        {
            throw Unsupported();
        }
        for (uint index = 0; index < count; ++index)
        {
            if (!ReadType(signature, ref offset))
                throw Unsupported();
        }
        if (offset != signature.Length)
            throw Unsupported();
    }

    private static bool ReadType(ReadOnlySpan<byte> bytes, ref int offset)
    {
        while (offset < bytes.Length && bytes[offset] == 0x45)
            offset++;
        if (offset >= bytes.Length)
            return false;
        switch (bytes[offset++])
        {
            case >= 0x02 and <= 0x0E:
            case 0x16:
            case 0x18:
            case 0x19:
            case 0x1C:
                return true;
            case 0x0F:
            case 0x10:
            case 0x1D:
                return ReadType(bytes, ref offset);
            case 0x11:
            case 0x12:
                if (!ReadCompressed(bytes, ref offset, out uint coded))
                    return false;
                if ((coded & 0x03) == 2)
                    throw TokenBearing();
                return (coded & 0x03) != 3;
            case 0x14:
                return ReadArray(bytes, ref offset);
            case 0x15:
            case 0x1F:
            case 0x20:
                throw TokenBearing();
            default:
                return false;
        }
    }

    private static bool ReadArray(ReadOnlySpan<byte> bytes, ref int offset)
    {
        if (!ReadType(bytes, ref offset) ||
            !ReadCompressed(bytes, ref offset, out _) ||
            !ReadCompressed(bytes, ref offset, out uint sizes))
        {
            return false;
        }
        for (uint index = 0; index < sizes; ++index)
        {
            if (!ReadCompressed(bytes, ref offset, out _))
                return false;
        }
        if (!ReadCompressed(bytes, ref offset, out uint bounds))
            return false;
        for (uint index = 0; index < bounds; ++index)
        {
            if (!ReadCompressed(bytes, ref offset, out _))
                return false;
        }
        return true;
    }

    private static bool ReadCompressed(
        ReadOnlySpan<byte> bytes,
        ref int offset,
        out uint value)
    {
        value = 0;
        if (offset >= bytes.Length)
            return false;
        byte first = bytes[offset++];
        if ((first & 0x80) == 0)
        {
            value = first;
            return true;
        }
        if ((first & 0xC0) == 0x80 && offset < bytes.Length)
        {
            value = (uint)(((first & 0x3F) << 8) | bytes[offset++]);
            return true;
        }
        if ((first & 0xE0) == 0xC0 && offset + 2 < bytes.Length)
        {
            value = ((uint)(first & 0x1F) << 24) |
                ((uint)bytes[offset] << 16) |
                ((uint)bytes[offset + 1] << 8) |
                bytes[offset + 2];
            offset += 3;
            return true;
        }
        return false;
    }

    private static NotSupportedException TokenBearing() =>
        new(
            "Constructor remainder local signatures containing TypeSpec or " +
            "custom-modifier module tokens are unsupported until exact " +
            "dynamic-scope relocation is implemented.");

    private static NotSupportedException Unsupported() =>
        new(
            "Constructor remainder local signature is unsupported until exact " +
            "dynamic-scope relocation is implemented.");
}

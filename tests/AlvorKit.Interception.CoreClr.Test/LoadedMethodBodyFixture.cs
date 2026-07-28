using System.Buffers.Binary;

namespace AlvorKit.Interception.CoreClr.Test;

/// <summary>Builds reviewed raw ECMA-335 body fixtures without reflection body data.</summary>
internal static class LoadedMethodBodyFixture
{
    /// <summary>Builds a complete tiny body from an IL byte stream.</summary>
    internal static byte[] Tiny(params byte[] code)
    {
        if (code.Length > 63)
            throw new ArgumentOutOfRangeException(nameof(code));

        var result = new byte[code.Length + 1];
        result[0] = checked((byte)((code.Length << 2) | 0x02));
        code.CopyTo(result, 1);
        return result;
    }

    /// <summary>Builds a complete fat body with an optional one-part extra section.</summary>
    internal static byte[] Fat(
        byte[] code,
        ushort maxStack = 8,
        bool initLocals = false,
        int localSignatureToken = 0,
        byte[]? section = null)
    {
        const int headerSize = 12;
        var flags = 0x3003 |
            (initLocals ? 0x10 : 0) |
            (section is null ? 0 : 0x08);
        var sectionOffset = Align4(headerSize + code.Length);
        var length = section is null
            ? headerSize + code.Length
            : sectionOffset + section.Length;
        var result = new byte[length];
        BinaryPrimitives.WriteUInt16LittleEndian(
            result,
            checked((ushort)flags));
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(2), maxStack);
        BinaryPrimitives.WriteInt32LittleEndian(
            result.AsSpan(4),
            code.Length);
        BinaryPrimitives.WriteInt32LittleEndian(
            result.AsSpan(8),
            localSignatureToken);
        code.CopyTo(result, headerSize);
        section?.CopyTo(result, sectionOffset);
        return result;
    }

    /// <summary>Builds one small typed-catch exception section.</summary>
    internal static byte[] SmallCatch(
        ushort tryOffset,
        byte tryLength,
        ushort handlerOffset,
        byte handlerLength,
        int catchToken)
    {
        var result = new byte[16];
        result[0] = 0x01;
        result[1] = checked((byte)result.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(
            result.AsSpan(6),
            tryOffset);
        result[8] = tryLength;
        BinaryPrimitives.WriteUInt16LittleEndian(
            result.AsSpan(9),
            handlerOffset);
        result[11] = handlerLength;
        BinaryPrimitives.WriteInt32LittleEndian(
            result.AsSpan(12),
            catchToken);
        return result;
    }

    /// <summary>Builds one fat filter exception section.</summary>
    internal static byte[] FatFilter(
        int tryOffset,
        int tryLength,
        int handlerOffset,
        int handlerLength,
        int filterOffset)
    {
        var result = new byte[28];
        result[0] = 0x41;
        result[1] = checked((byte)result.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4), 0x01);
        BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(8), tryOffset);
        BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(12), tryLength);
        BinaryPrimitives.WriteInt32LittleEndian(
            result.AsSpan(16),
            handlerOffset);
        BinaryPrimitives.WriteInt32LittleEndian(
            result.AsSpan(20),
            handlerLength);
        BinaryPrimitives.WriteInt32LittleEndian(
            result.AsSpan(24),
            filterOffset);
        return result;
    }

    /// <summary>Rounds a complete body position to an extra-section boundary.</summary>
    private static int Align4(int value) => (value + 3) & ~3;
}

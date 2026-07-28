using System.Buffers.Binary;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Reads fixed-width little-endian values from loaded IL operands.</summary>
internal static class LoadedIlPrimitiveReader
{
    /// <summary>Reads one unsigned byte operand.</summary>
    internal static byte ReadByte(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset)
    {
        Require(code, offset, 1, instructionOffset, "one-byte operand");
        return code[offset++];
    }

    /// <summary>Reads one signed byte operand.</summary>
    internal static sbyte ReadSByte(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset) =>
        unchecked((sbyte)ReadByte(code, ref offset, instructionOffset));

    /// <summary>Reads one unsigned sixteen-bit operand.</summary>
    internal static ushort ReadUInt16(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset)
    {
        Require(code, offset, 2, instructionOffset, "two-byte operand");
        var result = BinaryPrimitives.ReadUInt16LittleEndian(code[offset..]);
        offset += 2;
        return result;
    }

    /// <summary>Reads one signed thirty-two-bit operand.</summary>
    internal static int ReadInt32(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset)
    {
        Require(code, offset, 4, instructionOffset, "four-byte operand");
        var result = BinaryPrimitives.ReadInt32LittleEndian(code[offset..]);
        offset += 4;
        return result;
    }

    /// <summary>Reads one signed sixty-four-bit operand.</summary>
    internal static long ReadInt64(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset)
    {
        Require(code, offset, 8, instructionOffset, "eight-byte operand");
        var result = BinaryPrimitives.ReadInt64LittleEndian(code[offset..]);
        offset += 8;
        return result;
    }

    /// <summary>Reads one single-precision floating-point operand.</summary>
    internal static float ReadSingle(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset) =>
        BitConverter.Int32BitsToSingle(
            ReadInt32(code, ref offset, instructionOffset));

    /// <summary>Reads one double-precision floating-point operand.</summary>
    internal static double ReadDouble(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset) =>
        BitConverter.Int64BitsToDouble(
            ReadInt64(code, ref offset, instructionOffset));

    /// <summary>Requires a complete fixed-width value within the IL stream.</summary>
    private static void Require(
        ReadOnlySpan<byte> code,
        int offset,
        int size,
        int instructionOffset,
        string description)
    {
        if (offset < 0 || size < 0 || offset > code.Length - size)
        {
            throw new InvalidDataException(
                $"Malformed loaded IL at IL_{instructionOffset:X4}: {description} is truncated.");
        }
    }
}

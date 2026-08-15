using System.Buffers.Binary;

namespace AlvorKit;

/// <summary>Builds compact raw IL operation sequences for semantic recognition tests.</summary>
internal sealed class LoadedOperationBodyBuilder
{
    /// <summary>The raw IL bytes accumulated in baseline order.</summary>
    private readonly List<byte> code = [];

    /// <summary>Appends a one-byte operand-free opcode.</summary>
    internal LoadedOperationBodyBuilder Emit(byte opCode)
    {
        code.Add(opCode);
        return this;
    }

    /// <summary>Appends a two-byte operand-free opcode.</summary>
    internal LoadedOperationBodyBuilder EmitTwoByte(byte secondByte)
    {
        code.Add(0xFE);
        code.Add(secondByte);
        return this;
    }

    /// <summary>Appends a one-byte opcode and four-byte metadata token.</summary>
    internal LoadedOperationBodyBuilder EmitToken(
        byte opCode,
        int metadataToken)
    {
        code.Add(opCode);
        AppendInt32(metadataToken);
        return this;
    }

    /// <summary>Appends a two-byte opcode and four-byte metadata token.</summary>
    internal LoadedOperationBodyBuilder EmitTwoByteToken(
        byte secondByte,
        int metadataToken)
    {
        code.Add(0xFE);
        code.Add(secondByte);
        AppendInt32(metadataToken);
        return this;
    }

    /// <summary>Builds the complete tiny method body.</summary>
    internal byte[] ToTiny() =>
        LoadedMethodBodyFixture.Tiny([.. code]);

    /// <summary>Appends one little-endian signed integer.</summary>
    private void AppendInt32(int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        code.AddRange(bytes);
    }
}

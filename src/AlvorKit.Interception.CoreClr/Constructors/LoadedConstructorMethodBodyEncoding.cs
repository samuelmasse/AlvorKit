using System.Buffers.Binary;
using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Encodes fat method bodies and exception sections for constructor lowering.</summary>
internal static class LoadedConstructorMethodBodyEncoding
{
    private const ushort FatFormat = 0x0003;
    private const ushort MoreSections = 0x0008;
    private const ushort InitializeLocals = 0x0010;
    private const ushort FatHeaderDwords = 3;

    internal static byte[] Encode(
        IReadOnlyCollection<byte> code,
        ushort maxStack,
        bool initLocals,
        int localSignatureToken,
        ImmutableArray<LoadedExceptionRegion> exceptions,
        int exceptionOffsetAdjustment,
        Func<int, int>? catchToken = null)
    {
        var codeBytes = code as byte[] ?? [.. code];
        var hasExceptions = !exceptions.IsEmpty;
        var codeEnd = 12 + codeBytes.Length;
        var sectionStart = hasExceptions
            ? (codeEnd + 3) & ~3
            : codeEnd;
        byte[] sectionBytes = hasExceptions
            ? EncodeExceptionSection(
                exceptions,
                exceptionOffsetAdjustment,
                catchToken)
            : [];
        var sectionSize = sectionBytes.Length;
        var bytes = new byte[sectionStart + sectionSize];
        var flags = (ushort)(
            FatFormat |
            (hasExceptions ? MoreSections : 0) |
            (initLocals ? InitializeLocals : 0) |
            (FatHeaderDwords << 12));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, flags);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2), maxStack);
        BinaryPrimitives.WriteInt32LittleEndian(
            bytes.AsSpan(4),
            codeBytes.Length);
        BinaryPrimitives.WriteInt32LittleEndian(
            bytes.AsSpan(8),
            localSignatureToken);
        codeBytes.CopyTo(bytes, 12);
        if (!hasExceptions)
            return bytes;

        sectionBytes.CopyTo(bytes, sectionStart);
        return bytes;
    }

    internal static byte[] EncodeExceptionSection(
        ImmutableArray<LoadedExceptionRegion> exceptions,
        int exceptionOffsetAdjustment,
        Func<int, int>? catchToken = null)
    {
        if (exceptions.IsEmpty)
            return [];

        var bytes = new byte[(4 + exceptions.Length * 24)];
        Span<byte> section = bytes;
        section[0] = 0x41;
        var dataSize = section.Length;
        section[1] = ((byte)dataSize);
        section[2] = ((byte)(dataSize >> 8));
        section[3] = ((byte)(dataSize >> 16));
        for (var index = 0; index < exceptions.Length; ++index)
        {
            var region = exceptions[index];
            Span<byte> clause = section.Slice(4 + index * 24, 24);
            BinaryPrimitives.WriteUInt32LittleEndian(
                clause,
                region.RawFlags);
            BinaryPrimitives.WriteInt32LittleEndian(
                clause[4..],
                (region.TryOffset - exceptionOffsetAdjustment));
            BinaryPrimitives.WriteInt32LittleEndian(
                clause[8..],
                region.TryLength);
            BinaryPrimitives.WriteInt32LittleEndian(
                clause[12..],
                (region.HandlerOffset - exceptionOffsetAdjustment));
            BinaryPrimitives.WriteInt32LittleEndian(
                clause[16..],
                region.HandlerLength);
            int finalValue = region.FilterOffset >= 0
                ? (region.FilterOffset - exceptionOffsetAdjustment)
                : region.CatchTypeToken == 0
                    ? 0
                    : catchToken?.Invoke(region.CatchTypeToken) ??
                        region.CatchTypeToken;
            BinaryPrimitives.WriteInt32LittleEndian(
                clause[20..],
                finalValue);
        }

        return bytes;
    }
}

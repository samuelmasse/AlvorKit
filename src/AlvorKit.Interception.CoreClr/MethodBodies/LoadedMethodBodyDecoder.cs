using System.Buffers.Binary;
using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>
/// Decodes exact ECMA-335 method-body bytes supplied by the loaded-runtime backend.
/// </summary>
public static class LoadedMethodBodyDecoder
{
    /// <summary>The low-bit encoding of a fat header.</summary>
    private const ushort FatFormat = 0x03;

    /// <summary>The low-bit encoding of a tiny header.</summary>
    private const ushort TinyFormat = 0x02;

    /// <summary>The mask selecting a method header encoding.</summary>
    private const ushort FormatMask = 0x03;

    /// <summary>The fat-header flag indicating aligned extra sections.</summary>
    private const ushort MoreSections = 0x08;

    /// <summary>The fat-header flag requiring zero-initialized local storage.</summary>
    private const ushort InitLocalsFlag = 0x10;

    /// <summary>The complete supported fat-header flag set.</summary>
    private const ushort AllowedFatFlags =
        FatFormat | MoreSections | InitLocalsFlag;

    /// <summary>The minimum ECMA-335 fat-header length.</summary>
    private const int MinimumFatHeaderSize = 12;

    /// <summary>
    /// Copies, identifies, and decodes a complete authoritative loaded method body.
    /// </summary>
    public static LoadedMethodBodySnapshot Decode(
        ReadOnlySpan<byte> body)
    {
        if (body.IsEmpty)
            throw Malformed("body is empty");

        var immutableBytes = ImmutableArray.CreateRange(body.ToArray());
        var stableBody = immutableBytes.AsSpan();
        return (stableBody[0] & FormatMask) switch
        {
            TinyFormat => DecodeTiny(immutableBytes, stableBody),
            FatFormat => DecodeFat(immutableBytes, stableBody),
            _ => throw Malformed("header is neither tiny nor fat")
        };
    }

    /// <summary>Decodes a one-byte tiny header and its exact IL stream.</summary>
    private static LoadedMethodBodySnapshot DecodeTiny(
        ImmutableArray<byte> bytes,
        ReadOnlySpan<byte> body)
    {
        const int headerSize = 1;
        const ushort maxStack = 8;
        var codeSize = body[0] >> 2;
        if (body.Length != headerSize + codeSize)
            throw Malformed("tiny header code size does not match the supplied bytes");

        var instructions = LoadedIlInstructionDecoder.Decode(body[headerSize..]);
        return new(
            bytes,
            LoadedMethodBodyIdentity.Compute(body),
            LoadedMethodBodyHeaderKind.Tiny,
            headerSize,
            codeSize,
            maxStack,
            false,
            0,
            instructions,
            []);
    }

    /// <summary>Decodes a fat header, IL stream, and optional exception sections.</summary>
    private static LoadedMethodBodySnapshot DecodeFat(
        ImmutableArray<byte> bytes,
        ReadOnlySpan<byte> body)
    {
        Require(body, 0, MinimumFatHeaderSize, "fat header");
        var flagsAndSize = BinaryPrimitives.ReadUInt16LittleEndian(body);
        var flags = (ushort)(flagsAndSize & 0x0FFF);
        if ((flags & ~AllowedFatFlags) != 0)
            throw Malformed($"fat header contains unknown flags 0x{flags:X3}");

        var headerSize = (flagsAndSize >> 12) * sizeof(uint);
        if (headerSize < MinimumFatHeaderSize)
            throw Malformed("fat header is shorter than twelve bytes");
        Require(body, 0, headerSize, "declared fat header");

        var rawCodeSize = BinaryPrimitives.ReadUInt32LittleEndian(body[4..]);
        if (rawCodeSize > int.MaxValue)
            throw Malformed("code size exceeds the supported managed body size");
        var codeSize = (int)rawCodeSize;
        var codeEnd = (long)headerSize + codeSize;
        if (codeEnd > body.Length)
            throw Malformed("fat header code size exceeds the supplied bytes");

        var instructions = LoadedIlInstructionDecoder.Decode(
            body.Slice(headerSize, codeSize));
        var hasSections = (flags & MoreSections) != 0;
        ImmutableArray<LoadedExceptionRegion> regions;
        if (hasSections)
        {
            regions = LoadedExceptionSectionDecoder.Decode(
                body,
                (int)codeEnd,
                codeSize,
                instructions);
        }
        else
        {
            if (codeEnd != body.Length)
                throw Malformed("bytes remain after IL without the MoreSects flag");
            regions = [];
        }

        return new(
            bytes,
            LoadedMethodBodyIdentity.Compute(body),
            LoadedMethodBodyHeaderKind.Fat,
            headerSize,
            codeSize,
            BinaryPrimitives.ReadUInt16LittleEndian(body[2..]),
            (flags & InitLocalsFlag) != 0,
            BinaryPrimitives.ReadInt32LittleEndian(body[8..]),
            instructions,
            regions);
    }

    /// <summary>Requires a complete segment within the authoritative body bytes.</summary>
    private static void Require(
        ReadOnlySpan<byte> body,
        int offset,
        int size,
        string description)
    {
        if (offset < 0 || size < 0 || offset > body.Length - size)
            throw Malformed($"{description} is truncated");
    }

    /// <summary>Creates a malformed loaded-body exception.</summary>
    private static InvalidDataException Malformed(string message) =>
        new($"Malformed loaded method body: {message}.");
}

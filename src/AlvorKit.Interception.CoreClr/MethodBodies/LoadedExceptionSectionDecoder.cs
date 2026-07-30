using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Decodes chained small and fat ECMA-335 exception sections.</summary>
internal static class LoadedExceptionSectionDecoder
{
    /// <summary>The exception-table extra-section kind.</summary>
    private const byte ExceptionTableKind = 0x01;

    /// <summary>The flag selecting fat section and clause encodings.</summary>
    private const byte FatFormat = 0x40;

    /// <summary>The flag indicating another aligned extra section follows.</summary>
    private const byte MoreSections = 0x80;

    /// <summary>The mask isolating an extra section's semantic kind.</summary>
    private const byte KindMask = 0x3F;

    /// <summary>The common section header length.</summary>
    private const int SectionHeaderSize = 4;

    /// <summary>The encoded small clause length.</summary>
    private const int SmallClauseSize = 12;

    /// <summary>The encoded fat clause length.</summary>
    private const int FatClauseSize = 24;

    /// <summary>Decodes all extra sections following the loaded IL stream.</summary>
    internal static ImmutableArray<LoadedExceptionRegion> Decode(
        ReadOnlySpan<byte> body,
        int codeEnd,
        int codeSize,
        ImmutableArray<LoadedIlInstruction> instructions)
    {
        var boundaries = CreateBoundaries(codeSize, instructions);
        var regions = ImmutableArray.CreateBuilder<LoadedExceptionRegion>();
        var offset = Align4(codeEnd);
        while (true)
        {
            Require(body, offset, SectionHeaderSize, "section header");
            var kind = body[offset];
            if ((kind & KindMask) != ExceptionTableKind)
            {
                throw Malformed(
                    $"extra section kind 0x{kind & KindMask:X2} is not an exception table");
            }

            var isFat = (kind & FatFormat) != 0;
            var dataSize = isFat
                ? body[offset + 1] |
                    (body[offset + 2] << 8) |
                    (body[offset + 3] << 16)
                : body[offset + 1];
            if (!isFat && (body[offset + 2] != 0 || body[offset + 3] != 0))
                throw Malformed("small section reserved bytes are nonzero");

            var clauseSize = isFat ? FatClauseSize : SmallClauseSize;
            if (dataSize <= SectionHeaderSize ||
                (dataSize - SectionHeaderSize) % clauseSize != 0)
            {
                throw Malformed(
                    $"section size {dataSize} does not contain whole {clauseSize}-byte clauses");
            }

            Require(body, offset, dataSize, "section data");
            var format = isFat
                ? LoadedExceptionRegionFormat.Fat
                : LoadedExceptionRegionFormat.Small;
            var sectionEnd = (offset + dataSize);
            for (var clauseOffset = offset + SectionHeaderSize;
                 clauseOffset < sectionEnd;
                 clauseOffset += clauseSize)
            {
                regions.Add(LoadedExceptionClauseDecoder.Decode(
                    body.Slice(clauseOffset, clauseSize),
                    format,
                    boundaries));
            }

            if ((kind & MoreSections) == 0)
            {
                if (sectionEnd != body.Length)
                    throw Malformed("bytes remain after the final extra section");
                break;
            }

            offset = Align4(sectionEnd);
            if (offset >= body.Length)
                throw Malformed("section declares a missing following section");
        }

        return regions.ToImmutable();
    }

    /// <summary>Indexes valid region start and end boundaries by baseline offset.</summary>
    private static bool[] CreateBoundaries(
        int codeSize,
        ImmutableArray<LoadedIlInstruction> instructions)
    {
        var result = new bool[(codeSize + 1)];
        foreach (var instruction in instructions)
            result[instruction.BaselineOffset] = true;
        result[codeSize] = true;
        return result;
    }

    /// <summary>Aligns an extra-section position to its required four-byte boundary.</summary>
    private static int Align4(int value)
    {
        var aligned = ((long)value + 3) & ~3L;
        if (aligned > int.MaxValue)
            throw Malformed("section alignment exceeds the supported body size");
        return (int)aligned;
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

    /// <summary>Creates a malformed extra-section exception.</summary>
    private static InvalidDataException Malformed(string message) =>
        new($"Malformed loaded method exception section: {message}.");
}

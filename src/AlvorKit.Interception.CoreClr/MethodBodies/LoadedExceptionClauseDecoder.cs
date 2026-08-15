using System.Buffers.Binary;

namespace AlvorKit;

/// <summary>Decodes and validates one small or fat exception-handling clause.</summary>
internal static class LoadedExceptionClauseDecoder
{
    /// <summary>The ECMA-335 filter-clause flag.</summary>
    private const uint FilterFlag = 0x01;

    /// <summary>The ECMA-335 finally-clause flag.</summary>
    private const uint FinallyFlag = 0x02;

    /// <summary>The ECMA-335 fault-clause flag.</summary>
    private const uint FaultFlag = 0x04;

    /// <summary>The CoreCLR duplicated-clause flag retained in raw metadata.</summary>
    private const uint DuplicatedFlag = 0x08;

    /// <summary>Decodes one clause against immutable instruction boundaries.</summary>
    internal static LoadedExceptionRegion Decode(
        ReadOnlySpan<byte> clause,
        LoadedExceptionRegionFormat format,
        ReadOnlySpan<bool> boundaries)
    {
        uint flags;
        uint tryOffset;
        uint tryLength;
        uint handlerOffset;
        uint handlerLength;
        uint classTokenOrFilterOffset;
        if (format == LoadedExceptionRegionFormat.Fat)
        {
            flags = BinaryPrimitives.ReadUInt32LittleEndian(clause);
            tryOffset = BinaryPrimitives.ReadUInt32LittleEndian(clause[4..]);
            tryLength = BinaryPrimitives.ReadUInt32LittleEndian(clause[8..]);
            handlerOffset = BinaryPrimitives.ReadUInt32LittleEndian(clause[12..]);
            handlerLength = BinaryPrimitives.ReadUInt32LittleEndian(clause[16..]);
            classTokenOrFilterOffset =
                BinaryPrimitives.ReadUInt32LittleEndian(clause[20..]);
        }
        else
        {
            flags = BinaryPrimitives.ReadUInt16LittleEndian(clause);
            tryOffset = BinaryPrimitives.ReadUInt16LittleEndian(clause[2..]);
            tryLength = clause[4];
            handlerOffset = BinaryPrimitives.ReadUInt16LittleEndian(clause[5..]);
            handlerLength = clause[7];
            classTokenOrFilterOffset =
                BinaryPrimitives.ReadUInt32LittleEndian(clause[8..]);
        }

        return Create(
            flags,
            format,
            ToInt(tryOffset, "try offset"),
            ToInt(tryLength, "try length"),
            ToInt(handlerOffset, "handler offset"),
            ToInt(handlerLength, "handler length"),
            classTokenOrFilterOffset,
            boundaries);
    }

    /// <summary>Validates clause flags, ranges, and kind-specific union data.</summary>
    private static LoadedExceptionRegion Create(
        uint flags,
        LoadedExceptionRegionFormat format,
        int tryOffset,
        int tryLength,
        int handlerOffset,
        int handlerLength,
        uint classTokenOrFilterOffset,
        ReadOnlySpan<bool> boundaries)
    {
        if ((flags & ~(FilterFlag | FinallyFlag | FaultFlag | DuplicatedFlag)) != 0)
            throw Malformed($"exception clause flags 0x{flags:X8} are not supported");

        var kind = (flags & ~DuplicatedFlag) switch
        {
            0 => LoadedExceptionRegionKind.Catch,
            FilterFlag => LoadedExceptionRegionKind.Filter,
            FinallyFlag => LoadedExceptionRegionKind.Finally,
            FaultFlag => LoadedExceptionRegionKind.Fault,
            _ => throw Malformed(
                $"exception clause flags 0x{flags:X8} combine incompatible kinds")
        };

        ValidateRange("try", tryOffset, tryLength, boundaries);
        ValidateRange("handler", handlerOffset, handlerLength, boundaries);

        var catchToken = 0;
        var filterOffset = -1;
        if (kind == LoadedExceptionRegionKind.Catch)
        {
            if (classTokenOrFilterOffset == 0)
                throw Malformed("catch clause type token is zero");
            catchToken = unchecked((int)classTokenOrFilterOffset);
        }
        else if (kind == LoadedExceptionRegionKind.Filter)
        {
            filterOffset = ToInt(
                classTokenOrFilterOffset,
                "filter offset");
            if ((uint)filterOffset >= (uint)(boundaries.Length - 1) ||
                !boundaries[filterOffset] ||
                filterOffset >= handlerOffset)
            {
                throw Malformed(
                    "filter offset is not an instruction boundary before its handler");
            }
        }
        else if (classTokenOrFilterOffset != 0)
        {
            throw Malformed("finally or fault clause has nonzero reserved data");
        }

        return new(
            kind,
            format,
            flags,
            tryOffset,
            tryLength,
            handlerOffset,
            handlerLength,
            catchToken,
            filterOffset);
    }

    /// <summary>Validates a nonempty clause range against instruction boundaries.</summary>
    private static void ValidateRange(
        string name,
        int offset,
        int length,
        ReadOnlySpan<bool> boundaries)
    {
        var codeSize = boundaries.Length - 1;
        var end = (long)offset + length;
        if (length <= 0 ||
            (uint)offset >= (uint)codeSize ||
            end > codeSize ||
            !boundaries[offset] ||
            !boundaries[(int)end])
        {
            throw Malformed(
                $"{name} range [{offset}, {end}) does not follow instruction boundaries");
        }
    }

    /// <summary>Converts a clause coordinate without unsigned truncation.</summary>
    private static int ToInt(uint value, string name)
    {
        if (value > int.MaxValue)
            throw Malformed($"{name} exceeds the supported body size");
        return (int)value;
    }

    /// <summary>Creates a malformed extra-section exception.</summary>
    private static InvalidDataException Malformed(string message) =>
        new($"Malformed loaded method exception section: {message}.");
}

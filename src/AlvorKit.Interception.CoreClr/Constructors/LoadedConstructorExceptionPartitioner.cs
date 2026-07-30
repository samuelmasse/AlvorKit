using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Partitions complete exception clauses around a constructor initializer split.</summary>
internal static class LoadedConstructorExceptionPartitioner
{
    /// <summary>Retains or moves complete clauses and rejects every mixed clause.</summary>
    /// <param name="body">Loaded constructor body.</param>
    /// <param name="splitOffset">Baseline split coordinate.</param>
    /// <param name="preserved">Destination for clauses retained with the initializer.</param>
    /// <param name="moved">Destination for clauses moved with the remainder.</param>
    /// <param name="rejections">Destination for clauses that cross the split.</param>
    internal static void Partition(
        LoadedMethodBodySnapshot body,
        int splitOffset,
        ImmutableArray<LoadedExceptionRegion>.Builder preserved,
        ImmutableArray<LoadedExceptionRegion>.Builder moved,
        ImmutableArray<LoadedConstructorRemainderRejection>.Builder rejections)
    {
        for (var index = 0; index < body.ExceptionRegions.Length; ++index)
        {
            var region = body.ExceptionRegions[index];
            var side = Side(region, splitOffset);
            if (side == RegionSide.Preserved)
            {
                preserved.Add(region);
                continue;
            }
            if (side == RegionSide.Remainder)
            {
                moved.Add(region);
                continue;
            }

            var relatedOffset = Math.Min(
                region.TryOffset,
                region.FilterOffset < 0
                    ? region.HandlerOffset
                    : Math.Min(region.HandlerOffset, region.FilterOffset));
            rejections.Add(
                new(
                    LoadedConstructorRemainderRejectionReason
                        .CrossBoundaryExceptionRegion,
                    splitOffset,
                    relatedOffset,
                    $"Exception region {index} ({region.Kind}) cannot cross " +
                    $"constructor initializer split {Offset(splitOffset)}: " +
                    $"try={Range(region.TryOffset, region.TryLength)}, " +
                    $"handler={Range(region.HandlerOffset, region.HandlerLength)}" +
                    FilterRange(region) +
                    "."));
        }
    }

    /// <summary>Classifies one complete exception clause relative to the split.</summary>
    private static RegionSide Side(
        LoadedExceptionRegion region,
        int splitOffset)
    {
        Span<RegionSide> sides = stackalloc RegionSide[3];
        sides[0] = RangeSide(
            region.TryOffset,
            region.TryLength,
            splitOffset);
        sides[1] = RangeSide(
            region.HandlerOffset,
            region.HandlerLength,
            splitOffset);
        var count = 2;
        if (region.FilterOffset >= 0)
        {
            sides[count++] = RangeSide(
                region.FilterOffset,
                region.HandlerOffset - region.FilterOffset,
                splitOffset);
        }

        var first = sides[0];
        for (var index = 0; index < count; ++index)
        {
            if (sides[index] == RegionSide.Crossing ||
                sides[index] != first)
            {
                return RegionSide.Crossing;
            }
        }

        return first;
    }

    /// <summary>Classifies one nonempty half-open range relative to the split.</summary>
    private static RegionSide RangeSide(
        int offset,
        int length,
        int splitOffset)
    {
        var end = (offset + length);
        if (end <= splitOffset)
            return RegionSide.Preserved;
        if (offset >= splitOffset)
            return RegionSide.Remainder;
        return RegionSide.Crossing;
    }

    /// <summary>Formats one baseline coordinate.</summary>
    private static string Offset(int offset) => $"IL_{offset:X4}";

    /// <summary>Formats one half-open baseline range.</summary>
    private static string Range(int offset, int length) =>
        $"[{Offset(offset)}, {Offset((offset + length))})";

    /// <summary>Formats a filter range when the clause owns one.</summary>
    private static string FilterRange(LoadedExceptionRegion region) =>
        region.FilterOffset < 0
            ? string.Empty
            : $", filter={Range(
                region.FilterOffset,
                region.HandlerOffset - region.FilterOffset)}";

    /// <summary>Classifies a range as retained, moved, or unsafe to split.</summary>
    private enum RegionSide
    {
        /// <summary>The complete range remains before the split.</summary>
        Preserved,

        /// <summary>The complete range moves after the split.</summary>
        Remainder,

        /// <summary>The range or clause straddles the split.</summary>
        Crossing
    }
}

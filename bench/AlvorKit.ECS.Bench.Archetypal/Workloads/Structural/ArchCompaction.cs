namespace AlvorKit;

internal static class ArchCompaction
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(EntMut[] byRow, CompactionPosition position)
    {
        int live = byRow.Length;
        long removed = 0;

        while (live != 0)
        {
            int row = position switch
            {
                CompactionPosition.First => 0,
                CompactionPosition.Middle => live / 2,
                CompactionPosition.Last => live - 1,
                _ => throw new UnreachableException(),
            };
            EntMut ent = byRow[row];

            if (ent.UnsetArchetypal<int, FToggle, A>())
                removed++;
            live--;

            if (row != live)
                byRow[row] = byRow[live];
        }

        return removed;
    }
}

namespace AlvorKit;

internal static class HotConcreteClassDirectoryRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(ArchHotFixture fixture, int operations)
    {
        EntArchLoc[] locs = fixture.Locs;
        long sum = 0;

        for (int i = 0; i < operations; i++)
        {
            var loc = locs[i & 1023];
            sum += EntArchColumn<int, F00, Afr24ClassArch>.ValuesAt(loc.RowSetId)!.Length;
        }

        return sum;
    }
}

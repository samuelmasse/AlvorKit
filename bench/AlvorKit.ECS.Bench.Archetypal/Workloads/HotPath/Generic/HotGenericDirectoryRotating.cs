namespace AlvorKit;

internal static class HotGenericDirectoryRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(ArchHotFixture fixture, int operations)
    {
        EntArchLoc[] locs = fixture.Locs;
        long sum = 0;

        for (int i = 0; i < operations; i++)
        {
            var loc = locs[i & 1023];
            sum += EntArchColumn<int, F00, A>.ValuesAt(loc.RowSetId)!.Length;
        }

        return sum;
    }
}

namespace AlvorKit;

internal static class HotGenericLocRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(ArchHotFixture fixture, int operations)
    {
        EntMut[] ents = fixture.Ents;
        long sum = 0;

        for (int i = 0; i < operations; i++)
        {
            var loc = ents[i & 1023].Get<EntArchLoc, A>();
            sum += loc.RowSetId + loc.ArchId + loc.Row;
        }

        return sum;
    }
}

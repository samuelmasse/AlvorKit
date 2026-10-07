namespace AlvorKit;

internal static class HotConcreteClassLocRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(ArchHotFixture fixture, int operations)
    {
        EntMut[] ents = fixture.Ents;
        long sum = 0;

        for (int i = 0; i < operations; i++)
        {
            var loc = ents[i & 1023].Get<EntArchLoc, Afr24ClassArch>();
            sum += loc.RowSetId + loc.ArchId + loc.Row;
        }

        return sum;
    }
}

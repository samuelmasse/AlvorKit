namespace AlvorKit;

internal static class HotRowSetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(ArchHotFixture fixture, int operations)
    {
        int[][] columns = fixture.ScalarColumns;
        int[] rows = fixture.Rows;

        for (int i = 0; i < operations; i++)
        {
            int index = i & 1023;
            columns[index][rows[index]] = i;
        }
    }
}

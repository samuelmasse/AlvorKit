namespace AlvorKit;

internal static class HotRowGetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(ArchHotFixture fixture, int operations)
    {
        int[][] columns = fixture.ScalarColumns;
        int[] rows = fixture.Rows;
        long sum = 0;

        for (int i = 0; i < operations; i++)
        {
            int index = i & 1023;
            sum += columns[index][rows[index]];
        }

        return sum;
    }
}

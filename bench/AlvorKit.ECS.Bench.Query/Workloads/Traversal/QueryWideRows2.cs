namespace AlvorKit;

internal static class QueryWideRows2
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(EntArena arena, int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<WideQueryArch>().With<int, W0>().With<int, W1>();

        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var row in query.Rows())
                sum += row.W0 + row.W1;
        }

        return sum;
    }
}

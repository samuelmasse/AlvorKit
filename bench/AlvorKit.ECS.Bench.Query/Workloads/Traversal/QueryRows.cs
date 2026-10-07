namespace AlvorKit;

internal static class QueryRows
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(EntArena arena, int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<QueryArch>().With<int, ArchValue>();

        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var row in query.Rows())
                sum += row.ArchValue;
        }

        return sum;
    }
}

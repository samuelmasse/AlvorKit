namespace AlvorKit;

internal static class QuerySpans
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(EntArena arena, int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<QueryArch>().With<int, ArchValue>();

        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var chunk in query)
            {
                Span<int> values = chunk.Get<int, ArchValue>();

                for (int i = 0; i < values.Length; i++)
                    sum += values[i];
            }
        }

        return sum;
    }
}

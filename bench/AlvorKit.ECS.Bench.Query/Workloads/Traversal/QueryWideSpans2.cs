namespace AlvorKit;

internal static class QueryWideSpans2
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(EntArena arena, int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<WideQueryArch>().With<int, W0>().With<int, W1>();

        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var chunk in query)
            {
                Span<int> first = chunk.Get<int, W0>();
                Span<int> second = chunk.Get<int, W1>();

                for (int i = 0; i < first.Length; i++)
                    sum += first[i] + second[i];
            }
        }

        return sum;
    }
}

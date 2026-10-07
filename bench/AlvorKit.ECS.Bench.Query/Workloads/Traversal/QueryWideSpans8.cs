namespace AlvorKit;

internal static class QueryWideSpans8
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(EntArena arena, int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<WideQueryArch>()
            .With<int, W0>()
            .With<int, W1>()
            .With<int, W2>()
            .With<int, W3>()
            .With<int, W4>()
            .With<int, W5>()
            .With<int, W6>()
            .With<int, W7>();

        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var chunk in query)
            {
                Span<int> c0 = chunk.Get<int, W0>();
                Span<int> c1 = chunk.Get<int, W1>();
                Span<int> c2 = chunk.Get<int, W2>();
                Span<int> c3 = chunk.Get<int, W3>();
                Span<int> c4 = chunk.Get<int, W4>();
                Span<int> c5 = chunk.Get<int, W5>();
                Span<int> c6 = chunk.Get<int, W6>();
                Span<int> c7 = chunk.Get<int, W7>();

                for (int i = 0; i < c0.Length; i++)
                    sum += c0[i] + c1[i] + c2[i] + c3[i] + c4[i] + c5[i] + c6[i] + c7[i];
            }
        }

        return sum;
    }
}

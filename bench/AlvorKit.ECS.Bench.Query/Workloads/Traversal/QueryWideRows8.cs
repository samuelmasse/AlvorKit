namespace AlvorKit;

internal static class QueryWideRows8
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
            foreach (var row in query.Rows())
            {
                sum += row.W0 + row.W1 + row.W2 + row.W3;
                sum += row.W4 + row.W5 + row.W6 + row.W7;
            }
        }

        return sum;
    }
}

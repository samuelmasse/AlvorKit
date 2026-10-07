namespace AlvorKit;

public static class CoreSpans
{
    public static void Run(EntArena arena, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var query = arena.QueryArchetypal<CoreBenchComponents>()
                .With<int, CoreBenchComponents.First>()
                .With<int, CoreBenchComponents.Second>()
                .With<int, CoreBenchComponents.Third>();

            foreach (var chunk in query)
            {
                var first = chunk.Get<int, CoreBenchComponents.First>();
                var second = chunk.Get<int, CoreBenchComponents.Second>();
                var third = chunk.Get<int, CoreBenchComponents.Third>();

                for (var i = 0; i < first.Length; i++)
                    first[i] += second[i] + third[i];
            }
        }
    }
}

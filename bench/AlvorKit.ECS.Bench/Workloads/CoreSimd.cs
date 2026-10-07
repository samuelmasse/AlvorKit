namespace AlvorKit;

public static class CoreSimd
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
                var vectorLength = first.Length - first.Length % Vector256<int>.Count;
                var a = MemoryMarshal.Cast<int, Vector256<int>>(first[..vectorLength]);
                var b = MemoryMarshal.Cast<int, Vector256<int>>(second[..vectorLength]);
                var c = MemoryMarshal.Cast<int, Vector256<int>>(third[..vectorLength]);

                for (var i = 0; i < a.Length; i++)
                    a[i] += b[i] + c[i];

                for (var i = vectorLength; i < first.Length; i++)
                    first[i] += second[i] + third[i];
            }
        }
    }
}

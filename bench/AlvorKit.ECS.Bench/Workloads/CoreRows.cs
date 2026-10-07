namespace AlvorKit;

public static class CoreRows
{
    public static void Run(EntArena arena, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var query = arena.QueryArchetypal<CoreBenchComponents>()
                .With<int, CoreBenchComponents.First>()
                .With<int, CoreBenchComponents.Second>()
                .With<int, CoreBenchComponents.Third>();

            foreach (var row in query.Rows())
                row.First += row.Second + row.Third;
        }
    }
}

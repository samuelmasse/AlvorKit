namespace AlvorKit;

public static class CoreCreateShape
{
    public static void Run(EntArena arena, int count)
    {
        for (var i = 0; i < count; i++)
        {
            arena.AllocArchetypal<CoreBenchComponents>()
                .With<int, CoreBenchComponents.First>(i)
                .With<int, CoreBenchComponents.Second>(2)
                .With<int, CoreBenchComponents.Third>(3)
                .Create();
        }
    }
}

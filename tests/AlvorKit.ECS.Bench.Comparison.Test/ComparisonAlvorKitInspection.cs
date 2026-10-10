namespace AlvorKit;

internal static class ComparisonAlvorKitInspection
{
    internal static ComparisonObservation Read(EntArena world, int components)
    {
        var count = 0;
        long first = 0, second = 0, third = 0;

        if (components == 1)
        {
            foreach (var chunk in world.QueryArchetypal<RaceAlvorComponents>().With<int, RaceAlvorComponents.Component1>())
            {
                var s1 = chunk.Get<int, RaceAlvorComponents.Component1>();

                for (var j = 0; j < s1.Length; j++)
                {
                    count++;
                    first += s1[j];
                }
            }
        }
        if (components == 2)
        {
            foreach (var chunk in world.QueryArchetypal<RaceAlvorComponents>()
                .With<int, RaceAlvorComponents.Component1>()
                .With<int, RaceAlvorComponents.Component2>())
            {
                var s1 = chunk.Get<int, RaceAlvorComponents.Component1>();
                var s2 = chunk.Get<int, RaceAlvorComponents.Component2>();

                for (var j = 0; j < s1.Length; j++)
                {
                    count++;
                    first += s1[j];
                    second += s2[j];
                }
            }
        }
        if (components == 3)
        {
            foreach (var chunk in world.QueryArchetypal<RaceAlvorComponents>()
                .With<int, RaceAlvorComponents.Component1>()
                .With<int, RaceAlvorComponents.Component2>()
                .With<int, RaceAlvorComponents.Component3>())
            {
                var s1 = chunk.Get<int, RaceAlvorComponents.Component1>();
                var s2 = chunk.Get<int, RaceAlvorComponents.Component2>();
                var s3 = chunk.Get<int, RaceAlvorComponents.Component3>();

                for (var j = 0; j < s1.Length; j++)
                {
                    count++;
                    first += s1[j];
                    second += s2[j];
                    third += s3[j];
                }
            }
        }
        return new(count, first, second, third);
    }

    internal static int Padding(EntArena world)
    {
        var count = 0;

        foreach (var chunk in world.QueryArchetypal<RaceAlvorComponents>().With<bool, RaceAlvorComponents.Padding1>())
        count += chunk.Get<bool, RaceAlvorComponents.Padding1>().Length;
        return count;
    }
}

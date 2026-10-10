using static AlvorKit.RaceDefaultEcsBaseContext;

namespace AlvorKit;

internal static class ComparisonDefaultEcsInspection
{
    internal static ComparisonObservation Read(global::DefaultEcs.World world, int components)
    {
        var count = 0;
        long first = 0, second = 0, third = 0;

        if (components == 1)
        {
            using var query = world.GetEntities().With<Component1>().AsSet();

            foreach (ref readonly var ent in query.GetEntities())
            {
                var c1 = ent.Get<Component1>();
                count++;
                first += c1.Value;
            }
        }
        if (components == 2)
        {
            using var query = world.GetEntities().With<Component1>().With<Component2>().AsSet();

            foreach (ref readonly var ent in query.GetEntities())
            {
                var c1 = ent.Get<Component1>();
                var c2 = ent.Get<Component2>();
                count++;
                first += c1.Value;
                second += c2.Value;
            }
        }
        if (components == 3)
        {
            using var query = world.GetEntities().With<Component1>().With<Component2>().With<Component3>().AsSet();

            foreach (ref readonly var ent in query.GetEntities())
            {
                var c1 = ent.Get<Component1>();
                var c2 = ent.Get<Component2>();
                var c3 = ent.Get<Component3>();
                count++;
                first += c1.Value;
                second += c2.Value;
                third += c3.Value;
            }
        }
        return new(count, first, second, third);
    }

    internal static int Padding(global::DefaultEcs.World world)
    {
        var count = 0;

        using var query = world.GetEntities().With<ComparisonPadding>().AsSet();

        foreach (ref readonly var ent in query.GetEntities())
        {
            count++;
        }

        return count;
    }
}

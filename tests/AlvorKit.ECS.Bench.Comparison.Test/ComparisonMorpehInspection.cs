using static AlvorKit.RaceMorpehBaseContext;
using global::Scellecs.Morpeh;

namespace AlvorKit;

internal static class ComparisonMorpehInspection
{
    internal static ComparisonObservation Read(global::Scellecs.Morpeh.World world, int components)
    {
        var count = 0;
        long first = 0, second = 0, third = 0;

        if (components == 1)
        {
            var query = world.Filter.With<Component1>().Build();

            foreach (var ent in query)
            {
                var c1 = world.GetStash<Component1>().Get(ent);
                count++;
                first += c1.Value;
            }
        }
        if (components == 2)
        {
            var query = world.Filter.With<Component1>().With<Component2>().Build();

            foreach (var ent in query)
            {
                var c1 = world.GetStash<Component1>().Get(ent);
                var c2 = world.GetStash<Component2>().Get(ent);
                count++;
                first += c1.Value;
                second += c2.Value;
            }
        }
        if (components == 3)
        {
            var query = world.Filter.With<Component1>().With<Component2>().With<Component3>().Build();

            foreach (var ent in query)
            {
                var c1 = world.GetStash<Component1>().Get(ent);
                var c2 = world.GetStash<Component2>().Get(ent);
                var c3 = world.GetStash<Component3>().Get(ent);
                count++;
                first += c1.Value;
                second += c2.Value;
                third += c3.Value;
            }
        }
        return new(count, first, second, third);
    }

    internal static int Padding(global::Scellecs.Morpeh.World world)
    {
        var count = 0;

        var query = world.Filter.With<ComparisonPadding>().Build();

        foreach (var ent in query)
        {
            count++;
        }

        return count;
    }
}

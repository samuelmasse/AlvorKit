using static AlvorKit.RaceArchComponents;
using global::Arch.Core;

namespace AlvorKit;

internal static class ComparisonArchInspection
{
    internal static ComparisonObservation Read(global::Arch.Core.World world, int components)
    {
        var count = 0;
        long first = 0, second = 0, third = 0;

        if (components == 1)
        {
            var query = new QueryDescription().WithAll<Component1>();
            world.Query(in query, (ref Component1 c1) =>
            {
                count++;
                first += c1.Value;
            });
        }
        if (components == 2)
        {
            var query = new QueryDescription().WithAll<Component1, Component2>();
            world.Query(in query, (ref Component1 c1, ref Component2 c2) =>
            {
                count++;
                first += c1.Value;
                second += c2.Value;
            });
        }
        if (components == 3)
        {
            var query = new QueryDescription().WithAll<Component1, Component2, Component3>();
            world.Query(in query, (ref Component1 c1, ref Component2 c2, ref Component3 c3) =>
            {
                count++;
                first += c1.Value;
                second += c2.Value;
                third += c3.Value;
            });
        }
        return new(count, first, second, third);
    }

    internal static int Padding(global::Arch.Core.World world)
    {
        var count = 0;

        var query = new QueryDescription().WithAll<ComparisonPadding>();
        world.Query(in query, (ref ComparisonPadding c1) =>
        {
            count++;
        });

        return count;
    }
}

using static AlvorKit.RaceFrentBaseContext;
using global::Frent;
using global::Frent.Systems;

namespace AlvorKit;

internal static class ComparisonFrentInspection
{
    internal static ComparisonObservation Read(global::Frent.World world, int components)
    {
        var count = 0;
        long first = 0, second = 0, third = 0;

        if (components == 1)
        {
            world.Query<Component1>().Delegate((ref Component1 c1) =>
            {
                count++;
                first += c1.Value;
            });
        }
        if (components == 2)
        {
            world.Query<Component1, Component2>().Delegate((ref Component1 c1, ref Component2 c2) =>
            {
                count++;
                first += c1.Value;
                second += c2.Value;
            });
        }
        if (components == 3)
        {
            world.Query<Component1, Component2, Component3>().Delegate((ref Component1 c1, ref Component2 c2, ref Component3 c3) =>
            {
                count++;
                first += c1.Value;
                second += c2.Value;
                third += c3.Value;
            });
        }
        return new(count, first, second, third);
    }

    internal static int Padding(global::Frent.World world)
    {
        var count = 0;

        world.Query<ComparisonPadding>().Delegate((ref ComparisonPadding c1) =>
        {
            count++;
        });

        return count;
    }
}

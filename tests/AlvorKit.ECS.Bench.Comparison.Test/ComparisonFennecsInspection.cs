using static AlvorKit.RaceFennecsComponents;

namespace AlvorKit;

internal static class ComparisonFennecsInspection
{
    internal static ComparisonObservation Read(global::fennecs.World world, int components)
    {
        var count = 0;
        long first = 0, second = 0, third = 0;

        if (components == 1)
        {
            world.Stream<Component1>().For((ref Component1 c1) =>
            {
                count++;
                first += c1.Value;
            });
        }
        if (components == 2)
        {
            world.Stream<Component1, Component2>().For((ref Component1 c1, ref Component2 c2) =>
            {
                count++;
                first += c1.Value;
                second += c2.Value;
            });
        }
        if (components == 3)
        {
            world.Stream<Component1, Component2, Component3>().For((ref Component1 c1, ref Component2 c2, ref Component3 c3) =>
            {
                count++;
                first += c1.Value;
                second += c2.Value;
                third += c3.Value;
            });
        }
        return new(count, first, second, third);
    }

    internal static int Padding(global::fennecs.World world)
    {
        var count = 0;

        world.Stream<ComparisonPadding>().For((ref ComparisonPadding c1) =>
        {
            count++;
        });

        return count;
    }
}

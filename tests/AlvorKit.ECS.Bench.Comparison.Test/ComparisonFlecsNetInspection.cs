using static AlvorKit.RaceFlecsNetComponents;

namespace AlvorKit;

internal static class ComparisonFlecsNetInspection
{
    internal static ComparisonObservation Read(global::Flecs.NET.Core.World world, int components)
    {
        var count = 0;
        long first = 0, second = 0, third = 0;

        if (components == 1)
        {
            using var query = world.Query<Component1>();
            query.Each((ref Component1 c1) =>
            {
                count++;
                first += c1.Value;
            });
        }
        if (components == 2)
        {
            using var query = world.Query<Component1, Component2>();
            query.Each((ref Component1 c1, ref Component2 c2) =>
            {
                count++;
                first += c1.Value;
                second += c2.Value;
            });
        }
        if (components == 3)
        {
            using var query = world.Query<Component1, Component2, Component3>();
            query.Each((ref Component1 c1, ref Component2 c2, ref Component3 c3) =>
            {
                count++;
                first += c1.Value;
                second += c2.Value;
                third += c3.Value;
            });
        }
        return new(count, first, second, third);
    }

    internal static int Padding(global::Flecs.NET.Core.World world)
    {
        var count = 0;

        using var query = world.Query<ComparisonPadding>();
        query.Each((ref ComparisonPadding c1) =>
        {
            count++;
        });

        return count;
    }
}

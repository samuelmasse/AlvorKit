using static AlvorKit.RaceFrifloEngineComponents;

namespace AlvorKit;

internal static class ComparisonFrifloEngineEcsInspection
{
    internal static ComparisonObservation Read(global::Friflo.Engine.ECS.EntityStore world, int components)
    {
        var count = 0;
        long first = 0, second = 0, third = 0;

        if (components == 1)
        {
            foreach (var (values1, _) in world.Query<Component1>().Chunks)
            {
                for (var j = 0; j < values1.Length; j++)
                {
                    var c1 = values1.Span[j];
                    count++;
                    first += c1.Value;
                }
            }
        }
        if (components == 2)
        {
            foreach (var (values1, values2, _) in world.Query<Component1, Component2>().Chunks)
            {
                for (var j = 0; j < values1.Length; j++)
                {
                    var c1 = values1.Span[j];
                    var c2 = values2.Span[j];
                    count++;
                    first += c1.Value;
                    second += c2.Value;
                }
            }
        }
        if (components == 3)
        {
            foreach (var (values1, values2, values3, _) in world.Query<Component1, Component2, Component3>().Chunks)
            {
                for (var j = 0; j < values1.Length; j++)
                {
                    var c1 = values1.Span[j];
                    var c2 = values2.Span[j];
                    var c3 = values3.Span[j];
                    count++;
                    first += c1.Value;
                    second += c2.Value;
                    third += c3.Value;
                }
            }
        }
        return new(count, first, second, third);
    }

    internal static int Padding(global::Friflo.Engine.ECS.EntityStore world)
    {
        var count = 0;

        foreach (var (values1, _) in world.Query<ComparisonPadding>().Chunks)
        {
            for (var j = 0; j < values1.Length; j++)
            {
                count++;
            }
        }

        return count;
    }
}

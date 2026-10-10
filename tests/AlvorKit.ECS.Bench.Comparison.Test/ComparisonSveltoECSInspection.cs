using static AlvorKit.RaceSveltoECSBaseContext;
using global::Svelto.ECS;

namespace AlvorKit;

internal static class ComparisonSveltoECSInspection
{
    internal static ComparisonObservation Read(EnginesRoot root, int components)
    {
        var observer = new ComparisonSveltoObserver();
        root.AddEngine(observer, true);
        root.Ready();
        var count = 0;
        long first = 0, second = 0, third = 0;

        if (components == 1)
        {
            var (c1, length) = observer.entitiesDB.QueryEntities<Component1>(Group);
            count = length;

            for (var i = 0; i < length; i++)
                first += c1[i].Value;
        }
        if (components == 2)
        {
            var (c1, c2, length) = observer.entitiesDB.QueryEntities<Component1, Component2>(Group);
            count = length;

            for (var i = 0; i < length; i++)
            {
                first += c1[i].Value;
                second += c2[i].Value;
            }
        }
        if (components == 3)
        {
            var (c1, c2, c3, length) = observer.entitiesDB.QueryEntities<Component1, Component2, Component3>(Group);
            count = length;

            for (var i = 0; i < length; i++)
            {
                first += c1[i].Value;
                second += c2[i].Value;
                third += c3[i].Value;
            }
        }
        return new(count, first, second, third);
    }

    internal static int Padding(EnginesRoot root)
    {
        var count = 0;
        var observer = new ComparisonSveltoObserver();
        root.AddEngine(observer, true);
        root.Ready();

        var (values, length) = observer.entitiesDB.QueryEntities<ComparisonPadding>(Group);
        count = length;
        return count;
    }
}

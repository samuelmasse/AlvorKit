using static AlvorKit.RaceSveltoECSBaseContext;
using global::Svelto.ECS;

namespace AlvorKit;

/// <summary>Observes one benchmark root without registering duplicate engines or changing its readiness mode.</summary>
internal class ComparisonSveltoECSInspection
{
    private readonly ComparisonSveltoObserver observer = new();

    /// <summary>Attaches the observer once; the root initializes querying engines when they are added.</summary>
    internal ComparisonSveltoECSInspection(EnginesRoot root)
    {
        root.AddEngine(observer);
    }

    /// <summary>Reads component counts and values before or after a benchmark workload.</summary>
    internal ComparisonObservation Read(int components)
    {
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

    /// <summary>Counts padding components through the same initialized observer.</summary>
    internal int Padding()
    {
        var count = 0;
        var (_, length) = observer.entitiesDB.QueryEntities<ComparisonPadding>(Group);
        count = length;
        return count;
    }
}

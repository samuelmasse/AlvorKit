using static AlvorKit.RaceFrifloEngineComponents;
using Friflo.Engine.ECS;
using static AlvorKit.RaceFrifloUpdate1;

namespace AlvorKit;

internal static class RaceFrifloUpdate1Scalar
{
    internal static void Run(RaceFrifloUpdate1.FrifloEngineEcsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            foreach (var (component1, _) in fixture.queryOne.Chunks)
            {
                foreach (ref Component1 component in component1.Span)
                    ++component.Value;
            }
        }
    }
}

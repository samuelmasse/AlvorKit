using static AlvorKit.RaceFrifloEngineComponents;
using Friflo.Engine.ECS;
using static AlvorKit.RaceFrifloUpdate2;

namespace AlvorKit;

internal static class RaceFrifloUpdate2Scalar
{
    internal static void Run(RaceFrifloUpdate2.FrifloEngineEcsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            foreach (var (component1, component2, _) in fixture.queryTwo.Chunks)
            {
                Span<Component1> component1Span = component1.Span;
                Span<Component2> component2Span = component2.Span;

                for (int n = 0; n < component1Span.Length; n++)
                    Update(ref component1Span[n], ref component2Span[n]);
            }
        }
    }
}

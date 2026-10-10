using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorMixed;

namespace AlvorKit;

internal static class RaceAlvorMixedQuery
{
    internal static void Run(RaceAlvorMixed.AlvorKitContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var query = fixture.Arena.QueryArchetypal<RaceAlvorComponents>()
                .With<int, RaceAlvorComponents.Component1>()
                .With<int, RaceAlvorComponents.Component2>();

            foreach (var chunk in query)
            {
                Span<int> component1 = chunk.Get<int, RaceAlvorComponents.Component1>();
                Span<int> component2 = chunk.Get<int, RaceAlvorComponents.Component2>();

                for (int i = 0; i < component1.Length; i++)
                    component1[i] += component2[i];
            }
        }
    }
}

using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorUpdate3;

namespace AlvorKit;

internal static class RaceAlvorUpdate3Query
{
    internal static void Run(RaceAlvorUpdate3.AlvorKitContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var query = fixture.Arena.QueryArchetypal<RaceAlvorComponents>()
                .With<int, RaceAlvorComponents.Component1>()
                .With<int, RaceAlvorComponents.Component2>()
                .With<int, RaceAlvorComponents.Component3>();

            foreach (var chunk in query)
            {
                Span<int> component1 = chunk.Get<int, RaceAlvorComponents.Component1>();
                Span<int> component2 = chunk.Get<int, RaceAlvorComponents.Component2>();
                Span<int> component3 = chunk.Get<int, RaceAlvorComponents.Component3>();

                for (int i = 0; i < component1.Length; i++)
                    component1[i] += component2[i] + component3[i];
            }
        }
    }
}

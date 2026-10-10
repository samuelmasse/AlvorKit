using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorUpdate1;

namespace AlvorKit;

internal static class RaceAlvorUpdate1Query
{
    internal static void Run(RaceAlvorUpdate1.AlvorKitContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var query = fixture.Arena.QueryArchetypal<RaceAlvorComponents>().With<int, RaceAlvorComponents.Component1>();

            foreach (var chunk in query)
            {
                Span<int> components = chunk.Get<int, RaceAlvorComponents.Component1>();

                for (int i = 0; i < components.Length; i++)
                    components[i]++;
            }
        }
    }
}

using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorMixed;

namespace AlvorKit;

internal static class RaceAlvorMixedRows
{
    internal static void Run(RaceAlvorMixed.AlvorKitContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var query = fixture.Arena.QueryArchetypal<RaceAlvorComponents>()
                .With<int, RaceAlvorComponents.Component1>()
                .With<int, RaceAlvorComponents.Component2>();

            foreach (var row in query.Rows())
                row.Component1 += row.Component2;
        }
    }
}

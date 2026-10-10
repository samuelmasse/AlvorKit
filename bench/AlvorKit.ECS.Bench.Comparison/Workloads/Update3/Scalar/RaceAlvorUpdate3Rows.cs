using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorUpdate3;

namespace AlvorKit;

internal static class RaceAlvorUpdate3Rows
{
    internal static void Run(RaceAlvorUpdate3.AlvorKitContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var query = fixture.Arena.QueryArchetypal<RaceAlvorComponents>()
                .With<int, RaceAlvorComponents.Component1>()
                .With<int, RaceAlvorComponents.Component2>()
                .With<int, RaceAlvorComponents.Component3>();

            foreach (var row in query.Rows())
                row.Component1 += row.Component2 + row.Component3;
        }
    }
}

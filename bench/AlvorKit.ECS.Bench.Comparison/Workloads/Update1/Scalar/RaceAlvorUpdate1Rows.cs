using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorUpdate1;

namespace AlvorKit;

internal static class RaceAlvorUpdate1Rows
{
    internal static void Run(RaceAlvorUpdate1.AlvorKitContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var query = fixture.Arena.QueryArchetypal<RaceAlvorComponents>().With<int, RaceAlvorComponents.Component1>();

            foreach (var row in query.Rows())
                row.Component1++;
        }
    }
}

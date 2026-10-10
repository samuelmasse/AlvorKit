using System.Runtime.CompilerServices;
using static AlvorKit.RaceFennecsComponents;
using fennecs;
using static AlvorKit.RaceFennecsUpdate1;

namespace AlvorKit;

internal static class RaceFennecsUpdate1ForEach
{
    internal static void Run(RaceFennecsUpdate1.FennecsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.query.For((ref Component1 comp0) => comp0.Value++);
    }
}

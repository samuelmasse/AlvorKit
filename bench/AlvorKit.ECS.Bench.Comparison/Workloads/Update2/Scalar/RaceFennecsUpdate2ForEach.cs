using System.Runtime.CompilerServices;
using static AlvorKit.RaceFennecsComponents;
using fennecs;
using static AlvorKit.RaceFennecsUpdate2;

namespace AlvorKit;

internal static class RaceFennecsUpdate2ForEach
{
    internal static void Run(RaceFennecsUpdate2.FennecsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.query.For((ref Component1 c1, ref Component2 c2) => c1.Value += c2.Value);
    }
}

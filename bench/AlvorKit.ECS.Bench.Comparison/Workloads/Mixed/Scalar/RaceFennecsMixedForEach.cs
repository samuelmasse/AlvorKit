using static AlvorKit.RaceFennecsComponents;
using fennecs;
using static AlvorKit.RaceFennecsMixed;

namespace AlvorKit;

internal static class RaceFennecsMixedForEach
{
    internal static void Run(RaceFennecsMixed.FennecsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.query.For((ref Component1 c1, ref Component2 c2) => c1.Value += c2.Value);
    }
}

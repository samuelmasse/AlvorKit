using Scellecs.Morpeh;
using static AlvorKit.RaceMorpehMixed;

namespace AlvorKit;

internal static class RaceMorpehMixedDirect
{
    internal static void Run(RaceMorpehMixed.MorpehContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.MonoThreadDirectSystem.OnUpdate(0f);
    }
}

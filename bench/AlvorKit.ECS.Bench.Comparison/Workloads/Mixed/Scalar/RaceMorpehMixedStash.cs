using Scellecs.Morpeh;
using static AlvorKit.RaceMorpehMixed;

namespace AlvorKit;

internal static class RaceMorpehMixedStash
{
    internal static void Run(RaceMorpehMixed.MorpehContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.MonoThreadStashSystem.OnUpdate(0f);
    }
}

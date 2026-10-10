using Scellecs.Morpeh;
using static AlvorKit.RaceMorpehUpdate3;

namespace AlvorKit;

internal static class RaceMorpehUpdate3Stash
{
    internal static void Run(RaceMorpehUpdate3.MorpehContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.MonoThreadStashSystem.OnUpdate(0f);
    }
}

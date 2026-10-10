using Scellecs.Morpeh;
using static AlvorKit.RaceMorpehUpdate2;

namespace AlvorKit;

internal static class RaceMorpehUpdate2Direct
{
    internal static void Run(RaceMorpehUpdate2.MorpehContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.MonoThreadDirectSystem.OnUpdate(0f);
    }
}

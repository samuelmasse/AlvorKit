using Scellecs.Morpeh;
using static AlvorKit.RaceMorpehUpdate1;

namespace AlvorKit;

internal static class RaceMorpehUpdate1Direct
{
    internal static void Run(RaceMorpehUpdate1.MorpehContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.MonoThreadDirectSystem.OnUpdate(0f);
    }
}

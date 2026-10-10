using Scellecs.Morpeh;
using static AlvorKit.RaceMorpehUpdate3;

namespace AlvorKit;

internal static class RaceMorpehUpdate3Direct
{
    internal static void Run(RaceMorpehUpdate3.MorpehContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.MonoThreadDirectSystem.OnUpdate(0f);
    }
}

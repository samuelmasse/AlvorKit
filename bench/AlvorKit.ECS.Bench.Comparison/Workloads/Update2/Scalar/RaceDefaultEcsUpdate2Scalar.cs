using DefaultEcs;
using DefaultEcs.System;
using static AlvorKit.RaceDefaultEcsUpdate2;

namespace AlvorKit;

internal static class RaceDefaultEcsUpdate2Scalar
{
    internal static void Run(RaceDefaultEcsUpdate2.DefaultEcsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.MonoThreadEntitySetSystem.Update(0);
    }
}

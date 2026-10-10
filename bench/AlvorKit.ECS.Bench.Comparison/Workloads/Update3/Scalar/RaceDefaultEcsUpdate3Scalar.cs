using DefaultEcs;
using DefaultEcs.System;
using static AlvorKit.RaceDefaultEcsUpdate3;

namespace AlvorKit;

internal static class RaceDefaultEcsUpdate3Scalar
{
    internal static void Run(RaceDefaultEcsUpdate3.DefaultEcsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.MonoThreadEntitySetSystem.Update(0);
    }
}

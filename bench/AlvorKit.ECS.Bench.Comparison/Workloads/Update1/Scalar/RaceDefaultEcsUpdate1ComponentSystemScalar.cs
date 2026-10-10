using DefaultEcs;
using DefaultEcs.System;
using static AlvorKit.RaceDefaultEcsUpdate1;

namespace AlvorKit;

internal static class RaceDefaultEcsUpdate1ComponentSystemScalar
{
    internal static void Run(RaceDefaultEcsUpdate1.DefaultEcsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.MonoThreadComponentSystem.Update(0);
    }
}

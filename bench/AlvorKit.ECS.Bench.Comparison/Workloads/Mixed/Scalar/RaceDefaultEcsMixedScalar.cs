using DefaultEcs;
using DefaultEcs.System;
using static AlvorKit.RaceDefaultEcsMixed;

namespace AlvorKit;

internal static class RaceDefaultEcsMixedScalar
{
    internal static void Run(RaceDefaultEcsMixed.DefaultEcsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.MonoThreadEntitySetSystem.Update(0);
    }
}

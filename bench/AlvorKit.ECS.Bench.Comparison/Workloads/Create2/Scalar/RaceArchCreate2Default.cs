using Arch.Core;
using Arch.Core.Utils;
using static AlvorKit.RaceArchComponents;
using static AlvorKit.RaceArchCreate2;

namespace AlvorKit;

internal static class RaceArchCreate2Default
{
    internal static void Run(RaceArchBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            World world = fixture.World;
            world.EnsureCapacity(_archetype, entCount);

            for (int i = 0; i < entCount; ++i)
                world.Create(_archetype);
        }
    }
}

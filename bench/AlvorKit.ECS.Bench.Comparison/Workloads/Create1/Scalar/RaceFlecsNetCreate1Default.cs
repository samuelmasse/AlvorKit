using static AlvorKit.RaceFlecsNetComponents;
using Flecs.NET.Core;

namespace AlvorKit;

internal static class RaceFlecsNetCreate1Default
{
    internal static void Run(RaceFlecsNetBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            World world = fixture.World;

            for (int i = 0; i < entCount; ++i)
                world.Entity().Set<Component1>(new());
        }
    }
}

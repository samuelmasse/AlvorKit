using Svelto.DataStructures;
using Svelto.ECS;
using static AlvorKit.RaceSveltoUpdate1;

namespace AlvorKit;

internal static class RaceSveltoUpdate1Default
{
    internal static void Run(RaceSveltoUpdate1.SveltoECSContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.Engine.Update();
    }
}

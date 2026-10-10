using Svelto.DataStructures;
using Svelto.ECS;
using static AlvorKit.RaceSveltoUpdate2;

namespace AlvorKit;

internal static class RaceSveltoUpdate2Default
{
    internal static void Run(RaceSveltoUpdate2.SveltoECSContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.Engine.Update();
    }
}

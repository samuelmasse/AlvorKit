using Svelto.DataStructures;
using Svelto.ECS;
using static AlvorKit.RaceSveltoMixed;

namespace AlvorKit;

internal static class RaceSveltoMixedDefault
{
    internal static void Run(RaceSveltoMixed.SveltoECSContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.Engine.Update();
    }
}

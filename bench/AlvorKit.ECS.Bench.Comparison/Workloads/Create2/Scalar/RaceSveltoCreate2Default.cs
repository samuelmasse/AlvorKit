using Svelto.ECS;
using static AlvorKit.RaceSveltoCreate2;

namespace AlvorKit;

internal static class RaceSveltoCreate2Default
{
    internal static void Run(RaceSveltoECSBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < entCount; ++i)
                fixture.Factory.BuildEntity<SveltoDescriptor>((uint)i, RaceSveltoECSBaseContext.Group);
            fixture.Scheduler.SubmitEntities();
        }
    }
}

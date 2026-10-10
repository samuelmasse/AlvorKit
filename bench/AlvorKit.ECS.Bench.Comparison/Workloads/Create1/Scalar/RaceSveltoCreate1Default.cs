using Svelto.ECS;
using static AlvorKit.RaceSveltoCreate1;

namespace AlvorKit;

internal static class RaceSveltoCreate1Default
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

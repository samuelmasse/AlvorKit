using Frent;
using Frent.Core;
using static AlvorKit.RaceFrentBaseContext;
using static AlvorKit.RaceFrentCreate2;

namespace AlvorKit;

internal static class RaceFrentCreate2Default
{
    internal static void Run(RaceFrentBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            World world = fixture.World;
            world.EnsureCapacity(_entityType, entCount);

            for (int i = 0; i < entCount; i++)
                world.Create<Component1, Component2>(default, default);
        }
    }
}

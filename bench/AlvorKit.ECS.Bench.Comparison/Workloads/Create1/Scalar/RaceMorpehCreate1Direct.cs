// Retained direct-access variant: these APIs remain available in the current Morpeh package.
#pragma warning disable CS0618
using Scellecs.Morpeh;

namespace AlvorKit;

internal static class RaceMorpehCreate1Direct
{
    internal static void Run(RaceMorpehBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            World world = fixture.World;

            for (int i = 0; i < entCount; ++i)
                world.CreateEntity().AddComponent<RaceMorpehBaseContext.Component1>();
            world.Commit();
        }
    }
}

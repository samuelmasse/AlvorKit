using Scellecs.Morpeh;

namespace AlvorKit;

internal static class RaceMorpehCreate1Stash
{
    internal static void Run(RaceMorpehBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            World world = fixture.World;
            Stash<RaceMorpehBaseContext.Component1> stash1 = world.GetStash<RaceMorpehBaseContext.Component1>();

            for (int i = 0; i < entCount; ++i)
            {
                Entity ent = world.CreateEntity();
                stash1.Add(ent);
            }
            world.Commit();
        }
    }
}

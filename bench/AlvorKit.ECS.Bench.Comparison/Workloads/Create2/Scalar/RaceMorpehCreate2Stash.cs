using Scellecs.Morpeh;

namespace AlvorKit;

internal static class RaceMorpehCreate2Stash
{
    internal static void Run(RaceMorpehBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            World world = fixture.World;
            Stash<RaceMorpehBaseContext.Component1> stash1 = world.GetStash<RaceMorpehBaseContext.Component1>();
            Stash<RaceMorpehBaseContext.Component2> stash2 = world.GetStash<RaceMorpehBaseContext.Component2>();

            for (int i = 0; i < entCount; ++i)
            {
                Entity ent = world.CreateEntity();
                stash1.Add(ent);
                stash2.Add(ent);
            }
            world.Commit();
        }
    }
}

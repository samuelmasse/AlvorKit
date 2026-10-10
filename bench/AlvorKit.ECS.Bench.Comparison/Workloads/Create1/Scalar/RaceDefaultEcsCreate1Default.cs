using DefaultEcs;

namespace AlvorKit;

internal static class RaceDefaultEcsCreate1Default
{
    internal static void Run(RaceDefaultEcsBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < entCount; ++i)
            {
                Entity ent = fixture.World.CreateEntity();
                ent.Set<RaceDefaultEcsBaseContext.Component1>();
            }
        }
    }
}

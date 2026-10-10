using static AlvorKit.RaceFennecsComponents;
using fennecs;

namespace AlvorKit;

internal static class RaceFennecsCreate3Default
{
    internal static void Run(RaceFennecsBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            World world = fixture.World;

            for (int i = 0; i < entCount; ++i)
                world.Spawn().Add<Component1>().Add<Component2>().Add<Component3>();
        }
    }
}

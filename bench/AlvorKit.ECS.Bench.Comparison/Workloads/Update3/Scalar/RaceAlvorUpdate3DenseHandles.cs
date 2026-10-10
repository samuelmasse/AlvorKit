using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorUpdate3;

namespace AlvorKit;

internal static class RaceAlvorUpdate3DenseHandles
{
    internal static void Run(RaceAlvorUpdate3.AlvorKitContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            EntPtr[] ents = fixture.Ents;

            for (int i = 0; i < ents.Length; ++i)
            {
                EntPtr ent = ents[i];
                ent.Component1 = ent.Component1 + ent.Component2 + ent.Component3;
            }
        }
    }
}

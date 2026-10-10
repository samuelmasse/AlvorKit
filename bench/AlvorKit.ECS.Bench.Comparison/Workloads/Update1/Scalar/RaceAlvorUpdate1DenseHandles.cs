using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorUpdate1;

namespace AlvorKit;

internal static class RaceAlvorUpdate1DenseHandles
{
    internal static void Run(RaceAlvorUpdate1.AlvorKitContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            EntPtr[] ents = fixture.Ents;

            for (int i = 0; i < ents.Length; ++i)
            {
                EntPtr ent = ents[i];
                ent.Component1 = ent.Component1 + 1;
            }
        }
    }
}

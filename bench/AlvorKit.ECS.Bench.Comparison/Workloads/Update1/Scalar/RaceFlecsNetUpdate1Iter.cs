using static AlvorKit.RaceFlecsNetComponents;
using Flecs.NET.Core;
using static AlvorKit.RaceFlecsNetUpdate1;

namespace AlvorKit;

internal static class RaceFlecsNetUpdate1Iter
{
    internal static void Run(RaceFlecsNetUpdate1.FlecsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            fixture.query.Run((Iter it) =>
            {
                while (it.Next())
                {
                    var c1 = it.Field<Component1>(0);

                    foreach (int i in it)
                        c1[i].Value++;
                }
            });
        }
    }
}

using static AlvorKit.RaceFlecsNetComponents;
using Flecs.NET.Core;
using static AlvorKit.RaceFlecsNetUpdate3;

namespace AlvorKit;

internal static class RaceFlecsNetUpdate3Iter
{
    internal static void Run(RaceFlecsNetUpdate3.FlecsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            fixture.query.Run((Iter it) =>
            {
                while (it.Next())
                {
                    var c1 = it.Field<Component1>(0);
                    var c2 = it.Field<Component2>(1);
                    var c3 = it.Field<Component3>(2);

                    foreach (int i in it)
                        c1[i].Value += c2[i].Value + c3[i].Value;
                }
            });
        }
    }
}

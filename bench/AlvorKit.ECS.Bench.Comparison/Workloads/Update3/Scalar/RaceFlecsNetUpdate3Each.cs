using static AlvorKit.RaceFlecsNetComponents;
using Flecs.NET.Core;
using static AlvorKit.RaceFlecsNetUpdate3;

namespace AlvorKit;

internal static class RaceFlecsNetUpdate3Each
{
    internal static void Run(RaceFlecsNetUpdate3.FlecsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            fixture.query.Each((ref Component1 c1, ref Component2 c2, ref Component3 c3) =>
            {
                c1.Value += c2.Value + c3.Value;
            });
        }
    }
}

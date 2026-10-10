using static AlvorKit.RaceFlecsNetComponents;
using Flecs.NET.Core;
using static AlvorKit.RaceFlecsNetUpdate2;

namespace AlvorKit;

internal static class RaceFlecsNetUpdate2Each
{
    internal static void Run(RaceFlecsNetUpdate2.FlecsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            fixture.query.Each((ref Component1 c1, ref Component2 c2) =>
            {
                c1.Value += c2.Value;
            });
        }
    }
}

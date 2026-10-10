using static AlvorKit.RaceFlecsNetComponents;
using Flecs.NET.Core;
using static AlvorKit.RaceFlecsNetUpdate1;

namespace AlvorKit;

internal static class RaceFlecsNetUpdate1Each
{
    internal static void Run(RaceFlecsNetUpdate1.FlecsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            fixture.query.Each((ref Component1 c1) =>
            {
                c1.Value += 1;
            });
        }
    }
}

using static AlvorKit.RaceFlecsNetComponents;
using Flecs.NET.Core;
using static AlvorKit.RaceFlecsNetMixed;

namespace AlvorKit;

internal static class RaceFlecsNetMixedEach
{
    internal static void Run(RaceFlecsNetMixed.FlecsContext fixture, int entCount, int passes)
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

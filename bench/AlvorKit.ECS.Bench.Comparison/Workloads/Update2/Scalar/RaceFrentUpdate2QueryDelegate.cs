using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;
using static AlvorKit.RaceFrentUpdate2;

namespace AlvorKit;

internal static class RaceFrentUpdate2QueryDelegate
{
    internal static void Run(RaceFrentUpdate2.FrentContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.Query.Delegate((ref Component1 c1, ref Component2 c2) => c1.Value += c2.Value);
    }
}

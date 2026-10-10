using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;
using static AlvorKit.RaceFrentUpdate3;

namespace AlvorKit;

internal static class RaceFrentUpdate3QueryDelegate
{
    internal static void Run(RaceFrentUpdate3.FrentContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.Query.Delegate((ref Component1 c1, ref Component2 c2, ref Component3 c3) => c1.Value += c2.Value + c3.Value);
    }
}

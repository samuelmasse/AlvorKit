using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;
using static AlvorKit.RaceFrentUpdate3;

namespace AlvorKit;

internal static class RaceFrentUpdate3QueryInline
{
    internal static void Run(RaceFrentUpdate3.FrentContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.Query.Inline<Sum, Component1, Component2, Component3>(default);
    }
}

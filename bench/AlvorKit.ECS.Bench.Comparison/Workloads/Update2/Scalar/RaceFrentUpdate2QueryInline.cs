using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;
using static AlvorKit.RaceFrentUpdate2;

namespace AlvorKit;

internal static class RaceFrentUpdate2QueryInline
{
    internal static void Run(RaceFrentUpdate2.FrentContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.Query.Inline<Sum, Component1, Component2>(default);
    }
}

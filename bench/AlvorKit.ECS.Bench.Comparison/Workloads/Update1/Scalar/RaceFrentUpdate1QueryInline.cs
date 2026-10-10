using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;
using static AlvorKit.RaceFrentUpdate1;

namespace AlvorKit;

internal static class RaceFrentUpdate1QueryInline
{
    internal static void Run(RaceFrentUpdate1.FrentContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.Query.Inline<Increment, Component1>(default);
    }
}

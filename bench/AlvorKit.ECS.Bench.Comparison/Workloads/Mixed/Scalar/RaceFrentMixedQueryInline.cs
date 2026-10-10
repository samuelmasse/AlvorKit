using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;
using static AlvorKit.RaceFrentMixed;

namespace AlvorKit;

internal static class RaceFrentMixedQueryInline
{
    internal static void Run(RaceFrentMixed.FrentContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            fixture.Query.Inline<Sum, Component1, Component2>(default);
    }
}

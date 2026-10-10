using Arch.Core;
using System.Runtime.CompilerServices;
using Arch.Core.Utils;
using Arch.System;
using static AlvorKit.RaceArchComponents;
using static AlvorKit.RaceArchUpdate3;

namespace AlvorKit;

internal static class RaceArchUpdate3ScalarSourceGenerated
{
    internal static void Run(RaceArchUpdate3.ArchContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            ForEachQuery(fixture.World);
    }
}

using Arch.Core;
using System.Runtime.CompilerServices;
using Arch.Core.Utils;
using Arch.System;
using static AlvorKit.RaceArchComponents;
using static AlvorKit.RaceArchUpdate2;

namespace AlvorKit;

internal static class RaceArchUpdate2ScalarSourceGenerated
{
    internal static void Run(RaceArchUpdate2.ArchContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            ForEachQuery(fixture.World);
    }
}

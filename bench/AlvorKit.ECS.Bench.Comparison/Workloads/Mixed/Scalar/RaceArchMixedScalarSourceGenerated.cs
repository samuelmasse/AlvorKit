using Arch.Core;
using System.Linq;
using System.Runtime.CompilerServices;
using Arch.Core.Utils;
using Arch.System;
using static AlvorKit.RaceArchComponents;
using static AlvorKit.RaceArchMixed;

namespace AlvorKit;

internal static class RaceArchMixedScalarSourceGenerated
{
    internal static void Run(RaceArchMixed.ArchContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
            ForEachQuery(fixture.World);
    }
}

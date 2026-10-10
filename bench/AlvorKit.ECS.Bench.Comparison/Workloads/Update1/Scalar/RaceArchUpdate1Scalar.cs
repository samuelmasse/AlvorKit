using Arch.Core;
using System.Runtime.CompilerServices;
using Arch.Core.Utils;
using Arch.System;
using static AlvorKit.RaceArchComponents;
using static AlvorKit.RaceArchUpdate1;

namespace AlvorKit;

internal static class RaceArchUpdate1Scalar
{
    internal static void Run(RaceArchUpdate1.ArchContext fixture, int entCount, int passes)
    {
        ForEach1 _forEach = default;

        for (var pass = 0; pass < passes; pass++)
        {
            World world = fixture.World;
            world.InlineQuery<ForEach1, Component1>(_queryDescription, ref _forEach);
        }
    }
}

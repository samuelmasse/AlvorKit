using Arch.Core;
using System.Runtime.CompilerServices;
using Arch.Core.Utils;
using Arch.System;
using static AlvorKit.RaceArchComponents;
using static AlvorKit.RaceArchUpdate3;

namespace AlvorKit;

internal static class RaceArchUpdate3Scalar
{
    internal static void Run(RaceArchUpdate3.ArchContext fixture, int entCount, int passes)
    {
        ForEach3 _forEach3 = default;

        for (var pass = 0; pass < passes; pass++)
        {
            World world = fixture.World;
            world.InlineQuery<ForEach3, Component1, Component2, Component3>(_queryDescription, ref _forEach3);
        }
    }
}

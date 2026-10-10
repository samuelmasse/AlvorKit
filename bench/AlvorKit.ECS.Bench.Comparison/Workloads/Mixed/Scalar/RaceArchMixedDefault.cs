using Arch.Core;
using System.Linq;
using System.Runtime.CompilerServices;
using Arch.Core.Utils;
using Arch.System;
using static AlvorKit.RaceArchComponents;
using static AlvorKit.RaceArchMixed;

namespace AlvorKit;

internal static class RaceArchMixedDefault
{
    internal static void Run(RaceArchMixed.ArchContext fixture, int entCount, int passes)
    {
        ForEach2 _forEach2 = default;

        for (var pass = 0; pass < passes; pass++)
        {
            World world = fixture.World;
            world.InlineQuery<ForEach2, Component1, Component2>(in _queryDescription, ref _forEach2);
        }
    }
}

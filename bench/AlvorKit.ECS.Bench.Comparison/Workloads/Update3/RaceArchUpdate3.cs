using Arch.Core;
using System.Runtime.CompilerServices;
using Arch.Core.Utils;
using Arch.System;
using static AlvorKit.RaceArchComponents;

namespace AlvorKit;

internal static partial class RaceArchUpdate3
{
    private static readonly ComponentType[] _filterStorage = [typeof(Component1), typeof(Component2), typeof(Component3)];
    private static readonly QueryDescription _queryDescriptionStorage = new(all: new Signature(_filter));

    internal static ref readonly ComponentType[] _filter => ref _filterStorage;
    internal static ref readonly QueryDescription _queryDescription => ref _queryDescriptionStorage;
    internal readonly struct ForEach3 : IForEach<Component1, Component2, Component3>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ref Component1 t0, ref Component2 t1, ref Component3 t2)
        {
            t0.Value += t1.Value + t2.Value;
        }
    }
    [Query]
    internal static void ForEach(ref Component1 t0, Component2 t1, Component3 t2)
    {
        t0.Value += t1.Value + t2.Value;
    }

    internal class ArchContext(int entCount, int paddingCount) : RaceArchBaseContext(_filter, entCount, paddingCount)
    {
    }
}

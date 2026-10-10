using Arch.Core;
using System.Runtime.CompilerServices;
using Arch.Core.Utils;
using Arch.System;
using static AlvorKit.RaceArchComponents;

namespace AlvorKit;

internal static partial class RaceArchUpdate1
{
    private static readonly ComponentType[] _filterStorage = [typeof(Component1)];
    private static readonly QueryDescription _queryDescriptionStorage = new(all: new Signature(_filter));

    internal static ref readonly ComponentType[] _filter => ref _filterStorage;
    internal static ref readonly QueryDescription _queryDescription => ref _queryDescriptionStorage;
    internal readonly struct ForEach1 : IForEach<Component1>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ref Component1 t0)
        {
            ++t0.Value;
        }
    }
    [Query]
    internal static void ForEach(ref Component1 t0)
    {
        ++t0.Value;
    }

    internal class ArchContext(int entCount, int paddingCount) : RaceArchBaseContext(_filter, entCount, paddingCount)
    {
    }
}

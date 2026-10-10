using Arch.Core;
using System.Linq;
using System.Runtime.CompilerServices;
using Arch.Core.Utils;
using Arch.System;
using static AlvorKit.RaceArchComponents;

namespace AlvorKit;

internal static partial class RaceArchMixed
{
    private static readonly ComponentType[] _filterStorage = [typeof(Component1), typeof(Component2)];
    private static readonly QueryDescription _queryDescriptionStorage = new(all: new Signature(_filter));

    internal static ref readonly ComponentType[] _filter => ref _filterStorage;
    internal static ref readonly QueryDescription _queryDescription => ref _queryDescriptionStorage;
    internal readonly struct ForEach2 : IForEach<Component1, Component2>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ref Component1 t0, ref Component2 t1)
        {
            t0.Value += t1.Value;
        }
    }
    [Query]
    internal static void ForEach(ref Component1 t0, Component2 t1)
    {
        t0.Value += t1.Value;
    }

    internal class ArchContext : RaceArchBaseContext
    {
        public ArchContext(int entCount)
        {
            ComponentType[] paddingTypes = [typeof(Padding1), typeof(Padding2), typeof(Padding3), typeof(Padding4)];
            ComponentType[][] archetypes = paddingTypes.Select(t => _filter.Concat(Enumerable.Repeat(t, 1)).ToArray()).ToArray();

            foreach (ComponentType[] archetype in archetypes)
                World.EnsureCapacity(archetype, entCount / 4);

            for (int index = 0; index < entCount; index++)
            {
                var ent = World.Create(archetypes[index % archetypes.Length]);
                World.Set(ent, new Component2 { Value = 1 });
            }
        }

        private record struct Padding1();
        private record struct Padding2();
        private record struct Padding3();
        private record struct Padding4();
    }
}

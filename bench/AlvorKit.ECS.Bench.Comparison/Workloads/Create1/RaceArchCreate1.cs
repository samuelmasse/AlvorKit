using Arch.Core;
using Arch.Core.Utils;
using static AlvorKit.RaceArchComponents;

namespace AlvorKit;

internal static class RaceArchCreate1
{
    private static readonly ComponentType[] _archetypeStorage = [typeof(Component1)];

    internal static ref readonly ComponentType[] _archetype => ref _archetypeStorage;
}

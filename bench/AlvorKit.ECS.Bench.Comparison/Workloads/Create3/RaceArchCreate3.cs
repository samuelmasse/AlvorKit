using Arch.Core;
using Arch.Core.Utils;
using static AlvorKit.RaceArchComponents;

namespace AlvorKit;

internal static class RaceArchCreate3
{
    private static readonly ComponentType[] _archetypeStorage = [typeof(Component1), typeof(Component2), typeof(Component3)];

    internal static ref readonly ComponentType[] _archetype => ref _archetypeStorage;
}

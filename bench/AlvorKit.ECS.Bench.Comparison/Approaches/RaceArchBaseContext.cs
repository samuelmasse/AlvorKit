using Arch.Core;
using Arch.Core.Utils;

namespace AlvorKit;

internal class RaceArchBaseContext : IDisposable
{
    private readonly World _world;

    public World World => _world;

    public RaceArchBaseContext()
    {
        _world = World.Create();
    }

    public RaceArchBaseContext(ComponentType[] archetype, int amount, int paddingCount)
    {
        _world = World.Create();
        World.EnsureCapacity(archetype, amount);

        for (int index = 0; index < amount; index++)
        {
            for (var p = 0; p < paddingCount; p++)
                World.Create(new Signature(typeof(ComparisonPadding)));
            var ent = World.Create(archetype);

            if (archetype.Length >= 2)
                World.Set(ent, new RaceArchComponents.Component2 { Value = 1 });

            if (archetype.Length >= 3)
                World.Set(ent, new RaceArchComponents.Component3 { Value = 1 });
        }
    }

    public virtual void Dispose()
    {
        World.Destroy(World);
    }
}

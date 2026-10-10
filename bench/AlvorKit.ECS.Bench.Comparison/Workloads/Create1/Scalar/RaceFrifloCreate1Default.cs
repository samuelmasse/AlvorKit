using static AlvorKit.RaceFrifloEngineComponents;
using Friflo.Engine.ECS;

namespace AlvorKit;

internal static class RaceFrifloCreate1Default
{
    internal static void Run(RaceFrifloCreateContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            EntityStore store = fixture.Store;
            store.EnsureCapacity(entCount);
            Archetype archetype = store.GetArchetype(ComponentTypes.Get<Component1>());
            archetype.EnsureCapacity(entCount);

            for (int i = 0; i < entCount; ++i)
                archetype.CreateEntity();
        }
    }
}

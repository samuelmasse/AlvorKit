using static AlvorKit.RaceFrifloEngineComponents;
using Friflo.Engine.ECS;

namespace AlvorKit;

internal class RaceFrifloEngineEcsBaseContext
{
    private readonly EntityStore _entityStore;
    private readonly ArchetypeQuery<Component1> _queryOne;
    private readonly ArchetypeQuery<Component1, Component2> _queryTwo;
    private readonly ArchetypeQuery<Component1, Component2, Component3> _queryThree;

    internal EntityStore EntityStore => _entityStore;
    public ref readonly ArchetypeQuery<Component1> queryOne => ref _queryOne;
    public ref readonly ArchetypeQuery<Component1, Component2> queryTwo => ref _queryTwo;
    public ref readonly ArchetypeQuery<Component1, Component2, Component3> queryThree => ref _queryThree;

    protected RaceFrifloEngineEcsBaseContext()
    {
        _entityStore = new EntityStore(PidType.UsePidAsId);
        _queryOne = EntityStore.Query<Component1>();
        _queryTwo = EntityStore.Query<Component1, Component2>();
        _queryThree = EntityStore.Query<Component1, Component2, Component3>();
    }

    protected RaceFrifloEngineEcsBaseContext(int entCount, int padding, ComponentTypes componentTypes) : this()
    {
        EntityStore.EnsureCapacity(entCount + (padding * entCount));
        Archetype archetype = EntityStore.GetArchetype(componentTypes);
        archetype.EnsureCapacity(entCount);

        for (int index = 0; index < entCount; index++)
        {
            for (int n = 0; n < padding; n++)
                EntityStore.CreateEntity().AddComponent<ComparisonPadding>();
            archetype.CreateEntity();
        }
        foreach (var (values, _) in EntityStore.Query<Component2>().Chunks)
        {
            foreach (ref var value in values.Span)
                value.Value = 1;
        }

        foreach (var (values, _) in EntityStore.Query<Component3>().Chunks)
        {
            foreach (ref var value in values.Span)
                value.Value = 1;
        }
    }

}

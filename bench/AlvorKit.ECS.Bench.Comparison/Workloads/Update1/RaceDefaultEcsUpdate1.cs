using DefaultEcs;
using DefaultEcs.System;

namespace AlvorKit;

internal static partial class RaceDefaultEcsUpdate1
{
    internal partial class DefaultEcsContext : RaceDefaultEcsBaseContext
    {
        private readonly ISystem<int> _monoThreadComponentSystem;
        private readonly ISystem<int> _monoThreadEntitySetSystem;

        public ISystem<int> MonoThreadComponentSystem => _monoThreadComponentSystem;
        public ISystem<int> MonoThreadEntitySetSystem => _monoThreadEntitySetSystem;

        public DefaultEcsContext(int entCount, int paddingCount)
        {
            _monoThreadComponentSystem = new ComponentSystem(World);
            _monoThreadEntitySetSystem = new EntitySetSystem(World);

            for (int i = 0; i < entCount; ++i)
            {
                for (int j = 0; j < paddingCount; ++j)
                    World.CreateEntity().Set<ComparisonPadding>();
                Entity ent = World.CreateEntity();
                ent.Set<Component1>();
            }
        }

        private class ComponentSystem : AComponentSystem<int, Component1>
        {
            public ComponentSystem(World world) : base(world)
            {
            }

            protected override void Update(int state, Span<Component1> components)
            {
                foreach (ref Component1 component in components)
                    ++component.Value;
            }
        }

        private partial class EntitySetSystem : AEntitySetSystem<int>
        {
            [Update]
            private static void Update(ref Component1 component)
            {
                ++component.Value;
            }
        }

    }
}

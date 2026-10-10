using DefaultEcs;
using DefaultEcs.System;

namespace AlvorKit;

internal static partial class RaceDefaultEcsUpdate3
{
    internal partial class DefaultEcsContext : RaceDefaultEcsBaseContext
    {
        private readonly ISystem<int> _monoThreadEntitySetSystem;

        public ISystem<int> MonoThreadEntitySetSystem => _monoThreadEntitySetSystem;

        public DefaultEcsContext(int entCount, int paddingCount)
        {
            _monoThreadEntitySetSystem = new EntitySetSystem(World);

            for (int i = 0; i < entCount; ++i)
            {
                for (int j = 0; j < paddingCount; ++j)
                    World.CreateEntity().Set<ComparisonPadding>();
                Entity ent = World.CreateEntity();
                ent.Set<Component1>();
                ent.Set(new Component2 { Value = 1 });
                ent.Set(new Component3 { Value = 1 });
            }
        }

        private partial class EntitySetSystem : AEntitySetSystem<int>
        {
            [Update]
            private static void Update(ref Component1 c1, in Component2 c2, in Component3 c3)
            {
                c1.Value += c2.Value + c3.Value;
            }
        }

    }
}

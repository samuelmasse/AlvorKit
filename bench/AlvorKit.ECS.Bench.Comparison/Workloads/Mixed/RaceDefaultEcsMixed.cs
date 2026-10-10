using DefaultEcs;
using DefaultEcs.System;

namespace AlvorKit;

internal static partial class RaceDefaultEcsMixed
{
    internal partial class DefaultEcsContext : RaceDefaultEcsBaseContext
    {
        private readonly ISystem<int> _monoThreadEntitySetSystem;

        public ISystem<int> MonoThreadEntitySetSystem => _monoThreadEntitySetSystem;

        public DefaultEcsContext(int entCount)
        {
            _monoThreadEntitySetSystem = new EntitySetSystem(World);

            for (int i = 0; i < entCount; ++i)
            {
                Entity ent = World.CreateEntity();
                ent.Set<Component1>();
                ent.Set(new Component2 { Value = 1 });

                switch (i % 4)
                {
                    case 0:
                        ent.Set<Padding1>();
                        break;
                    case 1:
                        ent.Set<Padding2>();
                        break;
                    case 2:
                        ent.Set<Padding3>();
                        break;
                    case 3:
                        ent.Set<Padding4>();
                        break;
                }
            }
        }

        private record struct Padding1();
        private record struct Padding2();
        private record struct Padding3();
        private record struct Padding4();
        private partial class EntitySetSystem : AEntitySetSystem<int>
        {
            [Update]
            private static void Update(ref Component1 c1, in Component2 c2)
            {
                c1.Value += c2.Value;
            }
        }

    }
}

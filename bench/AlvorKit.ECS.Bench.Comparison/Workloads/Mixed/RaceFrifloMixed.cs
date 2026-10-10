using static AlvorKit.RaceFrifloEngineComponents;
using Friflo.Engine.ECS;

namespace AlvorKit;

internal static class RaceFrifloMixed
{
    internal class FrifloEngineEcsContext : RaceFrifloEngineEcsBaseContext
    {
        public FrifloEngineEcsContext(int entCount)
        {
            for (int i = 0; i < entCount; ++i)
            {
                Entity ent = EntityStore.CreateEntity();
                ent.AddComponent<Component1>();
                ent.AddComponent(new Component2 { Value = 1 });

                switch (i % 4)
                {
                    case 0:
                        ent.AddComponent<Padding1>();
                        break;
                    case 1:
                        ent.AddComponent<Padding2>();
                        break;
                    case 2:
                        ent.AddComponent<Padding3>();
                        break;
                    case 3:
                        ent.AddComponent<Padding4>();
                        break;
                }
            }
        }

        private record struct Padding1 : global::Friflo.Engine.ECS.IComponent
        {
        }

        private record struct Padding2 : global::Friflo.Engine.ECS.IComponent
        {
        }

        private record struct Padding3 : global::Friflo.Engine.ECS.IComponent
        {
        }

        private record struct Padding4 : global::Friflo.Engine.ECS.IComponent
        {
        }

    }

    internal static void Update(ref Component1 c1, ref Component2 c2)
    {
        c1.Value += c2.Value;
    }
}

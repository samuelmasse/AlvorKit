using Svelto.DataStructures;
using Svelto.ECS;

namespace AlvorKit;

internal static class RaceSveltoMixed
{
    internal class SveltoECSContext : RaceSveltoECSBaseContext
    {
        private readonly SveltoEngine _engine;

        public SveltoEngine Engine => _engine;

        public SveltoECSContext(int entCount)
        {
            _engine = new SveltoEngine();
            Root.AddEngine(Engine);
            uint id = 0;

            for (int i = 0; i < entCount; ++i)
            {
                EntityInitializer ent = (i % 4) switch
                {
                    0 => Factory.BuildEntity<EntDescriptor1>(id++, Group),
                    1 => Factory.BuildEntity<EntDescriptor2>(id++, Group),
                    2 => Factory.BuildEntity<EntDescriptor3>(id++, Group),
                    _ => Factory.BuildEntity<EntDescriptor4>(id++, Group)};
                ent.Get<Component2>() = new Component2
                {
                    Value = 1
                };
            }
            Scheduler.SubmitEntities();
        }

        private record struct Padding1() : IEntityComponent;
        private record struct Padding2() : IEntityComponent;
        private record struct Padding3() : IEntityComponent;
        private record struct Padding4() : IEntityComponent;
        private class EntDescriptor1 : GenericEntityDescriptor<Component1, Component2, Padding1>
        {
        }

        private class EntDescriptor2 : GenericEntityDescriptor<Component1, Component2, Padding2>
        {
        }

        private class EntDescriptor3 : GenericEntityDescriptor<Component1, Component2, Padding3>
        {
        }

        private class EntDescriptor4 : GenericEntityDescriptor<Component1, Component2, Padding4>
        {
        }

        private class EntDescriptor : GenericEntityDescriptor<Component1, Component2>
        {
        }

        public class SveltoEngine : IQueryingEntitiesEngine
        {
            private EntitiesDB _entitiesDB = null!;

            public EntitiesDB entitiesDB { get => _entitiesDB; set => _entitiesDB = value; }
            public void Ready()
            {
            }

            public void Update()
            {
                (NB<Component1> c1, NB<Component2> c2, int count) = entitiesDB.QueryEntities<Component1, Component2>(Group);

                for (int i = 0; i < count; i++)
                    c1[i].Value += c2[i].Value;
            }
        }
    }
}

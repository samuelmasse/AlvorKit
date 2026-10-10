using Svelto.DataStructures;
using Svelto.ECS;

namespace AlvorKit;

internal static class RaceSveltoUpdate2
{
    internal class SveltoECSContext : RaceSveltoECSBaseContext
    {
        private readonly SveltoEngine _engine;

        public SveltoEngine Engine => _engine;

        public SveltoECSContext(int entCount, int paddingCount)
        {
            _engine = new SveltoEngine();
            Root.AddEngine(Engine);
            uint id = 0;

            for (int i = 0; i < entCount; ++i)
            {
                for (int j = 0; j < paddingCount; ++j)
                    Factory.BuildEntity<ComparisonSveltoPadding>(id++, Group);
                EntityInitializer ent = Factory.BuildEntity<EntDescriptor>(id++, Group);
                ent.Get<Component2>() = new Component2
                {
                    Value = 1
                };
            }
            Scheduler.SubmitEntities();
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

        public class EntDescriptor : GenericEntityDescriptor<Component1, Component2>
        {
        }
    }
}

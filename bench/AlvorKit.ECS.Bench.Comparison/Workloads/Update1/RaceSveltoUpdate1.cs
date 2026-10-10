using Svelto.DataStructures;
using Svelto.ECS;

namespace AlvorKit;

internal static class RaceSveltoUpdate1
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
                Factory.BuildEntity<EntDescriptor>(id++, Group);
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
                (NB<Component1> entityViews, int count) = entitiesDB.QueryEntities<Component1>(Group);

                for (int i = 0; i < count; i++)
                    ++entityViews[i].Value;
            }
        }

        public class PaddingDescriptor : IEntityDescriptor
        {
            public IComponentBuilder[] componentsToBuild => Array.Empty<IComponentBuilder>();
        }

        public class EntDescriptor : GenericEntityDescriptor<Component1>
        {
        }
    }
}

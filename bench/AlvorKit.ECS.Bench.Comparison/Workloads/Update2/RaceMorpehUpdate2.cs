// Retained direct-access variant: these APIs remain available in the current Morpeh package.
#pragma warning disable CS0618
using Scellecs.Morpeh;

namespace AlvorKit;

internal static class RaceMorpehUpdate2
{
    internal class MorpehContext : RaceMorpehBaseContext
    {
        private readonly ISystem _monoThreadDirectSystem;
        private readonly ISystem _monoThreadStashSystem;

        public ISystem MonoThreadDirectSystem => _monoThreadDirectSystem;
        public ISystem MonoThreadStashSystem => _monoThreadStashSystem;

        public MorpehContext(int entCount, int paddingCount)
        {
            _monoThreadDirectSystem = new DirectSystem
            {
                World = World
            };
            MonoThreadDirectSystem.OnAwake();
            _monoThreadStashSystem = new StashSystem
            {
                World = World
            };
            MonoThreadStashSystem.OnAwake();

            for (int i = 0; i < entCount; ++i)
            {
                for (int j = 0; j < paddingCount; ++j)
                    World.GetStash<ComparisonPadding>().Add(World.CreateEntity());
                Entity ent = World.CreateEntity();
                ent.AddComponent<Component1>();
                ent.SetComponent(new Component2 { Value = 1 });
            }
            World.Commit();
        }

        private class DirectSystem : ISystem
        {
            private Filter _filter = null!;
            private World _world = null!;

            public World World { get => _world; set => _world = value; }
            void IDisposable.Dispose()
            {
            }

            public void OnAwake()
            {
                _filter = World.Filter.With<Component1>().With<Component2>().Build();
            }

            public void OnUpdate(float deltaTime)
            {
                foreach (Entity ent in _filter)
                    ent.GetComponent<Component1>().Value += ent.GetComponent<Component2>().Value;
            }
        }

        private class StashSystem : ISystem
        {
            private Stash<Component1> _stash1 = null!;
            private Stash<Component2> _stash2 = null!;
            private Filter _filter = null!;
            private World _world = null!;

            public World World { get => _world; set => _world = value; }
            public void OnAwake()
            {
                _stash1 = World.GetStash<Component1>();
                _stash2 = World.GetStash<Component2>();
                _filter = World.Filter.With<Component1>().With<Component2>().Build();
            }

            public void OnUpdate(float deltaTime)
            {
                foreach (Entity ent in _filter)
                    _stash1.Get(ent).Value += _stash2.Get(ent).Value;
            }

            public void Dispose()
            {
                _stash1.Dispose();
                _stash2.Dispose();
            }
        }
    }
}

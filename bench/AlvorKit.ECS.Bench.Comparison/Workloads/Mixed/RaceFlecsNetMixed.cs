using static AlvorKit.RaceFlecsNetComponents;
using Flecs.NET.Core;

namespace AlvorKit;

internal static class RaceFlecsNetMixed
{
    internal class FlecsContext : RaceFlecsNetBaseContext
    {
        private Query<Component1, Component2> _query;

        public ref Query<Component1, Component2> query => ref _query;

        public FlecsContext(int entCount)
        {
            for (int i = 0; i < entCount; ++i)
            {
                Entity ent = World.Entity().Set(new Component1()).Set(new Component2 { Value = 1 });

                switch (i % 4)
                {
                    case 0:
                        ent.Add<Padding1>();
                        break;
                    case 1:
                        ent.Add<Padding2>();
                        break;
                    case 2:
                        ent.Add<Padding3>();
                        break;
                    case 3:
                        ent.Add<Padding4>();
                        break;
                }
            }
            _query = World.QueryBuilder<Component1, Component2>().Build();
        }

        private record struct Padding1();
        private record struct Padding2();
        private record struct Padding3();
        private record struct Padding4();
    }
}

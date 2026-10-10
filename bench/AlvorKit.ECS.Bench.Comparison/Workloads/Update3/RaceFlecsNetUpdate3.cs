using static AlvorKit.RaceFlecsNetComponents;
using Flecs.NET.Core;

namespace AlvorKit;

internal static class RaceFlecsNetUpdate3
{
    internal class FlecsContext : RaceFlecsNetBaseContext
    {
        private Query<Component1, Component2, Component3> _query;

        public ref Query<Component1, Component2, Component3> query => ref _query;

        public FlecsContext(int entCount, int paddingCount)
        {
            for (int i = 0; i < entCount; ++i)
            {
                for (int j = 0; j < paddingCount; ++j)
                    World.Entity().Add<ComparisonPadding>();
                World.Entity().Set(new Component1()).Set(new Component2 { Value = 1 }).Set(new Component3 { Value = 1 });
            }
            _query = World.QueryBuilder<Component1, Component2, Component3>().Build();
        }
    }
}

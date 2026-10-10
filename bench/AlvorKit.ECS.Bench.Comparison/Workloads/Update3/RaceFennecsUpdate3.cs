using System.Runtime.CompilerServices;
using static AlvorKit.RaceFennecsComponents;
using fennecs;

namespace AlvorKit;

internal static class RaceFennecsUpdate3
{
    internal class FennecsContext : RaceFennecsBaseContext
    {
        private Stream<Component1, Component2, Component3> _query;

        public ref Stream<Component1, Component2, Component3> query => ref _query;

        public FennecsContext(int entCount, int paddingCount)
        {
            _query = World.Stream<Component1, Component2, Component3>();

            for (int i = 0; i < entCount; ++i)
            {
                for (int j = 0; j < paddingCount; ++j)
                    World.Spawn().Add<ComparisonPadding>();
                World.Spawn().Add<Component1>().Add(new Component2 { Value = 1 }).Add(new Component3 { Value = 1 });
            }
        }
    }
}

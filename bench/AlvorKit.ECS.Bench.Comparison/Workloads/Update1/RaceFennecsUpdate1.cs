using System.Runtime.CompilerServices;
using static AlvorKit.RaceFennecsComponents;
using fennecs;

namespace AlvorKit;

internal static class RaceFennecsUpdate1
{
    internal class FennecsContext : RaceFennecsBaseContext
    {
        private Stream<Component1> _query;

        public ref Stream<Component1> query => ref _query;

        public FennecsContext(int entCount, int paddingCount)
        {
            _query = World.Stream<Component1>();

            for (int i = 0; i < entCount; ++i)
            {
                for (int j = 0; j < paddingCount; ++j)
                    World.Spawn().Add<ComparisonPadding>();
                World.Spawn().Add<Component1>();
            }
        }
    }
}

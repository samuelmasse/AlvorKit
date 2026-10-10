using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;

namespace AlvorKit;

internal static class RaceFrentUpdate1
{
    internal class FrentContext : RaceFrentBaseContext
    {
        private Query _query;

        public ref Query Query => ref _query;

        public FrentContext(int entCount, int paddingCount)
        {
            for (int i = 0; i < entCount; i++)
            {
                World.Create<Component1>(default);

                for (int j = 0; j < paddingCount; j++)
                    World.Create<ComparisonPadding>(default);
            }
            _query = World.Query<Component1>();
        }
    }

    internal readonly struct Increment : IAction<Component1>
    {
        public void Run(ref Component1 t0)
        {
            t0.Value++;
        }
    }
}

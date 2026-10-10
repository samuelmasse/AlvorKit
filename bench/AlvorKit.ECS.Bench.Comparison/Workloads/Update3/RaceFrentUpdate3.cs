using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;

namespace AlvorKit;

internal static class RaceFrentUpdate3
{
    internal class FrentContext : RaceFrentBaseContext
    {
        private Query _query;

        public ref Query Query => ref _query;

        public FrentContext(int entCount, int paddingCount)
        {
            for (int i = 0; i < entCount; i++)
            {
                World.Create<Component1, Component2, Component3>(default, new() { Value = 1 }, new() { Value = 1 });

                for (var padding = 0; padding < paddingCount; padding++)
                    World.Create<ComparisonPadding>(default);
            }

            _query = World.Query<Component1, Component2, Component3>();
        }
    }

    internal readonly struct Sum : IAction<Component1, Component2, Component3>
    {
        public void Run(ref Component1 t0, ref Component2 t1, ref Component3 t2)
        {
            t0.Value += t1.Value + t2.Value;
        }
    }
}

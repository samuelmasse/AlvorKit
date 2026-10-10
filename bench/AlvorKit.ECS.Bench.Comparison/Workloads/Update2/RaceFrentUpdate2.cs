using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;

namespace AlvorKit;

internal static class RaceFrentUpdate2
{
    internal class FrentContext : RaceFrentBaseContext
    {
        private Query _query;

        public ref Query Query => ref _query;

        public FrentContext(int entCount, int padding)
        {
            for (int i = 0; i < entCount; i++)
            {
                World.Create<Component1, Component2>(default, new() { Value = 1 });

                for (int j = 0; j < padding; j++)
                    World.Create<ComparisonPadding>(default);
            }
            _query = World.Query<Component1, Component2>();
        }
    }

    internal readonly struct Sum : IAction<Component1, Component2>
    {
        public void Run(ref Component1 t0, ref Component2 t1)
        {
            t0.Value += t1.Value;
        }
    }
}

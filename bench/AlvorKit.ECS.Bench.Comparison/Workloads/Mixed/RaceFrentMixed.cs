using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;

namespace AlvorKit;

internal static class RaceFrentMixed
{
    internal class FrentContext : RaceFrentBaseContext
    {
        private Query _query;

        public ref Query Query => ref _query;

        public FrentContext(int entCount)
        {
            for (int i = 0; i < entCount; i++)
            {
                Entity e = (i % 4) switch
                {
                    0 => World.Create<Component1, Component2, Padding1>(default, new() { Value = 1 }, default),
                    1 => World.Create<Component1, Component2, Padding2>(default, new() { Value = 1 }, default),
                    2 => World.Create<Component1, Component2, Padding3>(default, new() { Value = 1 }, default),
                    _ => World.Create<Component1, Component2, Padding4>(default, new() { Value = 1 }, default),
                };
            }
            _query = World.Query<Component1, Component2>();
        }

        internal readonly struct Padding1;
        internal readonly struct Padding2;
        internal readonly struct Padding3;
        internal readonly struct Padding4;
    }

    internal readonly struct Sum : IAction<Component1, Component2>
    {
        public void Run(ref Component1 t0, ref Component2 t1)
        {
            t0.Value += t1.Value;
        }
    }
}

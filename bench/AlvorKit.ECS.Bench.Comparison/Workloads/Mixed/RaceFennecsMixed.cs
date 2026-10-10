using static AlvorKit.RaceFennecsComponents;
using fennecs;

namespace AlvorKit;

internal static class RaceFennecsMixed
{
    internal class FennecsContext : RaceFennecsBaseContext
    {
        private Stream<Component1, Component2> _query;

        public ref Stream<Component1, Component2> query => ref _query;

        public FennecsContext(int entCount)
        {
            _query = World.Stream<Component1, Component2>();

            for (int i = 0; i < entCount; ++i)
            {
                Entity ent = World.Spawn().Add<Component1>().Add(new Component2 { Value = 1 });

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
        }

        private record struct Padding1();
        private record struct Padding2();
        private record struct Padding3();
        private record struct Padding4();
    }
}

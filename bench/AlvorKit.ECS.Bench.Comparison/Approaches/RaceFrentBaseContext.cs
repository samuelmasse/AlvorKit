using Frent;

namespace AlvorKit;

internal class RaceFrentBaseContext : IDisposable
{
    private readonly World _world = new();

    public World World => _world;
    internal struct Component1
    {
        public int Value;
    }

    internal struct Component2
    {
        public int Value;
    }

    internal struct Component3
    {
        public int Value;
    }

    public void Dispose() => World.Dispose();
}

using DefaultEcs;

namespace AlvorKit;

internal class RaceDefaultEcsBaseContext : IDisposable
{
    private readonly World _world;

    public World World => _world;

    public RaceDefaultEcsBaseContext()
    {
        _world = new World();
    }

    public struct Component1
    {
        public int Value;
    }

    public struct Component2
    {
        public int Value;
    }

    public struct Component3
    {
        public int Value;
    }

    public virtual void Dispose()
    {
        World.Dispose();
    }
}

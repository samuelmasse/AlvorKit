using Scellecs.Morpeh;

namespace AlvorKit;

internal class RaceMorpehBaseContext : IDisposable
{
    private readonly World _world;

    public World World => _world;

    public RaceMorpehBaseContext()
    {
        _world = World.Create();
    }

    public struct Component1 : global::Scellecs.Morpeh.IComponent
    {
        public int Value;
    }

    public struct Component2 : global::Scellecs.Morpeh.IComponent
    {
        public int Value;
    }

    public struct Component3 : global::Scellecs.Morpeh.IComponent
    {
        public int Value;
    }

    public virtual void Dispose()
    {
        World.Dispose();
    }
}

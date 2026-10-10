using Flecs.NET.Core;

namespace AlvorKit;

internal class RaceFlecsNetBaseContext : IDisposable
{
    private readonly World _world;

    public World World => _world;

    public RaceFlecsNetBaseContext()
    {
        _world = World.Create();
    }

    public void Dispose()
    {
        World.Dispose();
    }
}

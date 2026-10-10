using fennecs;

namespace AlvorKit;

internal class RaceFennecsBaseContext : IDisposable
{
    private readonly World _world;

    public World World => _world;

    public RaceFennecsBaseContext()
    {
        _world = new World();
    }

    public void Dispose()
    {
        World.Dispose();
    }
}

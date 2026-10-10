namespace AlvorKit;

internal class RaceAlvorKitBaseContext : IDisposable
{
    private readonly EntArena _arena;

    public EntArena Arena => _arena;

    public RaceAlvorKitBaseContext()
    {
        _arena = new EntArena();
    }

    public void Dispose()
    {
        Arena.Dispose();
    }
}

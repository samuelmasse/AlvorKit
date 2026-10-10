namespace AlvorKit;

internal class RaceFrifloCreateContext : IDisposable
{
    private readonly global::Friflo.Engine.ECS.EntityStore store = new(global::Friflo.Engine.ECS.PidType.UsePidAsId);

    internal global::Friflo.Engine.ECS.EntityStore Store => store;
    public void Dispose()
    {
    }
}
